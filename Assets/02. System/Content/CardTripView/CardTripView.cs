/*
 * 작성자: Kim Bummoo
 * 작성일: 2025.05.13
 */

using FUTUREVISION.WebAR;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FUTUREVISION.Content
{
    public enum ECardTripStage
    {
        None,
        Stage1Guide,
        Stage1Play,
        Stage2Guide,
        Stage2Play,
        Stage3Play,
        Stage3Guide,
        StageClear,
    }

    public class CardTripView : BaseView
    {
        [Header("Card Trip View")]
        public GameObject ARObject;

        [Header("Card Trip View/Stage1")]
        public GameObject Stage1Object;
        public GameObject Stage1GuideObject;
        public Button Stage1GuideCloseButton;

        [Space]
        public CardTripItem TreasureItem;
        [Header("Card Trip View/Stage2")]
        public GameObject Stage2Object;
        public GameObject Stage2GuideObject;
        public Button Stage2GuideCloseButton;

        [Space]
        public CardTripItem GuardianSpiritItem;
        public List<CardTripItem> SpritOrbItemList = new List<CardTripItem>();
        [Header("Card Trip View/Stage3")]
        public GameObject Stage3Object;
        public GameObject Stage3GuideObject;
        public Button Stage3GuideCloseButton;

        [Space]
        public CardTripItem CaptureTargetItem;
        public RectTransform Card;
        public EventTrigger CardEventTrigger;

        [Header("Card Trip View/Stage Clear")]
        public GameObject StageClearObject;
        public Animation CardClearAnimation;

        [Header("Card Trip View/UI")]
        public Button ReplaceButton;

        [Header("Card Trip View/Guide Arrow")]
        public RectTransform GuideArrowObject;

        [Header("Update Guide Arrow")]
        private GameObject guideTarget = null;


        private void Update()
        {
            // 배치되지 않았으면 ARObject 비활성화
            ARObject.SetActive(WebARManager.Instance.ARTrackerModel.IsPlacement);

            // ARObject 로테이션 포지션 업데이트
            var arTrackerModel = WebARManager.Instance.ARTrackerModel;
            var targetObject = arTrackerModel.GetCurruentObject();
            ARObject.transform.position = targetObject.transform.position;
            ARObject.transform.rotation = targetObject.transform.rotation;
            ARObject.transform.localScale = targetObject.transform.localScale;

            // 가이드 화살표 업데이트
            UpdateGuideArrow();
        }

        private void UpdateGuideArrow()
        {
            if (guideTarget == null || guideTarget.activeSelf == false)
            {
                GuideArrowObject.gameObject.SetActive(false);
                return;
            }

            Vector3 screenPos = WebARManager.Instance.ARTrackerModel.ARCamera.cam.WorldToScreenPoint(guideTarget.transform.position);
            if (screenPos.z < 0)
            {
                // 타겟이 카메라 뒤에 있을 때
                GuideArrowObject.gameObject.SetActive(false);
                return;
            }

            // 화면 내에 타겟이 보일 때
            if (screenPos.x >= 0 && screenPos.x <= Screen.width &&
                screenPos.y >= 0 && screenPos.y <= Screen.height)
            {
                GuideArrowObject.gameObject.SetActive(false);
                return;
            }

            GuideArrowObject.gameObject.SetActive(true);

            Vector3 dir = (screenPos - new Vector3(Screen.width / 2f, Screen.height / 2f, 0)).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            GuideArrowObject.rotation = Quaternion.Euler(0, 0, angle);

            float edgeBuffer = 30f; // 화면 가장자리에서 약간 안쪽으로 위치시키기 위한 버퍼
            float x = Mathf.Clamp(screenPos.x, edgeBuffer, Screen.width - edgeBuffer);
            float y = Mathf.Clamp(screenPos.y, edgeBuffer, Screen.height - edgeBuffer);
            GuideArrowObject.anchoredPosition = new Vector2(x - Screen.width / 2f, y - Screen.height / 2f);
        }

        public void SetStage(ECardTripStage stage, Action callback = null)
        {
            Stage1Object.SetActive(stage == ECardTripStage.Stage1Play);
            Stage1GuideObject.SetActive(stage == ECardTripStage.Stage1Guide);

            Stage2Object.SetActive(stage == ECardTripStage.Stage2Play);
            Stage2GuideObject.SetActive(stage == ECardTripStage.Stage2Guide);

            Stage3Object.SetActive(stage == ECardTripStage.Stage3Play);
            Stage3GuideObject.SetActive(stage == ECardTripStage.Stage3Guide);

            StageClearObject.SetActive(stage == ECardTripStage.StageClear);
            if (stage == ECardTripStage.StageClear)
            {
                CardClearAnimation.Play();
                StartCoroutine(WaitFinishAnimation(callback));
            }

            // stage에 따른 가이드 타겟 설정
            switch (stage)
            {
                case ECardTripStage.Stage1Guide:
                case ECardTripStage.Stage1Play:
                    guideTarget = TreasureItem.gameObject;
                    break;
                case ECardTripStage.Stage2Guide:
                case ECardTripStage.Stage2Play:
                    guideTarget = GuardianSpiritItem.gameObject;
                    break;
                case ECardTripStage.Stage3Guide:
                case ECardTripStage.Stage3Play:
                    guideTarget = CaptureTargetItem.gameObject;
                    break;
                default:
                    guideTarget = null;
                    break;
            }
            UpdateGuideArrow();
        }

        private IEnumerator WaitFinishAnimation(Action callback)
        {
            yield return new WaitForSeconds(CardClearAnimation.clip.length);
            //yield return new WaitForSeconds(3.0f);
            callback?.Invoke();
        }
    }
}
