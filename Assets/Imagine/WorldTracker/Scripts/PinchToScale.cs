using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Imagine.WebAR
{
    public class PinchToScale : MonoBehaviour
    {
        [SerializeField] Transform scaleTransform;
        Vector3 origScale, startScale;
        Vector2 touch0StartPos, touch1StartPos;

        [SerializeField] float minScale = 0.1f;
        [SerializeField] float maxScale = 10f;

        bool isPinching = false;


        void Awake(){
            origScale = scaleTransform.localScale;
        }
        
        void Start()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            Input.multiTouchEnabled = true;
#endif
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
                    isPinching = true;
                    startScale = scaleTransform.localScale;

                    Debug.Log("Start Pinch");
                }
                else if (touch0.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended ||
                    touch1.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended ||
                    touch0.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled ||
                    touch1.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled)
                {
                    isPinching = false;
                    Debug.Log("End Pinch");
                }

                if (isPinching)
                {
                    var dStart = (touch1StartPos - touch0StartPos).magnitude;

                    var pos0 = touch0.position.ReadValue();
                    var pos1 = touch1.position.ReadValue();
                    var d = (pos1 - pos0).magnitude;

                    // var scale = Mathf.Clamp(d / dStart, minScale, maxScale);
                    // scaleTransform.localScale = startScale * scale;
                    scaleTransform.localScale = startScale * d / dStart;
                    scaleTransform.localScale = new Vector3(
                        Mathf.Clamp(scaleTransform.localScale.x, origScale.x * minScale, origScale.x * maxScale),
                        Mathf.Clamp(scaleTransform.localScale.y, origScale.y * minScale, origScale.y * maxScale),
                        Mathf.Clamp(scaleTransform.localScale.z, origScale.z * minScale, origScale.z * maxScale)
                        );

                    //Debug.Log("Pinch " + scale.ToString("0.00") + "x");
                }


            }
            else
            {
                isPinching = false;
            }

        }
#else
        void Update()
        {
            if (Input.touchCount == 2)
            {
                var touch0 = Input.GetTouch(0);
                var touch1 = Input.GetTouch(1);

                if (touch0.phase == TouchPhase.Began ||
                    touch1.phase == TouchPhase.Began)
                {
                    touch0StartPos = touch0.position;
                    touch1StartPos = touch1.position;
                    isPinching = true;
                    startScale = scaleTransform.localScale;

                    Debug.Log("Start Pinch");
                }
                else if (touch0.phase == TouchPhase.Ended ||
                    touch1.phase == TouchPhase.Ended ||
                    touch0.phase == TouchPhase.Canceled ||
                    touch1.phase == TouchPhase.Canceled)
                {
                    isPinching = false;
                    Debug.Log("End Pinch");
                }

                if (isPinching)
                {
                    var dStart = (touch1StartPos - touch0StartPos).magnitude;

                    var pos0 = touch0.position;
                    var pos1 = touch1.position;
                    var d = (pos1 - pos0).magnitude;

                    // var scale = Mathf.Clamp(d / dStart, minScale, maxScale);
                    // scaleTransform.localScale = startScale * scale;
                    scaleTransform.localScale = startScale * d / dStart;
                    scaleTransform.localScale = new Vector3(
                        Mathf.Clamp(scaleTransform.localScale.x, origScale.x * minScale, origScale.x * maxScale),
                        Mathf.Clamp(scaleTransform.localScale.y, origScale.y * minScale, origScale.y * maxScale),
                        Mathf.Clamp(scaleTransform.localScale.z, origScale.z * minScale, origScale.z * maxScale)
                        );

                    //Debug.Log("Pinch " + scale.ToString("0.00") + "x");
                }


            }
            else
            {
                isPinching = false;
            }

        }
#endif

        public void Reset()
        {
            scaleTransform.localScale = origScale;
        }
    }
}
