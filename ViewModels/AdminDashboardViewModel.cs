using System;
using LaboratorySitInSystem.DataAccess;
using LaboratorySitInSystem.Helpers;

namespace LaboratorySitInSystem.ViewModels
{
    public class AdminDashboardViewModel : ViewModelBase
    {
        private readonly IStudentRepository _studentRepo;
        private readonly ISessionRepository _sessionRepo;
        private readonly IScheduleRepository _scheduleRepo;
        private readonly ISettingsRepository _settingsRepo;
        private readonly IAdminRepository _adminRepo;

        private int _totalStudents;
        private int _activeSessionCount;
        private object _currentSubView;

        public int TotalStudents
        {
            get => _totalStudents;
            set => SetProperty(ref _totalStudents, value);
        }

        public int ActiveSessionCount
        {
            get => _activeSessionCount;
            set => SetProperty(ref _activeSessionCount, value);
        }

        public object CurrentSubView
        {
            get => _currentSubView;
            set => SetProperty(ref _currentSubView, value);
        }

        public RelayCommand GoToStudentManagementCommand { get; }
        public RelayCommand GoToScheduleManagementCommand { get; }
        public RelayCommand GoToActiveSessionsCommand { get; }
        public RelayCommand GoToSitInHistoryCommand { get; }
        public RelayCommand GoToSettingsCommand { get; }
        public RelayCommand GoToAboutCommand { get; }
        public RelayCommand LogoutCommand { get; }

        public AdminDashboardViewModel(
            IStudentRepository studentRepo,
            ISessionRepository sessionRepo,
            IScheduleRepository scheduleRepo,
            ISettingsRepository settingsRepo,
            IAdminRepository adminRepo)
        {
            _studentRepo = studentRepo ?? throw new ArgumentNullException(nameof(studentRepo));
            _sessionRepo = sessionRepo ?? throw new ArgumentNullException(nameof(sessionRepo));
            _scheduleRepo = scheduleRepo ?? throw new ArgumentNullException(nameof(scheduleRepo));
            _settingsRepo = settingsRepo ?? throw new ArgumentNullException(nameof(settingsRepo));
            _adminRepo = adminRepo ?? throw new ArgumentNullException(nameof(adminRepo));

            GoToStudentManagementCommand = new RelayCommand(ExecuteGoToStudentManagement);
            GoToScheduleManagementCommand = new RelayCommand(ExecuteGoToScheduleManagement);
            GoToActiveSessionsCommand = new RelayCommand(ExecuteGoToActiveSessions);
            GoToSitInHistoryCommand = new RelayCommand(ExecuteGoToSitInHistory);
            GoToSettingsCommand = new RelayCommand(ExecuteGoToSettings);
            GoToAboutCommand = new RelayCommand(ExecuteGoToAbout);
            LogoutCommand = new RelayCommand(ExecuteLogout);

            RefreshDashboard();
        }

        public void RefreshDashboard()
        {
            TotalStudents = _studentRepo.GetAll().Count;
            ActiveSessionCount = _sessionRepo.GetActiveSessionCount();
        }

        private void ExecuteGoToStudentManagement(object parameter)
        {
            CurrentSubView = new StudentManagementViewModel(_studentRepo);
        }

        private void ExecuteGoToScheduleManagement(object parameter)
        {
            CurrentSubView = new ScheduleManagementViewModel(_scheduleRepo, _studentRepo);
        }

        private void ExecuteGoToActiveSessions(object parameter)
        {
            CurrentSubView = new ActiveSessionsViewModel(_sessionRepo, _settingsRepo, _scheduleRepo);
        }

        private void ExecuteGoToSitInHistory(object parameter)
        {
            CurrentSubView = new SitInHistoryViewModel(_sessionRepo);
        }

        private void ExecuteGoToSettings(object parameter)
        {
            CurrentSubView = new SettingsViewModel(_settingsRepo);
        }

        private void ExecuteGoToAbout(object parameter)
        {
            CurrentSubView = new AboutViewModel();
        }

        private void ExecuteLogout(object parameter)
        {
            MainViewModel.Instance.NavigateTo(new LoginViewModel(_adminRepo));
        }
    }
}
