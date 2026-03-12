using UnityEngine;

public class PlayerGPS : MonoBehaviour
{
    [Header("PlayerGPS")]
    public LocationService LocationService;
    public OSMRoadLoader OsmRoadLoader;
    
    public Transform PlayerTransform;

    // Update is called once per frame
    void Update()
    {
        if (LocationService != null && PlayerTransform != null && LocationService.IsLocationServiceEnabled)
        {
            // 위도·경도 정보를 이용해 플레이어 위치를 갱신
            double lat = LocationService.Latitude;
            double lon = LocationService.Longitude;

            var pos = OsmRoadLoader.GeoToUnity(lat, lon);
            float x = pos.x;
            float z = pos.z;

            PlayerTransform.position = new Vector3(x, PlayerTransform.position.y, z);

            // 나침반 방향 갱신
            float heading = LocationService.CompassRotation;
            PlayerTransform.localRotation = Quaternion.Euler(0, 0, -heading);
        }
    }
}
