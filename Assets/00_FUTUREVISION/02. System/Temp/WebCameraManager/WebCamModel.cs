/*
 * 
 *
 */

using Unity.Mathematics;
using UnityEngine;

namespace FUTUREVISION.WebCamera
{
    public class WebCamModel : MonoBehaviour
    {
        [Header("WebCamModel")]
        private WebCamTexture webCamTexture;

        public WebCamTexture WebCamTexture { get => webCamTexture; }

        public virtual void Initialize()
        {
            webCamTexture = new WebCamTexture();
        }

        protected virtual void OnDestroy()
        {
            webCamTexture.Stop();
        }

        public virtual void Play(string device, int2? resolution = null)
        {
            // 카메라 설정
            webCamTexture.deviceName = device;

            if (resolution != null)
            {
                webCamTexture.requestedWidth = resolution.Value.x;
                webCamTexture.requestedHeight = resolution.Value.y;
            }

            webCamTexture.Play();
        }

        public virtual void Stop()
        {
            webCamTexture.Stop();
        }

        public virtual void Pause()
        {
            webCamTexture.Pause();
        }
    }

}