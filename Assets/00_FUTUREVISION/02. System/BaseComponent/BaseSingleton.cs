/*
 * DATE     : 2024.11.27
 * AUTHOR   : Kim Bum Moo
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FUTUREVISION
{
    public class BaseSingleton<FinalClass> : Base
        where FinalClass : BaseSingleton<FinalClass>
    {
        [Header("BaseSingleton")]
        public bool UseDontDestroyOnLoad = true;

        private static FinalClass instance;
        public static FinalClass Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<FinalClass>();

                    if (instance == null)
                    {
                        Debug.LogWarning("There is no instance of " + typeof(FinalClass).Name);
                    }
                }

                return instance;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            if (instance == null)
            {
                instance = this as FinalClass;

                if (UseDontDestroyOnLoad)
                {
                    DontDestroyOnLoad(this.gameObject);
                }
            }
            else if (instance != this)
            {
                Debug.LogWarning("There are multiple instances of " + typeof(FinalClass).Name, this);
            }
        }
    }

}