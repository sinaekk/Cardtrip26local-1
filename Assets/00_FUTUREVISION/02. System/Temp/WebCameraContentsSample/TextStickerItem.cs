/*
 * 작성자: Kim, Bummoo
 * 작성일: 2024.12.03.
 */

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FUTUREVISION.WebCamera
{
    public enum ETextStickerMode
    {
        DateMode,
        EditableMode,
    }

    public class TextStickerItem : BaseStickerItem
    {
        [Header("TextStickerItem")]
        [SerializeField] private RectTransform rectTransform;

        [Space(10)]
        [SerializeField] private Canvas canvas;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private RectTransform enterRectTransform;
        [SerializeField] private RectTransform textRectTransform;

        [Space(10)]
        [SerializeField] private ETextStickerMode textStickerMode = ETextStickerMode.EditableMode;

        public override void Initialize()
        {
            base.Initialize();

            var camera = WebCamera.WebCaptureManager.Instance.WebCaptureCamera;
            
            // 캔버스 초기화
            canvas.worldCamera = camera;

            // 텍스트 수정 비활성화
            inputField.enabled = false;
            switch (textStickerMode)
            {
                case ETextStickerMode.DateMode:
                    {
                        // 현재 날짜로 텍스트 설정
                        inputField.text = System.DateTime.Now.ToString("yyyy.MM.dd");
                    }
                    break;
                case ETextStickerMode.EditableMode:
                    {
                        // 텍스트 수정 완료 후 텍스트 수정 비활성화
                        inputField.onEndEdit.AddListener((text) =>
                        {
                            Debug.Log("TextStickerItem::OnEndEdit: " + text);

                            inputField.enabled = false;
                        });
                    }
                    break;
            }
        }

        protected virtual void FixedUpdate()
        {
            var size01 = enterRectTransform.sizeDelta;
            var size02 = textRectTransform.sizeDelta;

            var maxSize = new Vector2(Mathf.Max(size01.x, size02.x), Mathf.Max(size01.y, size02.y));
            rectTransform.sizeDelta = maxSize;
        }

        protected override void OnStartEditContent()
        {
            base.OnStartEditContent();

            switch (textStickerMode)
            {
                case ETextStickerMode.DateMode:
                    {

                    }
                    break;
                case ETextStickerMode.EditableMode:
                    {
                        // 텍스트 수정 활성화
                        inputField.enabled = true;
                        inputField.Select();
                    }
                    break;
            }
        }
        protected override void SetSortOrder(int index)
        {
            base.SetSortOrder(index);

            // 텍스트 스티커의 경우 캔버스의 SortingOrder를 설정
            canvas.sortingOrder = index;
        }
    }
}
