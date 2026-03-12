/*
 * 작성자: Kim, Bummoo
 * 작성일: 2024.12.03.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FUTUREVISION.WebCamera
{
    public enum EStickerEventState
    {
        None,
        Idle,            // 아무 동작도 없는 기본 상태
        WaitForInput,    // 입력을 대기하는 상태
        Translate,       // 위치 이동 상태
        Rotation,        // 회전 상태
        Scale            // 크기 조절 상태
    }


    public class BaseStickerItem : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IScrollHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("BaseStickerItem")]
        public Sprite PreviewSprite;

        [Space(10)]
        public Vector2 sizeRange = new Vector2(0.3f, 5.0f);
        public float ScaleSpeed = 0.1f; // 스크롤에 따른 크기 조절 속도
        public float RotateSpeed = 0.1f; // 회전 속도

        private EStickerEventState eventState;
        private PointerEventData firstHand;
        private PointerEventData secondHand;

        private DateTime startTime;
        private float startDistance;
        private float startScale;

        private Vector2 startDragPosition;
        private float startRotation;

        public virtual void Initialize()
        {
            eventState = EStickerEventState.Idle;
        }

        /// <summary>
        /// <para>상호작용 시작 시 호출되는 이벤트</para>
        /// </summary>
        protected virtual void OnStartInteraction()
        {
            // 모든 자식을 순회하며 SortOrder를 자식 인덱스로 설정
            for (int i = 0; i < transform.parent.childCount; i++)
            {
                transform.parent.GetChild(i).GetComponent<BaseStickerItem>().SetSortOrder(i);
            }
        }

        /// <summary>
        /// <para>컨텐츠 편집 시작 시 호출되는 이벤트</para>
        /// <para>짧게 눌렀을 때 편집 활성화</para>
        /// </summary>
        protected virtual void OnStartEditContent()
        {

        }

        protected virtual void SetSortOrder(int index)
        {

        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            switch (eventState)
            {
                case EStickerEventState.Idle:
                    startTime = DateTime.Now;
                    eventState = EStickerEventState.WaitForInput;
                    firstHand = eventData;
                    StartCoroutine(CheckLongClickCoroutine());

                    // 상호작용 시작 이벤트 호출
                    OnStartInteraction();
                    break;
                case EStickerEventState.WaitForInput:
                    if (eventData.pointerId == firstHand.pointerId) return;

                    eventState = EStickerEventState.Scale;
                    secondHand = eventData;

                    startDistance = Vector2.Distance(firstHand.position, secondHand.position);
                    startScale = transform.localScale.x;
                    break;
                default:
                    Debug.LogWarning("StickerItem: Invalid Event State: " + eventState);
                    break;
            }
        }

        private IEnumerator CheckLongClickCoroutine()
        {
            var camera = WebCaptureManager.Instance.WebCaptureCamera;

            while (eventState == EStickerEventState.WaitForInput)
            {
                float time = (float)(DateTime.Now - startTime).TotalSeconds;

                if (time > 1.0f)
                {
                    Debug.Log("StickerItem: Long Click");
                    eventState = EStickerEventState.Rotation;

                    //startDragPosition = firstHand.position;
                    startDragPosition = camera.ScreenToWorldPoint(firstHand.position);
                    startRotation = transform.eulerAngles.z;
                }

                yield return null;
            }
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != firstHand.pointerId) return;

            if (eventState == EStickerEventState.WaitForInput)
            {
                float time = (float)(DateTime.Now - startTime).TotalSeconds;

                if (time < 0.5f)
                {
                    Debug.Log("StickerItem: Click");

                    OnStartEditContent();
                }
                else
                    Debug.Log("StickerItem: Long Click");

                eventState = EStickerEventState.Idle;
            }
        }

        public void OnScroll(PointerEventData eventData)
        {
#if UNITY_EDITOR
            float newScale = transform.localScale.x + eventData.scrollDelta.y * ScaleSpeed;
            newScale = Mathf.Clamp(newScale, sizeRange.x, sizeRange.y);
            transform.localScale = new Vector3(newScale, newScale, newScale);
#endif
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventState == EStickerEventState.WaitForInput && eventData.pointerId == firstHand.pointerId)
            {
                startTime = DateTime.Now;
                eventState = EStickerEventState.Translate;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            var camera = WebCaptureManager.Instance.WebCaptureCamera;

            switch (eventState)
            {
                case EStickerEventState.Translate:
                    if (eventData.pointerId != firstHand.pointerId) return;

                    Vector3 delta = camera.ScreenToWorldPoint(eventData.delta) - camera.ScreenToWorldPoint(Vector3.zero);
                    Vector3 newPosition = transform.position + delta;

                    // 0,0과 스크린 크기를 기준으로 이동 제한 적용
                    Vector2 screenMin = camera.ScreenToWorldPoint(Vector3.zero);
                    Vector2 screenMax = camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height));

                    // 이동 제한 영역을 적용
                    newPosition.x = Mathf.Clamp(newPosition.x, screenMin.x, screenMax.x);
                    newPosition.y = Mathf.Clamp(newPosition.y, screenMin.y, screenMax.y);

                    transform.position = newPosition;
                    break;

                case EStickerEventState.Rotation:
                    if (eventData.pointerId != firstHand.pointerId) return;

                    Vector2 origin = (Vector2)transform.position;
                    Vector2 currentDragPosition = camera.ScreenToWorldPoint(eventData.position);

                    Vector2 from = (startDragPosition - origin).normalized;
                    Vector2 to = (currentDragPosition - origin).normalized;

                    float angle = Vector2.SignedAngle(from, to);
                    transform.eulerAngles = new Vector3(0, 0, startRotation + angle);
                    break;

                case EStickerEventState.Scale:
                    if (eventData.pointerId != firstHand.pointerId && eventData.pointerId != secondHand.pointerId) return;

                    float distance = Vector2.Distance(firstHand.position, secondHand.position);
                    float scale = distance / startDistance * startScale;

                    scale = Mathf.Clamp(scale, sizeRange.x, sizeRange.y);
                    transform.localScale = new Vector3(scale, scale, scale);
                    break;
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (eventData.pointerId == firstHand.pointerId || eventData.pointerId == secondHand.pointerId)
            {
                eventState = EStickerEventState.Idle;
            }
        }
    }
}
