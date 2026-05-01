using System;
using LaboratorySitInSystem.DataAccess;
using LaboratorySitInSystem.Helpers;

namespace LaboratorySitInSystem.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly ISettingsRepository _settingsRepo;

        // Default values
        private const int DEFAULT_ALARM_THRESHOLD = 30;
        private const int DEFAULT_AUTO_LOGOUT_DURATION = 1;
        private const int DEFAULT_DASHBOARD_REFRESH_INTERVAL = 30;

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
        public RelayCommand ResetToDefaultsCommand { get; }

        public SettingsViewModel(ISettingsRepository settingsRepo)
        {
            _settingsRepo = settingsRepo ?? throw new ArgumentNullException(nameof(settingsRepo));

            SaveCommand = new RelayCommand(ExecuteSave);
            ResetToDefaultsCommand = new RelayCommand(ExecuteResetToDefaults);

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

        private void ExecuteResetToDefaults(object parameter)
        {
            AlarmThreshold = DEFAULT_ALARM_THRESHOLD;
            StatusMessage = "Settings reset to default values.";
        }
    }
}
