using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FUTUREVISION.Content
{
    public class LocationView : BaseView
    {
        [Header("LocationView/Location")]
        public LocationService LocationService;
        public OSMRoadLoader RoadLoader;
        public GameObject UserLocationMarker;


        [Header("Camera/Input")]
        public Transform CameraRoot;
        public Camera LocationCamera;
        public EventTrigger LocationCameraContainer;      // 입력 받는 영역(선택)

        public Vector3 LastCameraRotation = new Vector3(25f, 0f, 0f);
        public float zoomRatio = 0f;
        public List<PointerEventData> ongoingTouches = new List<PointerEventData>();

        private Vector2[] touchLastPos = new Vector2[2];

        //
        [Header("LocationView/Marker")]
        public LocationPin LocationPinPrefab;
        public List<LocationPin> LocationPins = new List<LocationPin>();

        public override void Initialize()
        {
            base.Initialize();

            InitializeLocation();
            InitializeCamera();
        }

        protected override void Start()
        {
            base.Start();

            StartCoroutine(StartLocationService());
        }

        #region Location
        private void InitializeLocation()
        {
            LocationService.Initialize();
        }

        private IEnumerator StartLocationService()
        {
            // 위치 서비스 사용 가능 여부 대기
            Debug.Log("LocationView: StartLocationService");
            // LocationService.StartSensors();

            // while (!LocationService.IsLocationServiceEnabled)
            // {
            //     yield return null;
            // }

            // 1초 대기
            yield return new WaitForSeconds(1f);

            // OSM Road Loader 시작
            RoadLoader.centerLat = LocationService.Latitude;
            RoadLoader.centerLon = LocationService.Longitude;
            RoadLoader.UpdateRoad();
        }
        #endregion

        #region Camera
        private void InitializeCamera()
        {
            CameraRoot.localRotation = Quaternion.Euler(LastCameraRotation);

            // Begin Drag
            EventTrigger.Entry beginDragEntry = new EventTrigger.Entry();
            beginDragEntry.eventID = EventTriggerType.BeginDrag;
            beginDragEntry.callback.AddListener(OnBeginDragCamera);
            LocationCameraContainer.triggers.Add(beginDragEntry);

            // Drag
            EventTrigger.Entry dragEntry = new EventTrigger.Entry();
            dragEntry.eventID = EventTriggerType.Drag;
            dragEntry.callback.AddListener(OnDragCamera);
            LocationCameraContainer.triggers.Add(dragEntry);

            // End Drag
            EventTrigger.Entry endDragEntry = new EventTrigger.Entry();
            endDragEntry.eventID = EventTriggerType.EndDrag;
            endDragEntry.callback.AddListener(OnEndDragCamera);
            LocationCameraContainer.triggers.Add(endDragEntry);

            // OnClick
            EventTrigger.Entry clickEntry = new EventTrigger.Entry();
            clickEntry.eventID = EventTriggerType.PointerClick;
            clickEntry.callback.AddListener(OnPointerClick);
            LocationCameraContainer.triggers.Add(clickEntry);
        }

        private void OnPointerClick(BaseEventData data)
        {
            Debug.Log("LocationView: OnClick LocationCameraContainer");
            // Camera로 이벤트 전달
            PointerEventData pointerData = data as PointerEventData;

            // UI를 건너뛰고 LocationCamera에 Raycast로 이벤트 전달
            Ray ray = LocationCamera.ScreenPointToRay(pointerData.position);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                Debug.Log($"LocationView: Raycast Hit {hitInfo.collider.gameObject.name}");
                ExecuteEvents.Execute(hitInfo.collider.gameObject, pointerData, ExecuteEvents.pointerClickHandler);
            }
        }

        private void OnBeginDragCamera(BaseEventData data)
        {
            PointerEventData pointerData = data as PointerEventData;
            var existingTouch = ongoingTouches.Find(t => t.pointerId == pointerData.pointerId);
            if (existingTouch == null)
            {
                ongoingTouches.Add(pointerData);
                CacheTouchPositions();
            }
        }

        private void OnDragCamera(BaseEventData data)
        {
            PointerEventData pointerData = data as PointerEventData;
            var existingTouch = ongoingTouches.Find(t => t.pointerId == pointerData.pointerId);
            if (existingTouch != null)
            {
                int index = ongoingTouches.IndexOf(existingTouch);

                if (ongoingTouches.Count == 1)
                {
                    Vector2 delta = pointerData.position - touchLastPos[0];
                    float rotationSpeed = 0.1f;

                    LastCameraRotation.y += delta.x * rotationSpeed;
                    LastCameraRotation.x -= delta.y * rotationSpeed;
                    LastCameraRotation.x = Mathf.Clamp(LastCameraRotation.x, 10f, 80f);

                    CameraRoot.localRotation = Quaternion.Euler(LastCameraRotation);

                    touchLastPos[0] = pointerData.position;
                }
                else if (ongoingTouches.Count == 2)
                {
                    // 평균값 구할 거 없이, 1번 또는 2번 손가락에대해 처리해도 됨
                    Vector2 prevPos = touchLastPos[index];
                    Vector2 currPos = pointerData.position;
                    float prevDistance = Vector2.Distance(touchLastPos[0], touchLastPos[1]);
                    float currDistance = Vector2.Distance(
                        index == 0 ? currPos : touchLastPos[0],
                        index == 1 ? currPos : touchLastPos[1]);
                    float distanceDelta = (currDistance - prevDistance) / Screen.dpi; // 인치 단위로 보정
                    float zoomSpeed = 0.1f;
                    zoomRatio += distanceDelta * zoomSpeed;
                    zoomRatio = Mathf.Clamp(zoomRatio, 0f, 1f);

                    float newCameraDistance = Mathf.Lerp(zoomRatio, -100f, -1000f);
                }
            }
        }

        private void OnEndDragCamera(BaseEventData data)
        {
            PointerEventData pointerData = data as PointerEventData;
            var existingTouch = ongoingTouches.Find(t => t.pointerId == pointerData.pointerId);
            if (existingTouch != null)
            {
                ongoingTouches.Remove(existingTouch);
                CacheTouchPositions();
            }
        }

        private void CacheTouchPositions()
        {
            touchLastPos[0] = Vector2.zero;
            touchLastPos[1] = Vector2.zero;

            if (ongoingTouches.Count >= 1)
            {
                touchLastPos[0] = (Vector2)ongoingTouches[0].position;
            }
            if (ongoingTouches.Count >= 2)
            {
                touchLastPos[1] = (Vector2)ongoingTouches[1].position;
            }
        }
        #endregion
    }
}
