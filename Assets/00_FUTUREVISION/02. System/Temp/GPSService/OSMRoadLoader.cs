/*  ───────────────────────────────────────────────────────────
    OSMRoadMeshLoader.cs
    - Overpass API로 highway=* 폴리라인을 받아
      폭(width/lanes/등급 기본값)에 따라 Mesh로 생성
    - 1 Unity unit = 1 meter 스케일
   ───────────────────────────────────────────────────────────*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class OSMRoadLoader : MonoBehaviour
{
    /* ────────── 인스펙터 설정 ────────── */
    [Header("Query Box (degrees)")]
    public double mapSize  = 0.01;                       // 위도·경도 범위
    public double centerLat = 37.261682181845856,
                  centerLon = 127.10887739484686;

    [Header("Road")]
    public GameObject roadParent;
    public Material roadMaterial;                        // 로드용 머티리얼
    public float yOffset = 0.01f;                        // 지면 뜨게 하고 싶을 때

    /* ────────── 등급별 기본 폭(미터) ────────── */
    readonly Dictionary<string, float> _defaultWidth = new()
    {
        {"motorway",20f},{"trunk",16f},{"primary",14f},
        {"secondary",12f},{"tertiary",10f},
        {"residential",8f},{"service",6f}
    };

    /* ────────── 계산용 프로퍼티 ────────── */
    double LatS => centerLat - mapSize / 2;
    double LonW => centerLon - mapSize / 2;
    double LatN => centerLat + mapSize / 2;
    double LonE => centerLon + mapSize / 2;

    [ContextMenu("Update Road")]
    public void UpdateRoad()
    {
        StartCoroutine(LoadAndBuild());
    }

    /* =======================================================================
       ① OSM 데이터 다운로드
       =======================================================================*/
    private IEnumerator LoadAndBuild()
    {
        string q = $@"[out:json][timeout:25];
                      way[""highway""]({LatS},{LonW},{LatN},{LonE});
                      (._;>;);
                      out body geom;";
        string url = "https://overpass-api.de/api/interpreter?data=" +
                     UnityWebRequest.EscapeURL(q);

        using var req = UnityWebRequest.Get(url);
        Debug.Log($"OSMRoadLoader: Start LoadAndBuild\n{url}", this);
        yield return req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.Success)
        { 
            Debug.LogWarning(req.error, this);
            // 재시작
            yield return new WaitForSeconds(1f);
            UpdateRoad();
            yield break;
        }

        /* -------------------------------------------------------------------
           ② JSON 파싱
           -------------------------------------------------------------------*/
        var json = JObject.Parse(req.downloadHandler.text);
        var nodes = new Dictionary<long, Vector3>();
        Debug.Log($"OSMRoadLoader: JSON Loaded\n{json}", this);
        foreach (var el in json["elements"])
            if ((string)el["type"] == "node")
            {
                long id = (long)el["id"];
                double lat = (double)el["lat"],
                       lon = (double)el["lon"];
                nodes[id] = GeoToUnity(lat, lon);
            }

        /* -------------------------------------------------------------------
           ③ 각 way → Quad Strip Mesh
           -------------------------------------------------------------------*/
        Debug.Log($"OSMRoadLoader: Build Meshes", this);
        int roadIdx = 0;
        foreach (var el in json["elements"])
            if ((string)el["type"] == "way")
            {
                var refs = el["geometry"] != null ? el["geometry"] : el["nodes"];
                var pts = new List<Vector3>();
                foreach (var n in refs)
                {
                    if (refs == el["geometry"])
                        pts.Add(GeoToUnity((double)n["lat"], (double)n["lon"]));
                    else
                        pts.Add(nodes[(long)n]);
                }

                float width = GetRoadWidth(el["tags"]);
                BuildRoadMesh($"road_{roadIdx++}", pts, width);
            }
    }

    /* =======================================================================
       ④ Mesh 생성 유틸
       =======================================================================*/
    private void BuildRoadMesh(string name, List<Vector3> polyline, float width)
    {
        if (polyline.Count < 2) return;

        var verts = new List<Vector3>();
        var uvs   = new List<Vector2>();
        var tris  = new List<int>();

        float half = width * 0.5f;

        for (int i = 0; i < polyline.Count; i++)
        {
            Vector3 p   = polyline[i];
            Vector3 dir = Vector3.zero;

            if (i == 0)                       dir = (polyline[i + 1] - p).normalized;
            else if (i == polyline.Count - 1) dir = (p - polyline[i - 1]).normalized;
            else
            {
                Vector3 dir1 = (p - polyline[i - 1]).normalized;
                Vector3 dir2 = (polyline[i + 1] - p).normalized;
                dir = ((dir1 + dir2) * .5f).normalized;
            }

            Vector3 normal = new Vector3(-dir.z, 0, dir.x); // 평면 좌우 벡터
            verts.Add(p + normal * half + Vector3.up * yOffset);
            verts.Add(p - normal * half + Vector3.up * yOffset);

            float v = i / (polyline.Count - 1f);
            uvs.Add(new Vector2(0, v));
            uvs.Add(new Vector2(1, v));

            if (i < polyline.Count - 1)
            {
                int idx = i * 2;
                tris.Add(idx);     tris.Add(idx + 2); tris.Add(idx + 1);
                tris.Add(idx + 1); tris.Add(idx + 2); tris.Add(idx + 3);
            }
        }

        Mesh m = new Mesh { name = name };
        m.SetVertices(verts);
        m.SetUVs(0, uvs);
        m.SetTriangles(tris, 0);
        m.RecalculateNormals();

        GameObject go = new GameObject(name);
        go.transform.SetParent(roadParent.transform, false);
        // 부모 레이어로 go 레이어 설정
        go.layer = roadParent.layer;
        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();
        mf.sharedMesh   = m;
        mr.sharedMaterial = roadMaterial;
    }

    /* =======================================================================
       ⑤ 보조 함수
       =======================================================================*/
    public Vector3 GeoToUnity(double lat, double lon)
    {
        const double meterPerDeg = 111_000.0;
        double dz = (lat - centerLat) * meterPerDeg;
        double dx = (lon - centerLon) * meterPerDeg * Mathf.Cos((float)(lat * Mathf.Deg2Rad));
        return new Vector3((float)dx, 0, (float)dz);
    }

    private float GetRoadWidth(JToken tags)
    {
        if (tags?["width"] != null)
        {
            string w = tags["width"].ToString().Replace("m", "").Trim();
            if (float.TryParse(w, out float meters) && meters > 0.5f) return meters;
        }
        if (tags?["lanes"] != null && int.TryParse(tags["lanes"].ToString(), out int lanes))
            return Mathf.Max(3.5f * lanes, 3f);

        string hw = tags?["highway"]?.ToString();
        if (hw != null && _defaultWidth.TryGetValue(hw, out float def)) return def;
        return 3f;
    }
}
