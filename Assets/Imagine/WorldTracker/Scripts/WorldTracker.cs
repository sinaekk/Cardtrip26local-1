using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Runtime.InteropServices;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif


namespace Imagine.WebAR
{
    [System.Serializable]
    public class PlacementIndicatorSettings
    {
        [HideInInspector] public Vector2 placementOffset;
        public float minZ = 0.5f, maxZ = 5;

        public GameObject placementIndicator;
        public bool placed = false;

        [Space()]
        public UnityEvent OnPlacementIndicatorShown, OnPlacementIndicatorHidden;
    }

    [System.Serializable]
    public class EventSettings{
        public UnityEvent OnPlacedOrigin, OnResetOrigin;
        public List<GameObject> ShowGameObjectsWhenPlaced = new List<GameObject>();
        public List<GameObject> ShowGameObjectsWhenReset = new List<GameObject>();
    }

    public partial class WorldTracker : MonoBehaviour
    {
        [DllImport("__Internal")] private static extern void WebGLResetOrigin();
        [DllImport("__Internal")] private static extern void StartWebGLwTracker(string name);
        [DllImport("__Internal")] private static extern void StopWebGLwTracker();
        [DllImport("__Internal")] private static extern void SetWebGLwTrackerSettings(string settings);
        [DllImport("__Internal")] private static extern bool IsWebGLwTrackerReady();

        [SerializeField] ARCamera trackerCamera;

        public enum TrackingMode { MODE_3DOF, MODE_6DOF, MODE_3DOF_ORBIT }
        [SerializeField] public TrackingMode mode;

        public enum PlaneMode {HORIZONTAL, VERTICAL_EXPERIMENTAL}
        [SerializeField] public PlaneMode planeMode;

        [SerializeField] public Settings3DOF s3dof;
        [SerializeField] public Settings3DOFOrbit s3dof_orbit;
        [SerializeField] public Settings6DOF s6dof;

        [SerializeField] public GameObject mainObject;

        [SerializeField] [Range(50,200)] private float debugCamMoveSensitivity = 100;
        [SerializeField] [Range(50,200)] private float debugCamTiltSensitivity = 100;

        [SerializeField] private float cameraStartHeight = 1.25f;
        [SerializeField] private bool usePlacementIndicator = true;
        [SerializeField] private PlacementIndicatorSettings placementIndicatorSettings;
        [SerializeField] public EventSettings eventSettings;

        [SerializeField] public bool useCompass = false;


        private Vector3 origPos, origScale;
        private Quaternion origRot;

        private Vector3 trackerCamPos, trackerMainObjectScale;
        private Quaternion trackerCamRot;
        private Vector2 trackerCamSSPos;

        private void Awake()
        {
            if(trackerCamera == null){
                trackerCamera = FindObjectOfType<ARCamera>();
            }

            var camPos = trackerCamera.transform.position;
            camPos.y = cameraStartHeight;
            trackerCamera.transform.position = camPos;
            //Debug.Log(trackerCamera.transform.position);

            origPos = trackerCamera.transform.position;
            origRot = trackerCamera.transform.rotation;
            origScale = mainObject.transform.localScale;

            targetPos3DOF = origPos;
            targetRot3DOF = origRot;

            if (mode == TrackingMode.MODE_3DOF)
            {
                Awake_3DOF();
            }

            else if (mode == TrackingMode.MODE_3DOF_ORBIT)
            {
                Awake_3DOF_Orbit();
            }

            else if (mode == TrackingMode.MODE_6DOF)
            {
                Awake_6DOF();
            }

        }

        IEnumerator Start()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            while(!IsWebGLwTrackerReady()){
                yield return new WaitForEndOfFrame();
            }

            StartWebGLwTracker(name);
#endif

            if (mode == TrackingMode.MODE_3DOF)
            {
                Start_3DOF();
            }

            else if (mode == TrackingMode.MODE_3DOF_ORBIT)
            {
                Start_3DOF_Orbit();
            }

            else if (mode == TrackingMode.MODE_6DOF)
            {
                Start_6DOF();
            }

            ResetOrigin();

            StartGPS();

            yield break;
        }

        void OnDestroy()
        {
            //ResetOrigin();

#if UNITY_WEBGL && !UNITY_EDITOR
            StopWebGLwTracker();
#endif
            OnDestroyGPS();
        }

        void Update()
        {

            if (usePlacementIndicator && !placementIndicatorSettings.placed)
            {
                UpdatePlacementIndicator();
            }

            if (mode == TrackingMode.MODE_3DOF)
            {
                Update_3DOF();
            }
            else if (mode == TrackingMode.MODE_3DOF_ORBIT)
            {
                Update_3DOF_Orbit();
            }
            else if (mode == TrackingMode.MODE_6DOF)
            {
                Update_6DOF();
            }

            //Geolocation
            if(useGeolocation){
                UpdateGPS();
            }

            //Debug
#if UNITY_EDITOR
            if(useGeolocation){
                Update_DebugGeolocation();
            }
            else{
                 Update_Debug();
            }
#else
            Update_Debug();
#endif
            
        }

        void UpdatePlacementIndicator()
        {
            var ps = placementIndicatorSettings;

            //move this transform along z, in front of the camera
            var camPos = trackerCamera.transform.position;
            var colliderPlane = new Plane();
            if(planeMode == PlaneMode.HORIZONTAL){
                camPos.y = 0;
                //xz plane
                colliderPlane = new Plane(Vector3.up, camPos);
                placementIndicatorSettings.placementIndicator.transform.eulerAngles = Vector3.zero;
                // mainObject.transform.GetChild(0).eulerAngles = new Vector3(0, 0, 0);
            }
            else if(planeMode == PlaneMode.VERTICAL_EXPERIMENTAL){
                camPos.z += startZ;
                //xy plane
                colliderPlane = new Plane(Vector3.back, camPos);
                placementIndicatorSettings.placementIndicator.transform.eulerAngles = new Vector3(-90, 0, 0);
                // mainObject.transform.GetChild(0).eulerAngles = new Vector3(-90, 0, 0);

            }

            var ray = new Ray(trackerCamera.transform.position, trackerCamera.transform.forward);
            Debug.DrawRay(ray.origin, ray.direction, Color.magenta, 1);
            
            float enter = 0;
            if (colliderPlane.Raycast(ray, out enter))
            {
                //Debug.Log("hit!");

                if (!ps.placementIndicator.activeSelf)
                    ps.OnPlacementIndicatorShown?.Invoke();

                ps.placementIndicator.SetActive(true);

                var hitPoint = ray.GetPoint(enter);

                var d = Vector3.Distance(hitPoint, camPos);

                if (d > ps.minZ && d < ps.maxZ)
                {
                    //mainObject.transform.position = hitPoint;
                    ps.placementIndicator.transform.position = hitPoint;
                }
                else if (d < ps.minZ)
                {
                    //mainObject.transform.position = camPos + Vector3.Normalize(hitPoint - camPos) * ps.minZ;
                    ps.placementIndicator.transform.position = camPos + Vector3.Normalize(hitPoint - camPos) * ps.minZ;
                }
                else if (d > ps.maxZ)
                {
                    //mainObject.transform.position = camPos + Vector3.Normalize(hitPoint - camPos) * ps.maxZ;
                    ps.placementIndicator.transform.position = camPos + Vector3.Normalize(hitPoint - camPos) * ps.maxZ;
                    //TODO: BUG HERE - Object is off-center
                }
                //ps.placementIndicator.transform.position = mainObject.transform.position;

                ps.placementOffset = trackerCamera.cam.WorldToViewportPoint(ps.placementIndicator.transform.position);
                //Debug.Log(ps.placementOffset);
            }
            else
            {
                //Debug.Log("no hit!");

                if (ps.placementIndicator.activeSelf)
                    ps.OnPlacementIndicatorHidden?.Invoke();
                ps.placementIndicator.SetActive(false);
            }
        }

        public void Update_Debug()
        {   
#if ENABLE_INPUT_SYSTEM  
            var x_left    = Keyboard.current != null && Keyboard.current.aKey.isPressed;
            var x_right   = Keyboard.current != null && Keyboard.current.dKey.isPressed;
            var z_forward = Keyboard.current != null && Keyboard.current.wKey.isPressed;
            var z_back    = Keyboard.current != null && Keyboard.current.sKey.isPressed;
            var y_up      = Keyboard.current != null && Keyboard.current.rKey.isPressed;
            var y_down    = Keyboard.current != null && Keyboard.current.fKey.isPressed;

#else
            var x_left    = Input.GetKey(KeyCode.A);
            var x_right   = Input.GetKey(KeyCode.D);
            var z_forward = Input.GetKey(KeyCode.W);
            var z_back    = Input.GetKey(KeyCode.S);
            var y_up      = Input.GetKey(KeyCode.R);
            var y_down    = Input.GetKey(KeyCode.F);
#endif

            float speed = 0.025f * Time.deltaTime * debugCamMoveSensitivity;
            float dx = (x_right   ? speed : 0) + (x_left ? -speed : 0);
            float dy = (y_up      ? speed : 0) + (y_down ? -speed : 0);
            float dz = (z_forward ? speed : 0) + (z_back ? -speed : 0);

            //float dsca = 1 + (z_forward ? speed : 0) + (z_back ? -speed : 0);

#if ENABLE_INPUT_SYSTEM  
            var y_rot_left  = Keyboard.current != null && Keyboard.current.leftArrowKey.isPressed;
            var y_rot_right = Keyboard.current != null && Keyboard.current.rightArrowKey.isPressed;
            var x_rot_up    = Keyboard.current != null && Keyboard.current.upArrowKey.isPressed;
            var x_rot_down  = Keyboard.current != null && Keyboard.current.downArrowKey.isPressed;
            var z_rot_cw    = Keyboard.current != null && Keyboard.current.commaKey.isPressed;
            var z_rot_ccw   = Keyboard.current != null && Keyboard.current.periodKey.isPressed;

#else
            var y_rot_left  = Input.GetKey(KeyCode.LeftArrow);
            var y_rot_right = Input.GetKey(KeyCode.RightArrow);
            var x_rot_up    = Input.GetKey(KeyCode.UpArrow);
            var x_rot_down  = Input.GetKey(KeyCode.DownArrow);
            var z_rot_cw    = Input.GetKey(KeyCode.Comma);
            var z_rot_ccw   = Input.GetKey(KeyCode.Period);
#endif

            var angularSpeed = 0.5f * Time.deltaTime * debugCamTiltSensitivity; //degrees per frame
            var d_rotx = (x_rot_up    ? angularSpeed : 0) + (x_rot_down ? -angularSpeed : 0);
            var d_roty = (y_rot_right ? angularSpeed : 0) + (y_rot_left ? -angularSpeed : 0);
            var d_rotz = (z_rot_ccw   ? angularSpeed : 0) + (z_rot_cw   ? -angularSpeed : 0);

            var w = trackerCamera.transform.rotation.w;
            var i = trackerCamera.transform.rotation.x;
            var j = trackerCamera.transform.rotation.y;
            var k = trackerCamera.transform.rotation.z;

            var rot = new Quaternion(i, j, k, w);

            var dq = Quaternion.Euler(d_rotx, d_roty, d_rotz);
            rot *= dq;
            //rot *= Quaternion.AngleAxis(d_rotz, trackerCamera.transform.forward);
            //rot *= Quaternion.AngleAxis(d_roty, trackerCamera.transform.up);
            //rot *= Quaternion.AngleAxis(d_rotx, trackerCamera.transform.right);

            //Debug.Log(dx + "," + dy + "," + dsca);
            trackerCamera.transform.rotation = rot;
            var dp = trackerCamera.transform.right * dx + trackerCamera.transform.up * dy + trackerCamera.transform.forward * dz;
            trackerCamera.transform.position += dp;
        }

        public void SetCameraFov(float fov)
        {
            trackerCamera.GetComponent<Camera>().fieldOfView = fov;
        }

        public void StartTracker(){
            StartWebGLwTracker(gameObject.name);
        }

        public void StopTracker(){
            StopWebGLwTracker();
        }

        public void PlaceOrigin()
        {
            //ResetOrigin();
            if (usePlacementIndicator)
            {
                var ps = placementIndicatorSettings;
                if (ps.placed)
                {
                    Debug.LogError("Origin is already placed. Call ResetOrigin() first");
                    return;
                }

                
                if(mode == TrackingMode.MODE_3DOF){
                    Place_3DOF(ps.placementIndicator.transform.position);
                }
                else if(mode == TrackingMode.MODE_6DOF){
                    Place_6DOF();
                }

                ps.placed = true;
                ps.placementIndicator.SetActive(false);
                mainObject.SetActive(true);
                //mainObject.transform.LookAt(new Vector3(trackerCamera.transform.position.x, 0, trackerCamera.transform.position.z), Vector3.up);



            }

            eventSettings.OnPlacedOrigin?.Invoke();
            foreach(var g in eventSettings.ShowGameObjectsWhenPlaced){
                g.gameObject.SetActive(true);
            }
            foreach(var g in eventSettings.ShowGameObjectsWhenReset){
                g.gameObject.SetActive(false);
            }

            StartCoroutine(WaitAndInvoke(0.25f, ()=>{
                //Billboard the object to always face the camera
                mainObject.transform.LookAt(new Vector3(trackerCamera.transform.position.x, 0, trackerCamera.transform.position.z), Vector3.up);
            }));

            trackerCamPos = origPos;
            trackerCamRot = origRot;
            trackerMainObjectScale = origScale;
            
        }

        public void ResetOrigin()
        {
            trackerCamera.transform.rotation = origRot;
            trackerCamera.transform.position = origPos;


            if (mode == TrackingMode.MODE_3DOF)
            {
                Reset_3DOF();
            }
            else if (mode == TrackingMode.MODE_3DOF_ORBIT)
            {
                Reset_3DOF_Orbit();
            }
            else if (mode == TrackingMode.MODE_6DOF)
            {
                Reset_6DOF();
            }


            if (usePlacementIndicator)
            {
                var ps = placementIndicatorSettings;
                if (!ps.placed)
                {
                    Debug.LogError("Origin not placed. Call PlaceOrigin() first");
                    return;
                }

                ps.placed = false;
                ps.placementIndicator.SetActive(true);
                mainObject.SetActive(false);
            }
            else{
                if(mode == TrackingMode.MODE_6DOF){
                    var camPos = new Vector3(origPos.x, 0, origPos.z);
                    trackerCamera.transform.position = camPos;
                }
            }


#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLResetOrigin();
#endif

            eventSettings.OnResetOrigin?.Invoke();
            foreach(var g in eventSettings.ShowGameObjectsWhenPlaced){
                if(g == null)
                {
                    Debug.LogError("A null object ignored in ShowGameObjectsWhenPlaced list");
                    continue;
                }
                g.gameObject.SetActive(false);
            }
            foreach(var g in eventSettings.ShowGameObjectsWhenReset){
                if(g == null)
                {
                    Debug.LogError("A null object ignored in ShowGameObjectsWhenReset list");
                    continue;
                }
                //Debug.Log(g.name);
                g.gameObject.SetActive(true);
            }

        }

        IEnumerator WaitAndInvoke(float delay, UnityAction action){
            yield return new WaitForSeconds(delay);
            action.Invoke();
        }

    }

}
