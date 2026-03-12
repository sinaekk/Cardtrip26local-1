/*
 * 작성자: Kim Bummoo
 * 작성일: 2025.05.13
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FUTUREVISION.Content
{
    public class RewardView : BaseView
    {
        [Header("Reward View")]
        public List<GameObject> RewardItems;
        public Button ToNextButton;
        public Button ToBeforeButton;
        [Space]
        public GameObject LockImage;

        private int currentIndex = 0;

        public override void Initialize()
        {
            base.Initialize();
            ToNextButton.onClick.RemoveAllListeners();
            ToNextButton.onClick.AddListener(() =>
            {
                IncreaseIndex();
                GlobalManager.Instance.SoundModel.PlayButtonClickSound();
            });
            ToBeforeButton.onClick.RemoveAllListeners();
            ToBeforeButton.onClick.AddListener(() =>
            {
                DecreaseIndex();
                GlobalManager.Instance.SoundModel.PlayButtonClickSound();
            });
            SetRewardItem(0);
        }

        public void SetRewardItem(int index)
        {
            currentIndex = index;
            for (int i = 0; i < RewardItems.Count; i++)
            {
                RewardItems[i].SetActive(i == currentIndex);
            }

            LockImage.SetActive(!(currentIndex == 0));
            UpdateUI();
        }

        public void IncreaseIndex()
        {
            if (currentIndex < RewardItems.Count - 1)
            {
                currentIndex++;
                SetRewardItem(currentIndex);
            }
        }

        public void DecreaseIndex()
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                SetRewardItem(currentIndex);
            }
        }

        private void UpdateUI()
        {
            ToBeforeButton.interactable = currentIndex > 0;
            ToNextButton.interactable = currentIndex < RewardItems.Count - 1;
        }
    }
}
