using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Imagine.WebAR
{
    [System.Serializable]
    public class Settings6DOF
    {
        //public bool repositionOnTouch = false;

        public enum DepthMode { SCALE_AS_DEPTH, Z_AS_DEPTH_EXPERIMENTAL };
        public DepthMode depthMode = DepthMode.SCALE_AS_DEPTH;
        // public enum AngleMode {PRECISE_BUT_SKEWED, ACCURATE_BUT_LERPED}

        //public Vector3 camOffset;

        [Range(300, 600)] public int maxPixels = 450;
        [Range(10, 200)] public int maxPoints = 120;
        [Range(0, 1)] public float targetConfidence = 0.1f;

        [Space][Range(50, 250)] public int clampPixelDrift = 50;
        [Range(0.05f, 0.25f)] public float clampScaleDrift = 0.1f;

        [Space][Range(0.00001f, 0.001f)] public float minPointQuality = 0.0001f;
        [Range(0.001f, 0.1f)] public float minGoodPointQuality = 0.002f;
        [Range(1, 20)] public float minCoplanarFactor = 3;

        [Space][Range(.0001f, 0.01f)] public float angleSmoothFactor = 0.001f;
        // public AngleMode angleMode = AngleMode.PRECISE_BUT_SKEWED;
        [Range(0f, 0.05f)] public float angleDriftThreshold = 0.025f;

        [Space] public bool poseCorrectionEnabled = true;
        [Range(250, 1000)] public int poseCorrectionInterval = 500;

        public UnityEvent<float> OnConfidence;

        public bool use3DOFWhenOriginOutOfBounds = false;
        [Range(-0.25f, 0.5f)] public float outOfBoundsX = 0.5f;
        [Range(-0.25f, 0.5f)] public float outOfBoundsY = 0f;

    }

    public partial class WorldTracker
    {
        [DllImport("__Internal")] private static extern void WebGLPlaceOrigin(string camPosStr);
        [DllImport("__Internal")] private static extern void WebGLSetViewportPos(string vStr);



        private float startZ;


        void Awake_6DOF()
        {

        }

        void Start_6DOF()
        {
            var camPos = new Vector3(origPos.x, 0, origPos.z);
            startZ = (mainObject.transform.position - camPos).magnitude;

            var dmode = "";
            if(s6dof.depthMode == Settings6DOF.DepthMode.SCALE_AS_DEPTH)
                dmode = "SCALE";
            else if(s6dof.depthMode == Settings6DOF.DepthMode.Z_AS_DEPTH_EXPERIMENTAL)
                dmode = "Z";

            var json = "{";
            json += "\"MODE\":\"3DOF\","; //<--3DOF for placement mode only
            json += "\"DEPTHMODE\":\"" + dmode + "\",";
            json += "\"START_Z\":" + startZ.ToStringInvariantCulture() + ",";
            json += "\"CAM_START_HEIGHT\":" + cameraStartHeight.ToStringInvariantCulture() + ",";
            json += "\"ARM_LENGTH\":" + s3dof.armLength.ToStringInvariantCulture() + ",";
            json += "\"MAX_PIXELS\":" + s6dof.maxPixels + ",";
            json += "\"MAX_POINTS\":" + s6dof.maxPoints + ",";
            json += "\"TARGET_CONFIDENCE\":" + s6dof.targetConfidence.ToStringInvariantCulture() + ",";
            json += "\"CLAMP_PIXEL_DRIFT\":" + s6dof.clampPixelDrift + ",";
            json += "\"CLAMP_SCALE_DRIFT\":" + s6dof.clampScaleDrift.ToStringInvariantCulture() + ",";
            json += "\"ANGLE_SMOOTH_FACTOR\":" + s6dof.angleSmoothFactor.ToStringInvariantCulture() + ",";
            // json += "\"ANGLE_MODE\":\"" + s6dof.angleMode.ToString() + "\",";
            json += "\"ANGLE_DRIFT_THRESHOLD\":" + s6dof.angleDriftThreshold.ToStringInvariantCulture() + ",";
            json += "\"POSE_CORRECTION_ENABLED\":" + (s6dof.poseCorrectionEnabled ? "true" : "false") + ",";
            json += "\"POSE_CORRECTION_INTERVAL\":" + s6dof.poseCorrectionInterval + ",";
            json += "\"MIN_POINT_QUALITY\":" + s6dof.minPointQuality.ToStringInvariantCulture() + ",";
            json += "\"MIN_GOOD_POINT_QUALITY\":" + s6dof.minGoodPointQuality.ToStringInvariantCulture() + ",";
            json += "\"MIN_COPLANAR_FACTOR\":" + s6dof.minCoplanarFactor.ToStringInvariantCulture() + ",";
            json += "\"USE_COMPASS\":" + (useCompass?"true":"false") + ",";
            json += "\"USE_3DOF_WHEN_ORIGIN_OUT_OF_BOUNDS\":" + (s6dof.use3DOFWhenOriginOutOfBounds?"true":"false") + ",";
            json += "\"OUT_OF_BOUNDS_X\":" + s6dof.outOfBoundsX.ToStringInvariantCulture() + ",";
            json += "\"OUT_OF_BOUNDS_Y\":" + s6dof.outOfBoundsY.ToStringInvariantCulture();
            json += "}";
#if !UNITY_EDITOR && UNITY_WEBGL
            SetWebGLwTrackerSettings(json);
#endif

        }

        public void Update_6DOF()
        {
           if (usePlacementIndicator && !placementIndicatorSettings.placed)
            {
                Update_3DOF();
            }
        }

        void UpdateCameraTransform_6DOF(string data)
        {
            var ar = ((float)Screen.width) / Screen.height;

            if (usePlacementIndicator && !placementIndicatorSettings.placed)
            {
                UpdateCameraTransform_3DOF(data);
            }
            else
            {
                var vals = data.Split(new string[]{","}, System.StringSplitOptions.RemoveEmptyEntries);

                trackerCamRot.w = float.Parse(vals[0], CultureInfo.InvariantCulture);
                trackerCamRot.x = float.Parse(vals[1], CultureInfo.InvariantCulture);
                trackerCamRot.y = float.Parse(vals[2], CultureInfo.InvariantCulture);
                trackerCamRot.z = float.Parse(vals[3], CultureInfo.InvariantCulture);

                trackerCamPos.x = float.Parse(vals[4], CultureInfo.InvariantCulture);
                trackerCamPos.y = float.Parse(vals[5], CultureInfo.InvariantCulture);
                trackerCamPos.z = float.Parse(vals[6], CultureInfo.InvariantCulture);

                trackerCamera.transform.position = trackerCamPos;
                trackerCamera.transform.rotation = trackerCamRot;

                trackerMainObjectScale = float.Parse(vals[7], CultureInfo.InvariantCulture) * Vector3.one;
                mainObject.transform.localScale = trackerMainObjectScale;
            
                s6dof.OnConfidence?.Invoke(float.Parse(vals[8], CultureInfo.InvariantCulture));
            }

        }

        void Place_6DOF(){

            mainObject.transform.position = Vector3.zero;
            var startCamPos = new Vector3(origPos.x, 0, origPos.z);
            trackerCamera.transform.position = startCamPos;

#if UNITY_WEBGL && !UNITY_EDITOR
            var json = "{";
            json += "\"MODE\":\"6DOF\"" + ",";
            json += "\"START_Z\":" + startZ.ToStringInvariantCulture();
            json += "}";
            SetWebGLwTrackerSettings(json);
            var camPosStr = startCamPos.x + "," + startCamPos.y + "," + startCamPos.z;
            WebGLPlaceOrigin(camPosStr);     
            WebGLSetViewportPos((placementIndicatorSettings.placementOffset.x).ToStringInvariantCulture() + "," + (placementIndicatorSettings.placementOffset.y).ToStringInvariantCulture());
#endif
            
        }

        void Reset_6DOF()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            SetWebGLwTrackerSettings("{\"MODE\":\"3DOF\"}");
#endif
        }

    }
}