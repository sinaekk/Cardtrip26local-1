using System.Collections;
using System.Collections.Generic;
using Imagine.WebAR;
using UnityEngine;

namespace Imagine.WebAR.Demo{
    public class PortalDemo : MonoBehaviour
    {
        [SerializeField] GameObject portalMaskRealSide, portalMaskVirtualSide;
        [SerializeField] Transform doorTransform, camTransform;

        public float lastZ;

        void Start(){
            var camTransform = FindObjectOfType<ARCamera>().transform;
            lastZ = doorTransform.InverseTransformPoint(camTransform.position).z;
        }

        void Update()
        {
            var z = doorTransform.InverseTransformPoint(camTransform.position).z;
            var thresh = 0.02f;

            if(lastZ > thresh && z < thresh){
                portalMaskRealSide.SetActive(false);
                portalMaskVirtualSide.SetActive(true);
            }
            else if(lastZ < -thresh && z > -thresh){
                portalMaskRealSide.SetActive(true);
                portalMaskVirtualSide.SetActive(false);
            }

            lastZ = z;
            
        }
    }
}
