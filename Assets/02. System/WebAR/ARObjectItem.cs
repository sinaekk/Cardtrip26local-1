/*
 * 작성자: Kim Bummoo
 * 작성일: 2025.03.03
 *
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FUTUREVISION.WebAR
{
    public class ARObjectItem : MonoBehaviour
    {
        [Header("Spirit 매핑 키 (SpiritTable.Key 와 일치시킬 것)")]
        public string SpiritKey;

        [Space]
        public UnityEvent<ARObjectItem> OnClickMouseButton;
        public GameObject MissionObject;
        public GameObject ScreenShotObject;

        public virtual void Initialize()
        {

        }

        private void OnEnable()
        {
            UpdateRotation();
        }

        private void UpdateRotation()
        {
            if (Camera.main == null)
                return;

            // 항상 카메라 방향을 바라보도록 설정
            // Vector3 toCamera = Camera.main.transform.position - transform.position;
            // toCamera.y = 0; // y축 회전 방지

            // transform.rotation = Quaternion.LookRotation(toCamera);
            transform.rotation = Camera.main.transform.rotation * Quaternion.Euler(0, 180, 0);
        }

        // Touch 이벤트 처리를 위한 메소드 (2D Box Collider가 터치되었을 때 호출됨)
        private void OnMouseDown()
        {
            OnClickMouseButton.Invoke(this);
        }
    }
}
