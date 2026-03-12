using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Imagine.WebAR.Demo{
    
    public class ChangeGPSPinPosition : MonoBehaviour
    {
        [SerializeField] GameObject changePinLocationPopup, pinListItemPrefab;
        [SerializeField] TextMeshProUGUI changePinLocationPopupTitle, yourLocationText, pinLocationText;
        [SerializeField] TMP_InputField latInput, lonInput;
        [SerializeField] Button placeButton, place10MButton, closeButton;
        List<GPSPin> pins = new List<GPSPin>();
        GPSPin currentPin;
        Dictionary<GPSPin, TextMeshProUGUI> pinListTitles = new Dictionary<GPSPin, TextMeshProUGUI>();
        

        private WorldTracker wt;


        private const float EarthRadius = 6371000;
        IEnumerator Start()
        {
            wt = FindObjectOfType<WorldTracker>();
            wt.geolocationSettings.OnGPSPositionUpdated.AddListener(UpdatePositions);
            closeButton.onClick.AddListener(HideEditPopup);
            placeButton.onClick.AddListener(()=>{
                if(currentPin != null) {
                    ChangePinLocation(currentPin);
                    HideEditPopup();
                }
            });
            place10MButton.onClick.AddListener(()=>{
                if(currentPin != null){
                    Add10MetersInFrontToGPS(currentPin);
                    HideEditPopup();
                }
            });

            yield return new WaitForEndOfFrame();
            InitializePinList();
        }

        public void InitializePinList(){
            foreach(var pin in wt.geolocationSettings.gpsPins){
                pins.Add(pin);
                var pinListItem = Instantiate(pinListItemPrefab, pinListItemPrefab.transform.parent);
                var pinListTitle = pinListItem.transform.Find("Title").GetComponent<TextMeshProUGUI>();
                pinListTitle.text = pin.id + " - " + "? m away";
                pinListTitles.Add(pin, pinListTitle);
                pinListItem.transform.Find("Edit Button").GetComponent<Button>().onClick.AddListener(()=>{
                    ShowEditPopup(pin);
                });
            }
            pinListItemPrefab.SetActive(false);
        }

        public void ShowEditPopup(GPSPin pin){
            changePinLocationPopup.SetActive(true);
            changePinLocationPopupTitle.text = "CHANGE " + pin.id + " LOCATION";

            currentPin = pin;
        }

        public void HideEditPopup(){
            changePinLocationPopup.SetActive(false);
            currentPin = null;
        }

        public void ChangePinLocation(GPSPin pin){
            var lat = Double.Parse(latInput.text);
            var lon = Double.Parse(lonInput.text);

            if(lat >= -90f && lat <= 90f && lon >= -180f && lon <= 180f){
                pin.latitude = lat;
                pin.longitude = lon;

                HideEditPopup();

                UpdatePositions(lastData);
            }
        }

        GPSData lastData;
        public void UpdatePositions(GPSData data){
            var myLat = data.latitude;
            var myLon = data.longitude;

            foreach(var pin in pins){
                var distance = CalculateDistance((float)myLat, (float)myLon, 0, (float)pin.latitude, (float)pin.longitude, 0);
                distance = Mathf.Round(distance / 5) * 5;
                var pinListTitle = pinListTitles[pin];
                var colorTag = distance > 1000 ? "<color=red>" : "<color=white>";
                pinListTitle.text = pin.id + " - " +  colorTag + distance + " meters</color> away";
            }

            lastData = data;
        }

        public void Add10MetersInFrontToGPS( GPSPin pin)
        {
            var cam = GameObject.FindObjectOfType<ARCamera>().transform;
            var camHeading = Quaternion.Euler(0, cam.eulerAngles.y, 0);
            Vector3 displacement = camHeading * new Vector3(0, 0, 10);
            double lat = lastData.latitude;
            double lon = lastData.longitude;
            double alt = lastData.altitude;

            // 1 degree of latitude in meters
            double metersPerLatDegree = 111320.0;

            // 1 degree of longitude in meters, varies based on latitude
            double metersPerLonDegree = 111320.0 * Math.Cos(lat * Math.PI / 180.0);

            pin.latitude = lat + (displacement.z / metersPerLatDegree);
            pin.longitude = lon + (displacement.x / metersPerLonDegree);
            pin.altitude = alt + displacement.y;

            UpdatePositions(lastData);
        }

        public static float CalculateDistance(float lat1, float lon1, float alt1, float lat2, float lon2, float alt2)
        {
            float lat1Rad = lat1 * Mathf.Deg2Rad;
            float lon1Rad = lon1 * Mathf.Deg2Rad;
            float lat2Rad = lat2 * Mathf.Deg2Rad;
            float lon2Rad = lon2 * Mathf.Deg2Rad;

            float deltaLat = lat2Rad - lat1Rad;
            float deltaLon = lon2Rad - lon1Rad;

            float a = Mathf.Sin(deltaLat / 2) * Mathf.Sin(deltaLat / 2) +
                    Mathf.Cos(lat1Rad) * Mathf.Cos(lat2Rad) *
                    Mathf.Sin(deltaLon / 2) * Mathf.Sin(deltaLon / 2);

            float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
            float horizontalDistance = EarthRadius * c;

            float altitudeDifference = alt2 - alt1;

            float totalDistance = Mathf.Sqrt(horizontalDistance * horizontalDistance + altitudeDifference * altitudeDifference);

            return totalDistance;
        }
    }
}
