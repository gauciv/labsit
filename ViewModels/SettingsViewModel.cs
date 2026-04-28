using System;
using LaboratorySitInSystem.DataAccess;
using LaboratorySitInSystem.Helpers;

namespace LaboratorySitInSystem.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly ISettingsRepository _settingsRepo;

        private int _alarmThreshold;
        private string _statusMessage;

        public int AlarmThreshold
        {
            get => _alarmThreshold;
            set => SetProperty(ref _alarmThreshold, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public RelayCommand SaveCommand { get; }

        public SettingsViewModel(ISettingsRepository settingsRepo)
        {
            _settingsRepo = settingsRepo ?? throw new ArgumentNullException(nameof(settingsRepo));

            SaveCommand = new RelayCommand(ExecuteSave);

            LoadSettings();
        }

        private void LoadSettings()
        {
            var settings = _settingsRepo.GetSettings();
            AlarmThreshold = settings.AlarmThreshold;
        }

        private void ExecuteSave(object parameter)
        {
            _settingsRepo.UpdateAlarmThreshold(AlarmThreshold);
            StatusMessage = "Settings saved successfully.";
        }
    }
}
