
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace FUTUREVISION
{
    public enum LoadTimeState
    {
        None,
        Awake,
        OnEnable,
        Start,
    }

    public enum ReleaseTimeState
    {
        None,
        OnDisable,
        Destory,
    }

    public class LoadWithReference : MonoBehaviour
    {

        [Header("LoadWithReference")]
        public LoadTimeState LoadTimeState = LoadTimeState.Awake;
        public ReleaseTimeState ReleaseTimeState = ReleaseTimeState.None;
        [Space]
        [Tooltip("여기에 StreamingAsset 래퍼런스 할당")]
        public AssetReference Reference;

        #region Unity Event
        private void Awake()
        {
            if (LoadTimeState == LoadTimeState.Awake)
            {
                StartLoad();
            }
        }

        private void OnEnable()
        {
            if (LoadTimeState == LoadTimeState.OnEnable)
            {
                StartLoad();
            }
        }

        void Start()
        {
            if (LoadTimeState == LoadTimeState.Start)
            {
                StartLoad();
            }
        }

        private void OnDisable()
        {
            if (ReleaseTimeState == ReleaseTimeState.OnDisable)
            {
                StartRelease();
            }
        }

        private void OnDestroy()
        {
            if (ReleaseTimeState == ReleaseTimeState.Destory)
            {
                StartRelease();
            }
        }
        #endregion

        public void StartLoad(Action callback = null)
        {
            Debug.Log("Reference 로드 시작", this);
            AsyncOperationHandle handle = Reference.LoadAssetAsync<GameObject>();
            handle.Completed += Handle_Completed;
            handle.Completed += (_) => { callback?.Invoke(); };
        }

        // Instantiate the loaded prefab on complete
        private void Handle_Completed(AsyncOperationHandle obj)
        {
            if (obj.Status == AsyncOperationStatus.Succeeded)
            {
                Instantiate(Reference.Asset, transform);
            }
            else
            {
                Debug.LogError($"AssetReference {Reference.RuntimeKey} failed to load.");
            }
        }

        public void StartRelease()
        {
            Debug.Log("Reference 해제 시작", this);
            Reference.ReleaseAsset();
        }
    }
}