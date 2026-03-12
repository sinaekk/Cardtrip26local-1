using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Imagine.WebAR.Demo{
    public class ConfidenceDemo : MonoBehaviour
    {
        public Image confidenceIcon;
        private RectTransform container;

        public Camera cam;
        public Transform trackObject;
        
        // Start is called before the first frame update
        void Start()
        {
            container = confidenceIcon.transform.parent.GetComponent<RectTransform>();
        }

        public void OnConfidence(float confidence){
            var col = new Color((1 - confidence)*2, confidence*2, 0);
            confidenceIcon.color = col;
        }

        void Update(){
            if(!trackObject.gameObject.activeInHierarchy)
                return;

            var canvasW = container.rect.width;
            var canvasH = container.rect.height;

            var viewPos = cam.WorldToViewportPoint(trackObject.position);
            // Debug.Log(viewPos + ", " + canvasW + ", " + canvasH);
            confidenceIcon.rectTransform.anchoredPosition = new Vector2(
                (viewPos.x - 0.5f) * canvasW, 
                (viewPos.y - 0.5f) * canvasH
            );
        }
    }
}

