/*
 * 작성자: Kim Bummoo
 * 작성일: 2025.03.03
 *
 */

using Imagine.WebAR;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace FUTUREVISION.WebAR
{
    public enum CameraState
    {
        None,

        Front,
        Back,
    }

    public enum ARTrackerState
    {
        None,
        ScreenState,
        WorldState,
    }

    public class ARTrackerModel : BaseModel
    {
        [Header("AR Camera의 포지션과 이름이 하드코딩 되어있으니 변경하지 말것")]

        [Space(10)]
        [Header("Camera")]
        public ARCamera ARCamera;
        public ScreenshotManager ScreenshotManager;
        public GameObject Placement;

        [Space(10)]
        public Camera ScreenContentCamera;
        public Camera WorldContentsCamera;

        [Space(10)]
        [Header("Object")]
        public WorldTracker WorldTracker;

        [Space(10)]
        public GameObject ScreenController;
        public GameObject WorldController;

        [Space(10)]
        public GameObject ScreenObject;
        public GameObject WorldObject;

        [Space(10)]
        [Header("State")]
        public CameraState CameraState;
        public ARTrackerState ARTrackingState;

        [Space(10)]
        [Header("Event")]
        public UnityEvent<ARTrackerState> OnARTrackingStateChanged;

        public UnityEvent OnPlaced;
        public UnityEvent OnReset;
        public UnityEvent<bool> OnPlacementVisibilityChanged;
        //public UnityEvent<EScreenShotEventType> OnScreenShotEvent;

        private bool isPlacement = false;
        public bool IsPlacement => isPlacement;

        public override void Initialize()
        {
            base.Initialize();
        }

        public void SetCameraState(CameraState state)
        {
            CameraState = state;

            string settingParam = string.Empty;
            switch (state)
            {
                case CameraState.Front:
                    {
                        settingParam = "user";
                        ARCamera.isFlipped = true;
                    }
                    break;
                case CameraState.Back:
                    {
                        settingParam = "environment";
                        ARCamera.isFlipped = false;
                    }
                    break;
            }
            WebARManager.Instance.StartCoroutine(RestartCamera(settingParam));
        }

        IEnumerator RestartCamera(string settingParam)
        {
            Application.ExternalCall("StopWebcam");

            yield return new WaitForSeconds(0.5f);

            Application.ExternalCall("SetWebCamSetting", settingParam);
            Application.ExternalCall("StartWebcam");
        }

        public void StopCamera()
        {
            Application.ExternalCall("StopWebcam");
        }

        public void SetARTrackerState(ARTrackerState state)
        {
            ARTrackingState = state;

            switch (ARTrackingState)
            {
                case ARTrackerState.ScreenState:
                    WorldTracker.ResetOrigin();

                    // 카메라 전환
                    ScreenContentCamera.gameObject.SetActive(true);
                    WorldContentsCamera.gameObject.SetActive(false);

                    // 
                    ScreenController.SetActive(true);
                    WorldController.SetActive(false);

                    // 오브젝트 전환
                    ScreenObject.SetActive(true);
                    WorldObject.SetActive(false);
                    break;

                case ARTrackerState.WorldState:
                    WorldTracker.ResetOrigin();

                    // 카메라 전환
                    ScreenContentCamera.gameObject.SetActive(false);
                    WorldContentsCamera.gameObject.SetActive(true);

                    //
                    ScreenController.SetActive(false);
                    WorldController.SetActive(true);

                    // 오브젝트 전환
                    ScreenObject.SetActive(false);
                    WorldObject.SetActive(true);
                    break;
            }

            OnARTrackingStateChanged?.Invoke(ARTrackingState);
        }

        #region Getter

        public Camera GetScreenContentCamera()
        {
            return ScreenContentCamera;
        }

        public Camera GetWorldContentsCamera()
        {
            return WorldContentsCamera;
        }

        public CameraState GetCameraState()
        {
            return CameraState;
        }

        public ARTrackerState GetARTrackerState()
        {
            return ARTrackingState;
        }

        public GameObject GetCurruentObject()
        {
            switch (ARTrackingState)
            {
                case ARTrackerState.ScreenState:
                    return ScreenObject;
                case ARTrackerState.WorldState:
                    return WorldObject;
            }

            return null;
        }

        #endregion

        #region WorldTracker 제어

        public bool GetPlacementVisibility()
        {
            return Placement.activeSelf;
        }

        public void SetPlacement()
        {
            // AR 오브젝트를 배치합니다.
            WorldTracker.PlaceOrigin();
            isPlacement = true;

            OnPlaced?.Invoke();
        }

        public void ResetPlacement()
        {
            WorldTracker.ResetOrigin();
            isPlacement = false;

            OnReset?.Invoke();
        }

        public void TakeScreenShot()
        {
            // 스크린샷 매니저를 통해 스크린샷을 찍습니다. WebGL에서 정상작동 합니다.
            ScreenshotManager.GetScreenShot();
        }

        // WorldTracker 이벤트 콜백

        public void OnPlacementVisibilityChangedCallback(bool isShow)
        {
            OnPlacementVisibilityChanged?.Invoke(isShow);
        }

        public bool IsHorizontalWorld()
        {
            // 카메라가 수평인지 확인합니다.
            return Vector3.Angle(ARCamera.transform.up, Vector3.up) < 40.0f;
        }

        #endregion
    }
}
