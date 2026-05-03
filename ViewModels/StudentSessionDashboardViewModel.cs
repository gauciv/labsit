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
        private Timer _timer;

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
            
            // Debug logging for schedule information
            System.Diagnostics.Debug.WriteLine($"[DASHBOARD_INIT] Student: {student?.FullName}");
            System.Diagnostics.Debug.WriteLine($"[DASHBOARD_INIT] Session started: {activeSession?.StartTime}");
            if (activeSchedule != null)
            {
                System.Diagnostics.Debug.WriteLine($"[DASHBOARD_INIT] Schedule: {activeSchedule.SubjectName}");
                System.Diagnostics.Debug.WriteLine($"[DASHBOARD_INIT] Schedule time: {activeSchedule.StartTime:hh\\:mm} - {activeSchedule.EndTime:hh\\:mm}");
                System.Diagnostics.Debug.WriteLine($"[DASHBOARD_INIT] Schedule day: {activeSchedule.DayOfWeek}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[DASHBOARD_INIT] No active schedule provided");
            }

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
                System.Diagnostics.Debug.WriteLine($"[DASHBOARD] LoadDashboardData started for student: {Student?.StudentId}");
                
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
                var currentDay = DateTime.Now.DayOfWeek;
                System.Diagnostics.Debug.WriteLine($"[DASHBOARD] Current day: {currentDay} ({(int)currentDay})");
                
                // First, let's check if the student has ANY schedules at all
                var allSchedules = _scheduleRepo.GetByStudentId(Student.StudentId);
                System.Diagnostics.Debug.WriteLine($"[DASHBOARD] Student has {allSchedules?.Count ?? 0} total schedules");
                if (allSchedules != null)
                {
                    foreach (var sched in allSchedules)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DASHBOARD] All schedules - {sched.SubjectName} on {sched.DayOfWeek} ({(int)sched.DayOfWeek}) from {sched.StartTime} to {sched.EndTime}");
                    }
                }
                
                var todayScheds = _scheduleRepo.GetTodaySchedules(Student.StudentId, currentDay);
                System.Diagnostics.Debug.WriteLine($"[DASHBOARD] GetTodaySchedules returned {todayScheds?.Count ?? 0} schedules");
                
                if (todayScheds != null)
                {
                    foreach (var sched in todayScheds)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DASHBOARD] Adding schedule: {sched.SubjectName} on {sched.DayOfWeek} ({(int)sched.DayOfWeek}) from {sched.StartTime} to {sched.EndTime}");
                        TodaySchedules.Add(sched);
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"[DASHBOARD] TodaySchedules.Count: {TodaySchedules.Count}");
                System.Diagnostics.Debug.WriteLine($"[DASHBOARD] IsTodaySchedulesEmpty: {IsTodaySchedulesEmpty}");
                
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
            var now = DateTime.Now;
            var currentTime = now.TimeOfDay;
            
            System.Diagnostics.Debug.WriteLine($"[TIME_UPDATE] Current time: {currentTime:hh\\:mm\\:ss}");
            
            if (ActiveSchedule != null)
            {
                System.Diagnostics.Debug.WriteLine($"[TIME_UPDATE] Schedule: {ActiveSchedule.SubjectName} ends at {ActiveSchedule.EndTime:hh\\:mm\\:ss}");
                
                var remaining = ActiveSchedule.EndTime - currentTime;
                System.Diagnostics.Debug.WriteLine($"[TIME_UPDATE] Calculated remaining: {remaining:hh\\:mm\\:ss}");

                if (remaining.TotalSeconds <= 0)
                {
                    TimeRemainingDisplay = "00:00:00";
                    SessionWarning = "Your session time has ended.";
                    ShowWarning = true;
                    System.Diagnostics.Debug.WriteLine($"[TIME_UPDATE] Session ended");
                }
                else
                {
                    // Ensure we show positive time remaining
                    TimeRemainingDisplay = remaining.ToString(@"hh\:mm\:ss");
                    System.Diagnostics.Debug.WriteLine($"[TIME_UPDATE] Display: {TimeRemainingDisplay}");

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
                // Walk-in: show elapsed time (this shouldn't happen with new validation)
                var elapsed = now - ActiveSession.StartTime;
                TimeRemainingDisplay = elapsed.ToString(@"hh\:mm\:ss");
                ShowWarning = false;
                System.Diagnostics.Debug.WriteLine($"[TIME_UPDATE] Walk-in mode - elapsed: {TimeRemainingDisplay}");
            }
        }

        protected virtual void StartTimer()
        {
            // Dispose existing timer if any
            _timer?.Dispose();
            
            System.Diagnostics.Debug.WriteLine("[TIMER] Starting countdown timer");
            
            _timer = new Timer(_ =>
            {
                try
                {
                    Application.Current?.Dispatcher?.Invoke(() => 
                    {
                        UpdateTimeRemaining();
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[TIMER ERROR] {ex.Message}");
                }
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1)); // Start immediately, then every second
        }

        // Add cleanup method
        public void Dispose()
        {
            _timer?.Dispose();
        }

        private void ExecuteEndSessionEarly(object parameter)
        {
            var result = MessageBox.Show(
                "Are you sure you want to end your session early?\n\n" +
                "⚠️ WARNING: You will NOT be able to rejoin this class session today.\n" +
                "This action is permanent and cannot be undone.",
                "End Session Early",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _sessionRepo.EndSessionEarly(ActiveSession.SessionId, DateTime.Now);
                    StatusMessage = "Session ended early. You cannot rejoin this class today.";

                    // Notify that a session was ended
                    SessionEventHub.NotifySessionEnded(Student.StudentId);

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
