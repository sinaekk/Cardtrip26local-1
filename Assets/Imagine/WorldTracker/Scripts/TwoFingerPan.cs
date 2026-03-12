using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Imagine.WebAR
{
    public class TwoFingerPan : MonoBehaviour
    {
        [DllImport("__Internal")] private static extern void WebGLSetViewportPos(string vStr);

        [SerializeField] Transform panTransform;
        [SerializeField] Camera cam;
        Vector3 origPos;// startPos;
        Vector2 startViewportPos;
        Vector2 touch0StartPos, touch1StartPos;

        WorldTracker wt;

        // [SerializeField] float sensitivity = 0.01f;

        bool isPanning = false;

        void Start()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            Input.multiTouchEnabled = true;
#endif
            origPos = panTransform.localPosition;

            wt = FindObjectOfType<WorldTracker>();
        }


#if ENABLE_INPUT_SYSTEM
        void Update()
        {
            if(!wt.mainObject.activeInHierarchy)
                return;

            if (Touchscreen.current != null)
            {
                var activeTouchCount = 0;

                for (int i = 0; i < Touchscreen.current.touches.Count; i++)
                {
                    var touchPhase = Touchscreen.current.touches[i].phase.ReadValue();
                    if (touchPhase == UnityEngine.InputSystem.TouchPhase.Began || 
                        touchPhase == UnityEngine.InputSystem.TouchPhase.Moved ||
                        touchPhase == UnityEngine.InputSystem.TouchPhase.Stationary)
                    {
                        activeTouchCount++;
                    }
                }

                if(activeTouchCount < 2){
                    return;
                }

            }

            if (Touchscreen.current != null)
            {
                var touch0 = Touchscreen.current.touches[0];
                var touch1 = Touchscreen.current.touches[1];
            
                if (touch0.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began || 
                    touch1.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    touch0StartPos = touch0.position.ReadValue();
                    touch1StartPos = touch1.position.ReadValue();
                    isPanning = true;

                    // startPos = panTransform.localPosition;
                    startViewportPos = cam.WorldToViewportPoint(panTransform.position);

                    Debug.Log("Start Pan");
                }
                else if (touch0.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended ||
                    touch1.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended ||
                    touch0.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled ||
                    touch1.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled)
                {
                    isPanning = false;
                    Debug.Log("End Pan");
                }

                if (isPanning)
                {
                    var centerStart = (touch1StartPos + touch0StartPos) / 2;

                    var pos0 = touch0.position.ReadValue();
                    var pos1 = touch1.position.ReadValue();
                    var centerEnd = (pos1 + pos0) / 2;

                    var offset = centerEnd - centerStart;
                    offset.x /= Screen.width;
                    offset.y /= Screen.height;

                    var newViewportPos = startViewportPos + offset;
                    WebGLSetViewportPos(
                        newViewportPos.x.ToStringInvariantCulture() + "," + 
                        newViewportPos.y.ToStringInvariantCulture()
                    );

                    // var worldOffset = cam.right * offset.x + cam.up * offset.y;
                    // panTransform.localPosition = startPos + worldOffset * sensitivity * -1;
                    // //Debug.Log("Move " + offset + "->" + worldOffset);
                }


            }
            else
            {
                isPanning = false;
            }

        }

#else

        void Update()
        {
            if(!wt.mainObject.activeInHierarchy)
                return;

            if (Input.touchCount == 2)
            {
                var touch0 = Input.GetTouch(0);
                var touch1 = Input.GetTouch(1);

                if (touch0.phase == TouchPhase.Began ||
                touch1.phase == TouchPhase.Began)
                {
                    touch0StartPos = touch0.position;
                    touch1StartPos = touch1.position;
                    isPanning = true;

                    // startPos = panTransform.localPosition;
                    startViewportPos = cam.WorldToViewportPoint(panTransform.position);

                    Debug.Log("Start Pan");
                }
                else if (touch0.phase == TouchPhase.Ended ||
                    touch1.phase == TouchPhase.Ended ||
                    touch0.phase == TouchPhase.Canceled ||
                    touch1.phase == TouchPhase.Canceled)
                {
                    isPanning = false;
                    Debug.Log("End Pan");
                }

                if (isPanning)
                {
                    var centerStart = (touch1StartPos + touch0StartPos) / 2;

                    var pos0 = touch0.position;
                    var pos1 = touch1.position;
                    var centerEnd = (pos1 + pos0) / 2;

                    var offset = centerEnd - centerStart;
                    offset.x /= Screen.width;
                    offset.y /= Screen.height;

                    var newViewportPos = startViewportPos + offset;
                    WebGLSetViewportPos(
                        newViewportPos.x.ToStringInvariantCulture() + "," +
                        newViewportPos.y.ToStringInvariantCulture()
                    );

                    // var worldOffset = cam.right * offset.x + cam.up * offset.y;
                    // panTransform.localPosition = startPos + worldOffset * sensitivity * -1;
                    // //Debug.Log("Move " + offset + "->" + worldOffset);
                }


            }
            else
            {
                isPanning = false;
            }

        }
#endif

        public void Reset()
        {
            panTransform.localPosition = origPos;
        }
    }
}
