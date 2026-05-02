using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using LaboratorySitInSystem.DataAccess;
using LaboratorySitInSystem.Helpers;
using LaboratorySitInSystem.Models;

namespace LaboratorySitInSystem.ViewModels
{
    public class StudentSessionDashboardViewModel : ViewModelBase
    {
        private readonly ISessionRepository _sessionRepo;
        private readonly IScheduleRepository _scheduleRepo;
        private readonly IStudentRepository _studentRepo;

        private Student _student;
        private SitInSession _activeSession;
        private ClassSchedule _activeSchedule;
        private string _timeRemainingDisplay;
        private string _sessionWarning;
        private bool _showWarning;
        private string _timeInDisplay;
        private string _allowedDurationDisplay;
        private int _sitInCount;
        private string _statusMessage;

        public Student Student
        {
            get => _student;
            set => SetProperty(ref _student, value);
        }

        public SitInSession ActiveSession
        {
            get => _activeSession;
            set => SetProperty(ref _activeSession, value);
        }

        public ClassSchedule ActiveSchedule
        {
            get => _activeSchedule;
            set => SetProperty(ref _activeSchedule, value);
        }

        public string TimeRemainingDisplay
        {
            get => _timeRemainingDisplay;
            set => SetProperty(ref _timeRemainingDisplay, value);
        }

        public string SessionWarning
        {
            get => _sessionWarning;
            set => SetProperty(ref _sessionWarning, value);
        }

        public bool ShowWarning
        {
            get => _showWarning;
            set => SetProperty(ref _showWarning, value);
        }

        public string TimeInDisplay
        {
            get => _timeInDisplay;
            set => SetProperty(ref _timeInDisplay, value);
        }

        public string AllowedDurationDisplay
        {
            get => _allowedDurationDisplay;
            set => SetProperty(ref _allowedDurationDisplay, value);
        }

        public int SitInCount
        {
            get => _sitInCount;
            set => SetProperty(ref _sitInCount, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ObservableCollection<ClassSchedule> TodaySchedules { get; }
        public ObservableCollection<SitInSession> RecentHistory { get; }

        public bool IsTodaySchedulesEmpty => TodaySchedules.Count == 0;
        public bool IsRecentHistoryEmpty => RecentHistory.Count == 0;

        public RelayCommand EndSessionEarlyCommand { get; }
        public RelayCommand LogoutCommand { get; }

        public StudentSessionDashboardViewModel(
            Student student,
            SitInSession activeSession,
            ClassSchedule activeSchedule,
            ISessionRepository sessionRepo,
            IScheduleRepository scheduleRepo,
            IStudentRepository studentRepo)
        {
            _sessionRepo = sessionRepo;
            _scheduleRepo = scheduleRepo;
            _studentRepo = studentRepo;

            Student = student;
            ActiveSession = activeSession;
            ActiveSchedule = activeSchedule;

            TodaySchedules = new ObservableCollection<ClassSchedule>();
            RecentHistory = new ObservableCollection<SitInSession>();

            EndSessionEarlyCommand = new RelayCommand(ExecuteEndSessionEarly);
            LogoutCommand = new RelayCommand(ExecuteLogout);

            LoadDashboardData();
            StartTimer();
        }

        private void LoadDashboardData()
        {
            try
            {
                // Time in
                TimeInDisplay = ActiveSession.StartTime.ToString("h:mm tt");

                // Allowed duration from schedule
                if (ActiveSchedule != null)
                {
                    var duration = ActiveSchedule.EndTime - ActiveSchedule.StartTime;
                    if (duration.TotalHours >= 1)
                        AllowedDurationDisplay = $"{duration.TotalHours:0.#} hrs";
                    else
                        AllowedDurationDisplay = $"{duration.TotalMinutes:0} min";
                }
                else
                {
                    AllowedDurationDisplay = "Walk-in";
                }

                // Sit-in count
                SitInCount = _sessionRepo.GetStudentSitInCount(Student.StudentId);

                // Today's schedules
                TodaySchedules.Clear();
                var todayScheds = _scheduleRepo.GetTodaySchedules(Student.StudentId, DateTime.Now.DayOfWeek);
                foreach (var sched in todayScheds)
                    TodaySchedules.Add(sched);
                OnPropertyChanged(nameof(IsTodaySchedulesEmpty));

                // Recent history
                RecentHistory.Clear();
                var history = _sessionRepo.GetStudentRecentHistory(Student.StudentId, 10);
                foreach (var h in history)
                    RecentHistory.Add(h);
                OnPropertyChanged(nameof(IsRecentHistoryEmpty));

                UpdateTimeRemaining();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading data: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[DASHBOARD ERROR] {ex}");
            }
        }

        public void UpdateTimeRemaining()
        {
            if (ActiveSchedule != null)
            {
                var now = DateTime.Now.TimeOfDay;
                var remaining = ActiveSchedule.EndTime - now;

                if (remaining.TotalSeconds <= 0)
                {
                    TimeRemainingDisplay = "00:00:00";
                    SessionWarning = "Your session time has ended.";
                    ShowWarning = true;
                }
                else
                {
                    TimeRemainingDisplay = remaining.ToString(@"hh\:mm\:ss");

                    if (remaining.TotalMinutes <= 15)
                    {
                        SessionWarning = $"Your session ends in {(int)remaining.TotalMinutes} minutes. Please save your work.";
                        ShowWarning = true;
                    }
                    else
                    {
                        ShowWarning = false;
                    }
                }
            }
            else
            {
                // Walk-in: show elapsed time
                var elapsed = DateTime.Now - ActiveSession.StartTime;
                TimeRemainingDisplay = elapsed.ToString(@"hh\:mm\:ss");
                ShowWarning = false;
            }
        }

        protected virtual void StartTimer()
        {
            var timer = new Timer(_ =>
            {
                try
                {
                    Application.Current?.Dispatcher?.Invoke(() => UpdateTimeRemaining());
                }
                catch { /* UI thread may be gone */ }
            }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        private void ExecuteEndSessionEarly(object parameter)
        {
            var result = MessageBox.Show(
                "Are you sure you want to end your session early?\n\nYou will not be able to rejoin this session.",
                "End Session Early",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _sessionRepo.EndSessionEarly(ActiveSession.SessionId, DateTime.Now);
                    StatusMessage = "Session ended. Returning to login...";

                    MainViewModel.Instance.NavigateTo(new StudentSitInViewModel(
                        _studentRepo, _scheduleRepo, _sessionRepo));
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Failed to end session: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine($"[END SESSION ERROR] {ex}");
                }
            }
        }

        private void ExecuteLogout(object parameter)
        {
            MainViewModel.Instance.NavigateTo(new LoginViewModel(new AdminRepository()));
        }
    }
}
