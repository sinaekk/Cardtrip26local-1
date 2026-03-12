using UnityEngine;
using UnityEngine.UI;

public class LocationPin : MonoBehaviour
{
    public OSMRoadLoader roadLoader;
    public double latitude;
    public double longitude;

    [Space(10)]
    public Button button;
    public Image spotImage;

    // Update is called once per frame
    void Update()
    {
        var position = roadLoader.GeoToUnity(latitude, longitude);
        this.transform.localPosition = position;
    }
}
