/*
 * 작성자: Kim, Bummoo
 * 작성일: 2024.12.03.
 */

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FUTUREVISION.WebCamera
{
    public class SpriteStickerItem : BaseStickerItem
    {
        [Header("SpriteStickerItem")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private BoxCollider spriteCollider;

        public SpriteRenderer SpriteRenderer { get => spriteRenderer; }
        public BoxCollider SpriteCollider { get => spriteCollider; }

        public override void Initialize()
        {
            base.Initialize();

            spriteRenderer.sprite = PreviewSprite;
            spriteRenderer.sortingOrder = transform.GetSiblingIndex();
        }

        protected override void OnStartInteraction()
        {
            base.OnStartInteraction();

            // TODO: 최적화 및 코드 정리 필요
            // 자식중에서 최상위로 이동
            transform.SetAsLastSibling();
        }

        protected override void SetSortOrder(int index)
        {
            base.SetSortOrder(index);

            // 스프라이트 스티커의 경우 SortingOrder를 설정
            spriteRenderer.sortingOrder = index;
        }
    }

}
