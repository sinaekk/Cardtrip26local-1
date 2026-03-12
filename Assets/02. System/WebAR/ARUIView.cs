/*
 * 작성자: Kim Bummoo
 * 작성일: 2025.03.03
 *
 */

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FUTUREVISION.WebAR
{
    public class ARUIView : BaseView
    {
        [Space(10)]
        [Header("Button")]
        public TextBoxUIItem SwitchCameraButton;
        public TextBoxUIItem PlaceButton;
        public TextBoxUIItem TakeScreenshotButton;
        public TextBoxUIItem SwitchARButton;

        [Space(10)]
        public ButtonUIItem BeforeButton;
        public ButtonUIItem NextButton;

        public override void Initialize() 
        {
            base.Initialize();

            SwitchCameraButton.Button.onClick.AddListener(() =>
            {
                GlobalManager.Instance.StartCoroutine(DelayButton(1.0f, SwitchCameraButton.Button));
                GlobalManager.Instance.SoundModel.PlayButtonClickSound();
            });
            PlaceButton.Button.onClick.AddListener(() =>
            {
                GlobalManager.Instance.StartCoroutine(DelayButton(1.0f, PlaceButton.Button));
                GlobalManager.Instance.SoundModel.PlayButtonClickSound();
            });
            TakeScreenshotButton.Button.onClick.AddListener(() => GlobalManager.Instance.StartCoroutine(DelayButton(1.0f, TakeScreenshotButton.Button)));
            SwitchARButton.Button.onClick.AddListener(() => {
                GlobalManager.Instance.StartCoroutine(DelayButton(1.0f, SwitchARButton.Button));
                GlobalManager.Instance.SoundModel.PlayButtonClickSound();
            });
        }

        public void SetActivePlacedButton(bool isPlaced)
        {
            //SwitchCameraButton.gameObject.SetActive(isPlaced);
            PlaceButton.gameObject.SetActive(isPlaced);
            TakeScreenshotButton.gameObject.SetActive(!isPlaced);
            //SwitchARButton.gameObject.SetActive(isPlaced);
        }

        public void SetPlaceButtonInteractable(bool interactable)
        {
            PlaceButton.Button.interactable = interactable;
        }

        public void SetActiveSwitchARButton(bool isActive)
        {
            SwitchARButton.gameObject.SetActive(isActive);
        }

        public void SetActiveStepButton(bool before, bool next)
        {
            BeforeButton.gameObject.SetActive(before);
            NextButton.gameObject.SetActive(next);
        }

        // 버튼 연속 클릭을 막기위한 1초 딜레이
        private IEnumerator DelayButton(float delay, Button button)
        {
            button.interactable = false;

            yield return new WaitForSeconds(delay);

            button.interactable = true;
        }
    }
}
