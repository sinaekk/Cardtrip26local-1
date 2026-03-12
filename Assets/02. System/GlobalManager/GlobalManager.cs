
/*
 * 작성자: Kim, Bummoo
 * 작성일: 2024.12.04
 */
using FUTUREVISION.WebCamera;
using FUTUREVISION.WebAR;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FUTUREVISION
{
    public class GlobalManager : BaseManager<GlobalManager>
    {
        [Header("GlobalManager")]
        public DataModel DataModel;
        public SoundModel SoundModel;
        [Space(10)]
        public Gemini_Chatbot Gemini_Chatbot;

        protected override void Start()
        {
            base.Start();

            Initialize();

            WebARManager.Instance.Initialize();
        }
    }
}