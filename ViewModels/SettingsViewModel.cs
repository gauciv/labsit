using System;
using LaboratorySitInSystem.DataAccess;
using LaboratorySitInSystem.Helpers;

namespace LaboratorySitInSystem.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly ISettingsRepository _settingsRepo;

        private int _alarmThreshold;
        private int _autoLogoutDuration;
        private int _dashboardRefreshInterval;
        private bool _enableSoundNotifications;
        private bool _requireStudentId;
        private bool _showSessionHistory;
        private string _statusMessage;
        private bool _isSaveSuccess;

        public int AlarmThreshold
        {
            get => _alarmThreshold;
            set => SetProperty(ref _alarmThreshold, value);
        }

        public int AutoLogoutDuration
        {
            get => _autoLogoutDuration;
            set => SetProperty(ref _autoLogoutDuration, value);
        }

        public int DashboardRefreshInterval
        {
            get => _dashboardRefreshInterval;
            set => SetProperty(ref _dashboardRefreshInterval, value);
        }

        public bool EnableSoundNotifications
        {
            get => _enableSoundNotifications;
            set => SetProperty(ref _enableSoundNotifications, value);
        }

        public bool RequireStudentId
        {
            get => _requireStudentId;
            set => SetProperty(ref _requireStudentId, value);
        }

        public bool ShowSessionHistory
        {
            get => _showSessionHistory;
            set => SetProperty(ref _showSessionHistory, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsSaveSuccess
        {
            get => _isSaveSuccess;
            set => SetProperty(ref _isSaveSuccess, value);
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
            try
            {
                // Load alarm threshold from database
                var settings = _settingsRepo.GetSettings();
                if (settings != null)
                {
                    AlarmThreshold = settings.AlarmThreshold;
                }

                // Load local preferences from app settings
                var appSettings = Properties.Settings.Default;
                AutoLogoutDuration = appSettings.AutoLogoutDuration;
                DashboardRefreshInterval = appSettings.DashboardRefreshInterval;
                EnableSoundNotifications = appSettings.EnableSoundNotifications;
                RequireStudentId = appSettings.RequireStudentId;
                ShowSessionHistory = appSettings.ShowSessionHistory;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to load settings: {ex.Message}";
                IsSaveSuccess = false;
            }
        }

        private void ExecuteSave(object parameter)
        {
            try
            {
                // Persist alarm threshold to database
                _settingsRepo.UpdateAlarmThreshold(AlarmThreshold);

                // Persist local preferences
                var appSettings = Properties.Settings.Default;
                appSettings.AutoLogoutDuration = AutoLogoutDuration;
                appSettings.DashboardRefreshInterval = DashboardRefreshInterval;
                appSettings.EnableSoundNotifications = EnableSoundNotifications;
                appSettings.RequireStudentId = RequireStudentId;
                appSettings.ShowSessionHistory = ShowSessionHistory;
                appSettings.Save();

                StatusMessage = "Settings saved successfully.";
                IsSaveSuccess = true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to save settings: {ex.Message}";
                IsSaveSuccess = false;
            }
        }

        private void ExecuteResetToDefaults(object parameter)
        {
            AlarmThreshold = 30;
            AutoLogoutDuration = 1;
            DashboardRefreshInterval = 30;
            EnableSoundNotifications = true;
            RequireStudentId = true;
            ShowSessionHistory = true;

            ExecuteSave(null);
            if (IsSaveSuccess)
            {
                StatusMessage = "Settings reset to defaults and saved.";
            }
        }
    }
}
