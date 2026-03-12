/*
 * 작성자: #AUTHOR#
 * 작성일: #DATE#
 *
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FUTUREVISION
{
    public class Base : MonoBehaviour
    {
        [Header("Base")]
        // 초기화 여부로, 별도로 초기화 하지 않을 경우 Start()에서 초기화됨
        protected bool isInitialize = false;
        public bool IsInitialize => isInitialize;

        public virtual void Initialize()
        {
            Debug.Log($"[{this.GetType().Name}] Initialize", this);
            this.isInitialize = true;
        }

        #region Unity
        protected virtual void Awake()
        {
        }

        protected virtual void Start()
        {
        }

        protected virtual void OnEnable()
        {
            // 2025.09.05 김범무
            // 실행되었을 때 초기화 되어 있지 않으면 자동으로 초기화
            // 오브젝트 생성시 초기화를 할 수도 있고 아닐 수도 있는데 매번 호출하기 번거로움
            // Start에 있을 경우 Manager가 먼저 초기화 되어야 하는데 순서 보장이 안됨
            if (this.isInitialize)
            {
                Initialize();
            }
        }

        protected virtual void OnDisable()
        {
        }
        #endregion
    }
}
