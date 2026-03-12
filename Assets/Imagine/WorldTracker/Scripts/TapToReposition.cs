using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Runtime.InteropServices;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif


namespace Imagine.WebAR{
    public class TapToReposition : MonoBehaviour
    {
        [DllImport("__Internal")] private static extern void WebGLSetViewportPos(string vStr);

        private Vector2 startTapPos;
        private WorldTracker wt;
        private bool clicking = false;

        void Start()
        {
            wt = FindObjectOfType<WorldTracker>();
        }
#if ENABLE_INPUT_SYSTEM
        void Update()
        {
            if( EventSystem.current.IsPointerOverGameObject() ||
                !wt.mainObject.activeInHierarchy
            ){
                return;
            }

            if ((Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
                (Touchscreen.current != null && Touchscreen.current.primaryTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began))
            {
                if (Application.isMobilePlatform && Touchscreen.current != null){
                    startTapPos = Touchscreen.current.primaryTouch.position.ReadValue();
                }
                
                else if(Mouse.current != null)
                {
                    startTapPos = Mouse.current.position.ReadValue();
                }
                clicking = true;
            }

            else if (clicking && ((Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame) ||
                (Touchscreen.current != null && Touchscreen.current.primaryTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended)))
            {
                var endTapPos = new Vector2();

                if (Application.isMobilePlatform && Touchscreen.current != null){
                    endTapPos = Touchscreen.current.primaryTouch.position.ReadValue();
                }
                else if(Mouse.current != null)
                {
                    endTapPos = Mouse.current.position.ReadValue();
                }

                var distance = Vector2.Distance(startTapPos, endTapPos);

                if(distance < 10){
                    WebGLSetViewportPos(
                        (endTapPos.x / Screen.width).ToStringInvariantCulture() + "," + 
                        (endTapPos.y / Screen.height).ToStringInvariantCulture()
                    );
                }
                // else{
                //     Debug.Log("distance is too big = " + distance);
                // }

                clicking = false;
            }
        }
#else
        void Update()
        {
            if( EventSystem.current.IsPointerOverGameObject() ||
                !wt.mainObject.activeInHierarchy
            )
                return;

            if (Input.GetMouseButtonDown(0))
            {
                startTapPos = Input.mousePosition;
            }

            else if (Input.GetMouseButtonUp(0))
            {
                if(Vector2.Distance(startTapPos, Input.mousePosition) < 3){
                    WebGLSetViewportPos(
                        (Input.mousePosition.x / Screen.width).ToStringInvariantCulture() + "," + 
                        (Input.mousePosition.y / Screen.height).ToStringInvariantCulture()
                    );
                }
            }
        }
#endif
    }

}

