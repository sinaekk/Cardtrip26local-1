/*
 * 작성자: 김범무
 * 작성일: 2025.01.06
 */

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FUTUREVISION
{
    public class TextUIItem : MonoBehaviour
    {
        [Header("TextUIItem")]
        public TextMeshProUGUI Text;

        private string textData;
        private int currentLength;
        private float updateDelay = 0.1f;

        public void OnEnable()
        {
            textData = Text.text;
            Text.text = string.Empty;
            currentLength = 0;
            StartCoroutine(UpdateText());
        }

        public IEnumerator UpdateText()
        {
            while (currentLength < textData.Length)
            {
                currentLength++;
                Text.text = textData.Substring(0, currentLength);

                yield return new WaitForSeconds(updateDelay);
            }
        }
    }
}
