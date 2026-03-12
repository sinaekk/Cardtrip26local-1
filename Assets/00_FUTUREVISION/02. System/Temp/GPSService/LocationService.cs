using System.Collections.Generic;
using UnityEngine;

public class LocationService : FUTUREVISION.BaseModel
{
    public int AVERAGE_COUNT = 60;

    [Header("LocationService")]
    public double Latitude  = 37.261682181845856;
    public double Longitude = 127.10887739484686;
    public List<float> LatitudeHistory  = new();
    public List<float> LongitudeHistory = new();

    [Header("Compass")]
    public float CompassRotation = 0f;
    public List<float> CompassHistory = new();

    private bool isLocationServiceStarted = false;
    public bool IsLocationServiceEnabled => isLocationServiceStarted;

    // ✅ WebGL에서 권한은 '사용자 제스처'가 필요하므로, UI 버튼 등에서 이 함수를 호출하세요.
    public void StartSensors()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        WebGLLocationBridge.StartSensors();
        isLocationServiceStarted = true;   // 권한 획득 전이라도 UI 상태 표시에 활용
#else
        // 네이티브 경로
        Input.location.Start(1f, 0.1f);
        Input.compass.enabled = true;
        isLocationServiceStarted = true;
#endif
    }

    public void StopSensors()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        WebGLLocationBridge.StopSensors();
#else
        Input.location.Stop();
        Input.compass.enabled = false;
#endif
        isLocationServiceStarted = false;
        LatitudeHistory.Clear();
        LongitudeHistory.Clear();
        CompassHistory.Clear();
    }

    public override void Initialize()
    {
        base.Initialize();
        // ❗ WebGL은 여기서 자동 시작하지 말고, 반드시 버튼/클릭으로 StartSensors 호출
#if !UNITY_WEBGL || UNITY_EDITOR
        // 에디터/네이티브만 즉시 시작
        StartSensors();
#endif
    }

    public void FixedUpdate()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL: JS에서 폴링
        if (!isLocationServiceStarted) return;

        if (WebGLLocationBridge.HasGeo())
        {
            float newLat = (float)WebGLLocationBridge.GetLatitude();
            float newLon = (float)WebGLLocationBridge.GetLongitude();
            UpdateAveragedLatLon(newLat, newLon);
        }

        if (WebGLLocationBridge.HasHeading())
        {
            UpdateCompass(WebGLLocationBridge.GetHeading());
        }
#else
        // 에디터/네이티브
        if (Application.isEditor)
        {
            isLocationServiceStarted = true;
        }
        else
        {
            if (Input.location.status != LocationServiceStatus.Running) return;
        }

        // 위치
        float newLatitude  = (float)Input.location.lastData.latitude;
        float newLongitude = (float)Input.location.lastData.longitude;
        if (newLatitude != 0 && newLongitude != 0)
        {
            isLocationServiceStarted = true;
            UpdateAveragedLatLon(newLatitude, newLongitude);
        }

        // 나침반
        UpdateCompass(Input.compass.trueHeading);
#endif
    }

    private void UpdateAveragedLatLon(float newLat, float newLon)
    {
        LatitudeHistory.Add(newLat);
        if (LatitudeHistory.Count > AVERAGE_COUNT) LatitudeHistory.RemoveAt(0);

        LongitudeHistory.Add(newLon);
        if (LongitudeHistory.Count > AVERAGE_COUNT) LongitudeHistory.RemoveAt(0);

        double avgLat = 0, avgLon = 0;
        foreach (var v in LatitudeHistory)  avgLat += v / LatitudeHistory.Count;
        foreach (var v in LongitudeHistory) avgLon += v / LongitudeHistory.Count;
        Latitude  = avgLat;
        Longitude = avgLon;
    }

    private void UpdateCompass(float headingDeg)
    {
        CompassHistory.Add(headingDeg);
        if (CompassHistory.Count > AVERAGE_COUNT) CompassHistory.RemoveAt(0);

        float sumX = 0f, sumY = 0f;
        foreach (var v in CompassHistory)
        {
            float rad = v * Mathf.Deg2Rad;
            sumX += Mathf.Cos(rad);
            sumY += Mathf.Sin(rad);
        }
        float avgX = sumX / CompassHistory.Count;
        float avgY = sumY / CompassHistory.Count;

        float angle = Mathf.Atan2(avgY, avgX) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        CompassRotation = angle;
    }
}
