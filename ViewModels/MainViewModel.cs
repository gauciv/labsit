using System;

namespace LaboratorySitInSystem.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private static MainViewModel _instance;

        private object _currentView;

        public static MainViewModel Instance => _instance;

        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public MainViewModel()
        {
            _instance = this;
        }

        public void NavigateTo(ViewModelBase viewModel)
        {
            CurrentView = viewModel;
        }
    }
}
