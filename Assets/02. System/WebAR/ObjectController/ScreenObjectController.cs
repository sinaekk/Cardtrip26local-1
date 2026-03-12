/*
 * 작성자: Kim Bummoo
 * 작성일: 2024.12.13
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FUTUREVISION
{
    public class ScreenObjectController : MonoBehaviour
    {
        public Camera ContentsCamera; // ContentsCamera를 통해 카메라 정보를 사용
        [SerializeField] private Transform filterObject; // 필터 오브젝트

        private Vector2 initialTouchPosition1;
        private Vector2 initialTouchPosition2;
        private float initialDistance;
        private float initialScale;

        public float rotationSpeed = 0.5f;
        public float scaleSpeed = 0.1f;
        public float moveSpeed = 0.1f;

        public virtual void Initialize()
        {
            // 초기화 로직이 필요하면 여기에 추가
        }

        public void OnEnable()
        {
            // 크기와 위치를 초기화
            filterObject.localPosition = Vector3.zero;
            filterObject.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));
            filterObject.localScale = Vector3.one;
        }

        void Update()
        {
            // 터치가 1개인 경우 (회전)
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

                // 첫 번째 터치가 시작된 지점으로 회전
                if (touch.phase == TouchPhase.Moved)
                {
                    float rotation = touch.deltaPosition.x * rotationSpeed;
                    filterObject.transform.Rotate(Vector3.up, rotation);
                }
            }
            // 터치가 2개인 경우 (위치 및 크기 조정)
            else if (Input.touchCount == 2)
            {
                Touch touch1 = Input.GetTouch(0);
                Touch touch2 = Input.GetTouch(1);

                // 첫 번째 터치와 두 번째 터치의 초기 위치를 기록
                if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
                {
                    initialTouchPosition1 = touch1.position;
                    initialTouchPosition2 = touch2.position;
                    initialDistance = (touch1.position - touch2.position).magnitude; // 초기 거리 저장
                    initialScale = filterObject.transform.localScale.x; // 객체의 초기 크기 저장
                }

                // 두 번째 터치가 움직이는 동안 위치와 크기 조정
                if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
                {
                    // 두 손가락 간 거리로 크기 조정
                    float currentDistance = (touch1.position - touch2.position).magnitude;
                    float scaleFactor = currentDistance / initialDistance;
                    float newScale = initialScale * scaleFactor;

                    // 스케일을 0.2 ~ 5 범위로 제한
                    newScale = Mathf.Clamp(newScale, 0.2f, 5f);

                    filterObject.transform.localScale = new Vector3(newScale, newScale, newScale);

                    // 두 손가락의 이동량을 기준으로 객체 이동
                    Vector2 currentTouchPosition1 = touch1.position;
                    Vector2 currentTouchPosition2 = touch2.position;

                    // 초기 위치와 현재 위치의 차이 계산
                    Vector2 deltaTouch1 = currentTouchPosition1 - initialTouchPosition1;
                    Vector2 deltaTouch2 = currentTouchPosition2 - initialTouchPosition2;

                    // 두 손가락의 차이만큼 이동
                    Vector2 deltaPosition = (deltaTouch1 + deltaTouch2) / 2;

                    Vector3 move = new Vector3(deltaPosition.x * moveSpeed, deltaPosition.y * moveSpeed, 0);
                    filterObject.transform.localPosition += move;

                    // 객체가 카메라 뷰포트 범위를 벗어나지 않도록 제한
                    RestrictToCameraBounds();

                    // 초기 위치 업데이트 (다음 프레임에 대비)
                    initialTouchPosition1 = currentTouchPosition1;
                    initialTouchPosition2 = currentTouchPosition2;
                }
            }
            else
            {
                initialDistance = 0; // 터치가 2개 이상 없으면 초기화
            }
        }

        // 카메라 뷰포트를 벗어나지 않도록 객체의 위치를 제한하는 함수
        private void RestrictToCameraBounds()
        {
            // 카메라의 월드 좌표로 변환된 화면의 경계를 구합니다.
            Vector3 screenMin = ContentsCamera.ViewportToWorldPoint(new Vector3(0, 0, ContentsCamera.nearClipPlane));
            Vector3 screenMax = ContentsCamera.ViewportToWorldPoint(new Vector3(1, 1, ContentsCamera.nearClipPlane));

            // 객체의 현재 위치를 제한된 범위 내로 클램프합니다.
            Vector3 currentPosition = filterObject.position;

            // 객체의 위치를 화면의 최소/최대 x, y 값으로 제한
            float clampedX = Mathf.Clamp(currentPosition.x, screenMin.x, screenMax.x);
            float clampedY = Mathf.Clamp(currentPosition.y, screenMin.y, screenMax.y);

            // 객체의 새로운 위치 설정
            filterObject.position = new Vector3(clampedX, clampedY, currentPosition.z);
        }
    }
}
