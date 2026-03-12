/*
 * DATE     : 2024.11.27
 * AUTHOR   : Kim Bum Moo
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FUTUREVISION
{
    public class BaseManager<FinalManagerClass> : BaseSingleton<FinalManagerClass>
        where FinalManagerClass : BaseManager<FinalManagerClass>
    {
        [Header("Base Manager")]
        public List<BaseModel> ModelList;
        public List<BaseViewModel> ViewModelList;

        public override void Initialize()
        {
            base.Initialize();

            ModelList.ForEach(model => model.Initialize());
            ViewModelList.ForEach(viewModel => viewModel.Initialize());
        }
    }
}