/*
 * 
 *
 */

using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace FUTUREVISION.WebCamera
{
    public sealed class WebCaptureManager : BaseManager<WebCaptureManager>
    {
        [Header("WebCaptureManager")]
        public Camera WebCaptureCamera;
        public RenderTexture RenderTexture;
        public WebCamModel Model;
        public WebCamViewModel ViewModel;

        [Space(10)]
        [SerializeField] private bool UseFrontCameraDefault = true;

        // 특별한 이유가 없다면 Model, ViewModel을 외부에서 접근하는 것을 허용하지 않음
        //public WebCamModel Model { get => model; }
        //public WebCamViewModel ViewModel { get => viewModel; }

        private Texture2D texture;

        public Texture2D Texture { get => texture; }


        protected override void Awake()
        {
            base.Awake();

            Initialize();
        }

        public override void Initialize()
        {
            base.Initialize();

            Model.Initialize();
            ViewModel.Initialize();

            ViewModel.SetCamTexture(Model.WebCamTexture);
            ViewModel.UpdateImageSize(new int2(Model.WebCamTexture.width, Model.WebCamTexture.height));
        }

        public void SetWebCamActive(bool newActive, string device = null)
        {
            if (newActive)
            {
                Model.Play(device);
            }
            else
            {
                Model.Stop();
            }

            this.gameObject.SetActive(newActive);
        }

        private void Update()
        {
            // Render Texture의 크기를 스크린 크기에 맞춰 업데이트
            if (RenderTexture.width != Screen.width || RenderTexture.height != Screen.height)
            {
                RenderTexture.Release();
                RenderTexture.width = Screen.width;
                RenderTexture.height = Screen.height;
                RenderTexture.Create();
            }

            // Image SIze Update
            ViewModel.UpdateImageSize(new int2(Model.WebCamTexture.width, Model.WebCamTexture.height));


            if (texture != null)
            {
                Destroy(texture);
            }
            texture = CopyDynamicTextureFromCamera();
        }

        public void SetCameraDevice(WebCamDevice cameraDevice)
        {
            Model.Stop();

            var mirrorWeight = cameraDevice.isFrontFacing ? -1f : 1f;
            ViewModel.transform.localScale = new Vector3(1, mirrorWeight, 1);
            Model.Play(cameraDevice.name);
        }

        /// <summary>
        /// <para>카메라의 타겟 텍스처를 복사하여 Texture2D로 반환합니다.</para>
        /// <para>* 사용 후 Texture2D 자원 해제 필요</para>
        /// </summary>
        /// <returns></returns>
        private Texture2D CopyDynamicTextureFromCamera()
        {
            // 카메라가 렌더 타겟을 갱신하도록 렌더 호출
            WebCaptureCamera.Render();

            // 현재 카메라의 RenderTexture를 임시적으로 활성화
            RenderTexture currentRT = RenderTexture.active;
            RenderTexture.active = WebCaptureCamera.targetTexture;

            // Texture2D 생성
            Texture2D image = new Texture2D(WebCaptureCamera.targetTexture.width, WebCaptureCamera.targetTexture.height, TextureFormat.RGB24, false);

            // RenderTexture 데이터를 Texture2D에 복사
            image.ReadPixels(new Rect(0, 0, WebCaptureCamera.targetTexture.width, WebCaptureCamera.targetTexture.height), 0, 0);
            image.Apply();

            // RenderTexture 활성화 상태 복원
            RenderTexture.active = currentRT;

            return image;
        }

#if UNITY_WEBGL

        /// <summary>
        /// <para>웹에서 텍스처를 다운로드합니다.</para>
        /// </summary>
        public void DownloadTexture()
        {
            // 캡쳐를 함
            var date = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            var fileName = $"GodLiving-{date}.png";
            var bytes = WebCaptureManager.Instance.Texture.EncodeToPNG();
            WebGLDownloader.DownloadFile(bytes, bytes.Length, fileName);
        }
#endif
    }
}