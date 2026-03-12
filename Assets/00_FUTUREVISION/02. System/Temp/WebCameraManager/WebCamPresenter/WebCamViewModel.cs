/*
 * 
 *
 */

using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace FUTUREVISION
{
    public class WebCamViewModel : BaseViewModel
    {
        [Header("이유는 모르겠으나, Canvas의 sorting order를 0 보다 작게 해야 camera에 있는 physics raycaster 이벤트가 blocking되지 않음.")]

        [Header("WebCamPresenter")]
        [SerializeField] protected WebCamView view;
        public WebCamView View { get => view; }

        public override void Initialize()
        {
            base.Initialize();

            view.Initialize();
        }

        public virtual void SetCamTexture(WebCamTexture webCamTexture)
        {
            view.SetCamTexture(webCamTexture);
        }

        public virtual void UpdateImageSize(int2 imageSize)
        {
            view.UpdateImageSize(imageSize);
        }
    }
}
