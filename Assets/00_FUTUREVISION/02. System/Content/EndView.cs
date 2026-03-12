/*
 * 작성자: 김범무
 * 작성일: 2025.05.11
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FUTUREVISION.Content
{
    public class EndView : MonoBehaviour
    {
        [Header("End View")]
        [SerializeField] protected GameObject End1;
        [SerializeField] protected GameObject End2;
        [SerializeField] protected GameObject End3;
        [SerializeField] protected GameObject End4;
        [SerializeField] protected GameObject End5;
        [SerializeField] protected GameObject End6;
        [Space(10)]
        public Button ToBingoButton;
        protected List<GameObject> EndObjects => new List<GameObject>
        {
            End1, End2, End3, End4, End5, End6
        };

        public virtual void Initialize()
        {

        }

        public void OnEnable()
        {
            var dataModel = GlobalManager.Instance.DataModel;
            var endObjects = this.EndObjects;
            foreach (var endObject in endObjects)
            {
                endObject.SetActive(false);
            }
            //endObjects[dataModel.StepIndex].SetActive(true);

            //ToBingoButton.gameObject.SetActive(dataModel.StepIndex == 5);
        }
    }
}
