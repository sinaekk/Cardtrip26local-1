using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Imagine.WebAR
{
    public class SwipeToRotateY : MonoBehaviour
    {

        [SerializeField] private Transform rotTransform;
        //TODO: implement using Screen.DPI / Canvas.innerWidth.Height API grabber

        private Vector2 startDragPos;
        private bool isDragging = false;



        private Quaternion origRot, startRot;

        [SerializeField] private float sensitivity = 20;

        void Awake()
        {
            origRot = rotTransform.rotation;
        }

#if ENABLE_INPUT_SYSTEM
        void Update()
        {
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

                if(activeTouchCount > 1){
                    isDragging = false;
                    return;
                }

                // var touchPhase = Touchscreen.current.primaryTouch.phase.ReadValue();
                // Debug.Log($"primaryTouch {Touchscreen.current.primaryTouch.phase.ReadValue()} Began={touchPhase == UnityEngine.InputSystem.TouchPhase.Began}");
            }


            if ((Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
                (Touchscreen.current != null && Touchscreen.current.primaryTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began))
            {
                // Debug.Log("clicked!");

                if (Application.isMobilePlatform && Touchscreen.current != null){
                    // Debug.Log($"primaryTouch Began at {Touchscreen.current.primaryTouch.position.ReadValue()}");
                    startDragPos = Touchscreen.current.primaryTouch.position.ReadValue();
                    // Debug.Log("touch here!" + startDragPos);
                }
                
                else if(Mouse.current != null)
                {
                    startDragPos = Mouse.current.position.ReadValue();
                    // Debug.Log("mouse here!" + startDragPos);

                }
                

                startRot = rotTransform.rotation;
                isDragging = true;
            }

            else if ((Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame) ||
                (Touchscreen.current != null && Touchscreen.current.primaryTouch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended))
            {
                isDragging = false;
            }

            if (isDragging)
            {
                Vector2 curDragPos = new Vector2();

                if (Application.isMobilePlatform && Touchscreen.current != null){
                    curDragPos = Touchscreen.current.primaryTouch.position.ReadValue();
                    // Debug.Log($"touch curDragPos = {curDragPos.x}");
                }
                
                else if(Mouse.current != null)
                {
                    curDragPos = Mouse.current.position.ReadValue();
                    // Debug.Log($"mouse curDragPos = {curDragPos.x}");
                }

                

                var x = curDragPos.x - startDragPos.x;
                var a = x * sensitivity * -1;

                rotTransform.rotation = startRot * Quaternion.AngleAxis(a, Vector3.up);
            }

        }
#else
        void Update()
        {

            if (Input.touchCount > 1)
            {
                isDragging = false;
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                startDragPos = Input.mousePosition;
                startRot = rotTransform.rotation;
                isDragging = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }

            if (isDragging)
            {
                var curDragPos = Input.mousePosition;

                var x = curDragPos.x - startDragPos.x;
                var a = x * sensitivity * -1;

                rotTransform.rotation = startRot * Quaternion.AngleAxis(a, Vector3.up);

            }

        }
    #endif

        public void Reset()
        {
            rotTransform.rotation = origRot;
        }
    }
}
