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
        private bool _isApprovalPopupOpen;
        private SitInSession _pendingApprovalSession;

        public ObservableCollection<SitInSession> ActiveSessions { get; }
        public ObservableCollection<SitInSession> PendingApprovals { get; }

        public bool IsEmpty => ActiveSessions.Count == 0;

        public bool IsApprovalPopupOpen
        {
            get => _isApprovalPopupOpen;
            set => SetProperty(ref _isApprovalPopupOpen, value);
        }

        public SitInSession PendingApprovalSession
        {
            get => _pendingApprovalSession;
            set => SetProperty(ref _pendingApprovalSession, value);
        }

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
        public RelayCommand DeclineSessionCommand { get; }

        public ActiveSessionsViewModel(
            ISessionRepository sessionRepo,
            ISettingsRepository settingsRepo,
            IScheduleRepository scheduleRepo)
        {
            _sessionRepo = sessionRepo ?? throw new ArgumentNullException(nameof(sessionRepo));
            _settingsRepo = settingsRepo ?? throw new ArgumentNullException(nameof(settingsRepo));
            _scheduleRepo = scheduleRepo ?? throw new ArgumentNullException(nameof(scheduleRepo));

            ActiveSessions = new ObservableCollection<SitInSession>();
            PendingApprovals = new ObservableCollection<SitInSession>();

            ForceEndSessionCommand = new RelayCommand(ExecuteForceEndSession, CanForceEndSession);
            RefreshCommand = new RelayCommand(ExecuteRefresh);
            ApproveSessionCommand = new RelayCommand(ExecuteApproveSession);
            DeclineSessionCommand = new RelayCommand(ExecuteDeclineSession);

            RefreshAndCheckSessions();
            StartTimer();
            
            // TEMPORARY: Add dummy pending approval for screenshot/testing
            AddDummyPendingApproval();
        }

        /// <summary>
        /// TEMPORARY METHOD: Adds a dummy pending approval session for testing/screenshot purposes.
        /// Remove this method and the call in the constructor when done testing.
        /// </summary>
        private void AddDummyPendingApproval()
        {
            var dummySession = new SitInSession
            {
                SessionId = 999,
                StudentId = "2021-12345",
                StudentName = "Juan Dela Cruz",
                SubjectName = "Computer Programming 101",
                StartTime = DateTime.Now,
                IsScheduled = false,
                RequiresApproval = true,
                IsApproved = false
            };
            
            PendingApprovals.Add(dummySession);
            
            // Automatically show the popup
            PendingApprovalSession = dummySession;
            IsApprovalPopupOpen = true;
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
            PendingApprovals.Clear();
            
            foreach (var session in sessions)
            {
                if (session.RequiresApproval && !session.IsApproved)
                {
                    PendingApprovals.Add(session);
                }
                else
                {
                    ActiveSessions.Add(session);
                }
            }
            OnPropertyChanged(nameof(IsEmpty));
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

            _sessionRepo.EndSession(SelectedSession.SessionId, DateTime.Now);
            StatusMessage = $"Session for {SelectedSession.StudentName} ended.";
            SelectedSession = null;
            RefreshAndCheckSessions();
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
            if (PendingApprovalSession == null) return;

            // Update session approval status
            PendingApprovalSession.IsApproved = true;
            PendingApprovalSession.RequiresApproval = false;
            
            // Update in database via repository
            _sessionRepo.ApproveSession(PendingApprovalSession.SessionId);

            StatusMessage = $"Session for {PendingApprovalSession.StudentName} approved.";
            
            // Close popup and refresh
            IsApprovalPopupOpen = false;
            PendingApprovalSession = null;
            RefreshAndCheckSessions();
        }

        private void ExecuteDeclineSession(object parameter)
        {
            if (PendingApprovalSession == null) return;

            // End the session since it was declined
            _sessionRepo.EndSession(PendingApprovalSession.SessionId, DateTime.Now);
            
            StatusMessage = $"Session request for {PendingApprovalSession.StudentName} declined.";
            
            // Close popup and refresh
            IsApprovalPopupOpen = false;
            PendingApprovalSession = null;
            RefreshAndCheckSessions();
        }
    }
}
