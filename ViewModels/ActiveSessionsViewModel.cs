using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Input;
using LaboratorySitInSystem.DataAccess;
using LaboratorySitInSystem.Helpers;
using LaboratorySitInSystem.Models;

namespace LaboratorySitInSystem.ViewModels
{
    public class ActiveSessionsViewModel : ViewModelBase
    {
        private readonly ISessionRepository _sessionRepo;
        private readonly ISettingsRepository _settingsRepo;
        private readonly IScheduleRepository _scheduleRepo;

        private SitInSession _selectedSession;
        private bool _isAlarmActive;
        private int _alarmThreshold;
        private string _statusMessage;

        public ObservableCollection<SitInSession> ActiveSessions { get; }
        public ObservableCollection<SitInSession> PendingSessions { get; }

        public bool IsEmpty => ActiveSessions.Count == 0;
        public bool HasPendingSessions => PendingSessions.Count > 0;

        public SitInSession SelectedSession
        {
            get => _selectedSession;
            set => SetProperty(ref _selectedSession, value);
        }

        public bool IsAlarmActive
        {
            get => _isAlarmActive;
            set => SetProperty(ref _isAlarmActive, value);
        }

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

        public RelayCommand ForceEndSessionCommand { get; }
        public RelayCommand RefreshCommand { get; }
        public RelayCommand ApproveSessionCommand { get; }
        public RelayCommand RejectSessionCommand { get; }

        public ActiveSessionsViewModel(
            ISessionRepository sessionRepo,
            ISettingsRepository settingsRepo,
            IScheduleRepository scheduleRepo)
        {
            _sessionRepo = sessionRepo ?? throw new ArgumentNullException(nameof(sessionRepo));
            _settingsRepo = settingsRepo ?? throw new ArgumentNullException(nameof(settingsRepo));
            _scheduleRepo = scheduleRepo ?? throw new ArgumentNullException(nameof(scheduleRepo));

            ActiveSessions = new ObservableCollection<SitInSession>();
            PendingSessions = new ObservableCollection<SitInSession>();

            ForceEndSessionCommand = new RelayCommand(ExecuteForceEndSession, CanForceEndSession);
            RefreshCommand = new RelayCommand(ExecuteRefresh);
            ApproveSessionCommand = new RelayCommand(ExecuteApproveSession);
            RejectSessionCommand = new RelayCommand(ExecuteRejectSession);

            // Subscribe to session change events
            SessionEventHub.SessionChanged += OnSessionChanged;

            RefreshAndCheckSessions();
            StartTimer();
        }

        private void OnSessionChanged(object sender, SessionChangedEventArgs e)
        {
            // Refresh sessions when notified of changes
            RefreshAndCheckSessions();
        }

        /// <summary>
        /// Virtual method to allow tests to override timer creation.
        /// In production, creates a timer that ticks every 30 seconds.
        /// </summary>
        protected virtual void StartTimer()
        {
            var timer = new Timer(_ => RefreshAndCheckSessions(), null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        }

        /// <summary>
        /// Core logic: refresh sessions, check alarm, auto-end expired scheduled sessions.
        /// Separated from timer for testability.
        /// </summary>
        public void RefreshAndCheckSessions()
        {
            AutoEndExpiredScheduledSessions();
            LoadActiveSessions();
            CheckAlarmThreshold();
        }

        private void LoadActiveSessions()
        {
            var sessions = _sessionRepo.GetActiveSessions();
            ActiveSessions.Clear();
            foreach (var session in sessions)
            {
                ActiveSessions.Add(session);
            }
            OnPropertyChanged(nameof(IsEmpty));

            // Also load pending sessions
            LoadPendingSessions();
        }

        private void LoadPendingSessions()
        {
            var pending = _sessionRepo.GetPendingSessions();
            PendingSessions.Clear();
            foreach (var session in pending)
            {
                PendingSessions.Add(session);
            }
            OnPropertyChanged(nameof(HasPendingSessions));
        }

        private void CheckAlarmThreshold()
        {
            var settings = _settingsRepo.GetSettings();
            AlarmThreshold = settings.AlarmThreshold;

            int activeCount = _sessionRepo.GetActiveSessionCount();
            IsAlarmActive = activeCount >= AlarmThreshold;

            if (IsAlarmActive)
            {
                StatusMessage = $"ALARM: {activeCount} active sessions (threshold: {AlarmThreshold})";
            }
            else
            {
                StatusMessage = $"{activeCount} active sessions";
            }
        }

        private void AutoEndExpiredScheduledSessions()
        {
            var sessions = _sessionRepo.GetActiveSessions();
            var now = DateTime.Now;

            foreach (var session in sessions)
            {
                if (!session.IsScheduled) continue;

                var schedule = _scheduleRepo.GetActiveSchedule(
                    session.StudentId,
                    session.StartTime.DayOfWeek,
                    session.StartTime.TimeOfDay);

                if (schedule != null && now.TimeOfDay >= schedule.EndTime)
                {
                    _sessionRepo.EndSession(session.SessionId, now);
                }
            }
        }

        private void ExecuteForceEndSession(object parameter)
        {
            if (SelectedSession == null) return;

            var result = System.Windows.MessageBox.Show(
                $"Are you sure you want to force end the session for {SelectedSession.StudentName}?\n\n" +
                "The student will be notified and will not be able to rejoin this schedule today.",
                "Force End Session",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                _sessionRepo.ForceEndSession(SelectedSession.SessionId, DateTime.Now);
                StatusMessage = $"Session for {SelectedSession.StudentName} forcefully ended.";
                SessionEventHub.NotifySessionEnded(SelectedSession.StudentId);
                SelectedSession = null;
                RefreshAndCheckSessions();
            }
        }

        private bool CanForceEndSession(object parameter)
        {
            return SelectedSession != null;
        }

        private void ExecuteRefresh(object parameter)
        {
            RefreshAndCheckSessions();
        }

        private void ExecuteApproveSession(object parameter)
        {
            if (parameter is SitInSession session)
            {
                _sessionRepo.ApproveSession(session.SessionId);
                StatusMessage = $"Approved sit-in for {session.StudentName}.";
                SessionEventHub.NotifySessionStarted(session.StudentId);
                RefreshAndCheckSessions();
            }
        }

        private void ExecuteRejectSession(object parameter)
        {
            if (parameter is SitInSession session)
            {
                var result = System.Windows.MessageBox.Show(
                    $"Are you sure you want to reject the sit-in request for {session.StudentName}?\n\n" +
                    "The student will be blocked from entering this schedule today.",
                    "Reject Sit-In Request",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    _sessionRepo.RejectSession(session.SessionId);
                    StatusMessage = $"Rejected sit-in for {session.StudentName}.";
                    RefreshAndCheckSessions();
                }
            }
        }
    }
}
