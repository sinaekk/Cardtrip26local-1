/*
 * DATE     : 2024.11.27
 * AUTHOR   : Kim Bum Moo
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FUTUREVISION
{
    public class BaseViewModel : Base
    {
        [Header("Base View Model")]
        public List<BaseView> SubViewList;
        public override void Initialize()
        {
            base.Initialize();

            SubViewList.ForEach(view => view.Initialize());
        }
    }
}