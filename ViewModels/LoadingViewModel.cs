using System;
using LaboratorySitInSystem.DataAccess;

namespace LaboratorySitInSystem.ViewModels
{
    public class LoadingViewModel : ViewModelBase
    {
        public Action OnLoadingComplete { get; set; }

        public LoadingViewModel(Action onComplete)
        {
            OnLoadingComplete = onComplete;
        }
    }
}
