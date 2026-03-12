using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FUTUREVISION.Content
{
    public class CardTripItem : BaseItem
    {
        [Header("Card Trip Item")]
        public UnityEvent OnClicked;
        public SpriteRenderer SpriteRenderer;

        // 터치 및 클릭 이벤트 처리
        public void OnItemClicked()
        {
            Debug.Log("CardTripItem Clicked: " + gameObject.name, this);
            // 아이템 클릭 시 수행할 동작 구현
        }

        private void OnMouseDown()
        {
            OnItemClicked();
            OnClicked?.Invoke();
        }
    }
}
