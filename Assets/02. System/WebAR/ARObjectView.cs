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
    public class ARObjectView : BaseView
    {
        [Header("Object")]
        public GameObject ParentObject;
        public List<ARObjectItem> ObjectList = new List<ARObjectItem>();
        protected int CurrentObjectIndex = 0;
        [Space]
        public UnityEvent<ARObjectItem> OnClickObjectItem;

        public override void Initialize()
        {
            base.Initialize();

            ObjectList.ForEach(item =>
            {
                item.OnClickMouseButton.AddListener((item) =>
                {
                    OnClickObjectItem.Invoke(item);
                });
            });

            // 초기화
            //SetCurrentObject(GlobalManager.Instance.DataModel.StepIndex);
        }

        private void OnEnable()
        {
            Update();
        }

        protected virtual void Update()
        {
            var arTrackerModel = WebARManager.Instance.ARTrackerModel;
            switch (arTrackerModel.GetARTrackerState())
            {
                case ARTrackerState.ScreenState:
                    ParentObject.SetActive(true);
                    break;
                case ARTrackerState.WorldState:
                    ParentObject.SetActive(arTrackerModel.IsPlacement);
                    break;
                case ARTrackerState.None:
                    Debug.LogWarning($"ARObjectView: 정의되지 않음");
                    break;
            }

            var targetObject = arTrackerModel.GetCurruentObject();

            if (targetObject == null)
            {
                Debug.LogWarning($"ARObjectView: targetObject is null");
                return;
            }

            ParentObject.transform.position = targetObject.transform.position;
            ParentObject.transform.rotation = targetObject.transform.rotation;
            ParentObject.transform.localScale = targetObject.transform.localScale;
        }

        public void SetCurrentObject(int index, bool isLoop = true)
        {
            if (isLoop)
            {
                // 순환하도록 처리
                index = (index + ObjectList.Count) % ObjectList.Count;
            }

            // 범위를 벗어나면 경고 후 종료
            if (index < 0 || index >= ObjectList.Count)
            {
                Debug.LogWarning("Index out of range");
                return;
            }

            CurrentObjectIndex = index;

            for (int i = 0; i < ObjectList.Count; i++)
            {
                ObjectList[i].gameObject.SetActive(i == index);
            }
        }

        public int GetCurrentObjectIndex()
        {
            return CurrentObjectIndex;
        }

        public ARObjectItem GetCurrentObject()
        {
            return ObjectList[CurrentObjectIndex];
        }

        /// <summary>
        /// SpiritKey 와 일치하는 ARObjectItem 을 활성화한다.
        /// 매칭 항목이 없으면 index 0 을 fallback 으로 사용.
        /// </summary>
        public void SetCurrentObjectByKey(string spiritKey)
        {
            int index = ObjectList.FindIndex(item => item.SpiritKey == spiritKey);
            if (index < 0)
            {
                Debug.LogWarning($"[ARObjectView] SpiritKey '{spiritKey}' not found. Using index 0.", this);
                index = 0;
            }
            SetCurrentObject(index, isLoop: false);
        }
    }
}
