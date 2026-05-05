using System;
using System.Linq;
using LaboratorySitInSystem.DataAccess;
using LaboratorySitInSystem.Helpers;
using LaboratorySitInSystem.Models;

namespace LaboratorySitInSystem.ViewModels
{
    public class StudentSitInViewModel : ViewModelBase
    {
        private readonly IStudentRepository _studentRepo;
        private readonly IScheduleRepository _scheduleRepo;
        private readonly ISessionRepository _sessionRepo;

        private string _studentIdInput;
        private Student _currentStudent;
        private ClassSchedule _matchedSchedule;
        private string _statusMessage;
        private bool _showPendingModal;
        private string _pendingStudentName;
        private string _pendingSubject;

        public string StudentIdInput
        {
            get => _studentIdInput;
            set => SetProperty(ref _studentIdInput, value);
        }

        public Student CurrentStudent
        {
            get => _currentStudent;
            set => SetProperty(ref _currentStudent, value);
        }

        public ClassSchedule MatchedSchedule
        {
            get => _matchedSchedule;
            set => SetProperty(ref _matchedSchedule, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool ShowPendingModal
        {
            get => _showPendingModal;
            set => SetProperty(ref _showPendingModal, value);
        }

        public string PendingStudentName
        {
            get => _pendingStudentName;
            set => SetProperty(ref _pendingStudentName, value);
        }

        public string PendingSubject
        {
            get => _pendingSubject;
            set => SetProperty(ref _pendingSubject, value);
        }

        public RelayCommand LoginCommand { get; }
        public RelayCommand GoBackCommand { get; }
        public RelayCommand DismissPendingCommand { get; }

        public StudentSitInViewModel(
            IStudentRepository studentRepo,
            IScheduleRepository scheduleRepo,
            ISessionRepository sessionRepo)
        {
            _studentRepo = studentRepo ?? throw new ArgumentNullException(nameof(studentRepo));
            _scheduleRepo = scheduleRepo ?? throw new ArgumentNullException(nameof(scheduleRepo));
            _sessionRepo = sessionRepo ?? throw new ArgumentNullException(nameof(sessionRepo));

            LoginCommand = new RelayCommand(ExecuteLogin);
            GoBackCommand = new RelayCommand(ExecuteGoBack);
            DismissPendingCommand = new RelayCommand(ExecuteDismissPending);
        }

        private void ExecuteLogin(object parameter)
        {
            StatusMessage = string.Empty;
            CurrentStudent = null;
            MatchedSchedule = null;

            if (string.IsNullOrWhiteSpace(StudentIdInput))
            {
                StatusMessage = "Please enter a Student ID.";
                return;
            }

            try
            {
                var student = _studentRepo.GetById(StudentIdInput);
                if (student == null)
                {
                    StatusMessage = "Student not found.";
                    return;
                }

                CurrentStudent = student;

                // Check if student has an approved active session — go straight to dashboard
                var activeSession = _sessionRepo.GetActiveSessionByStudent(StudentIdInput);
                if (activeSession != null)
                {
                    var existingSchedule = _scheduleRepo.GetActiveSchedule(
                        StudentIdInput, DateTime.Now.DayOfWeek, DateTime.Now.TimeOfDay);
                    MainViewModel.Instance.NavigateTo(new StudentSessionDashboardViewModel(
                        student, activeSession, existingSchedule, _sessionRepo, _scheduleRepo, _studentRepo));
                    return;
                }

                // Check if student already has a pending request
                var pendingSession = _sessionRepo.GetPendingSessionByStudent(StudentIdInput);
                if (pendingSession != null)
                {
                    // Already pending — show the pending modal again
                    PendingStudentName = student.FullName;
                    PendingSubject = pendingSession.SubjectName;
                    ShowPendingModal = true;
                    return;
                }

                // Validate schedule
                var now = DateTime.Now;
                var currentDay = now.DayOfWeek;
                var currentTime = now.TimeOfDay;

                var allSchedules = _scheduleRepo.GetByStudentId(StudentIdInput);
                if (allSchedules == null || allSchedules.Count == 0)
                {
                    StatusMessage = "Access denied. You have no class schedules registered in the system.";
                    return;
                }

                var schedule = _scheduleRepo.GetActiveSchedule(StudentIdInput, currentDay, currentTime);
                if (schedule == null)
                {
                    var todaySchedules = _scheduleRepo.GetTodaySchedules(StudentIdInput, currentDay);
                    if (todaySchedules.Count > 0)
                    {
                        var scheduleList = string.Join(", ", todaySchedules.Select(s => 
                            $"{s.SubjectName} ({s.StartTime:hh\\:mm}-{s.EndTime:hh\\:mm})"));
                        StatusMessage = $"Access denied. Current time ({currentTime:hh\\:mm}) does not fall within your scheduled classes today.\n\nToday's schedules: {scheduleList}";
                    }
                    else
                    {
                        StatusMessage = $"Access denied. You have no scheduled classes today ({currentDay}).";
                    }
                    return;
                }

                // Check if student ended early today
                var hasEndedEarly = _sessionRepo.HasEndedSessionEarlyToday(StudentIdInput, schedule.SubjectName, now);
                if (hasEndedEarly)
                {
                    StatusMessage = $"Access denied. You have already ended your session early for {schedule.SubjectName} today and cannot rejoin.";
                    return;
                }

                MatchedSchedule = schedule;

                // Create a PENDING session — requires admin approval
                var session = new SitInSession
                {
                    StudentId = StudentIdInput,
                    StudentName = student.FullName,
                    SubjectName = schedule.SubjectName,
                    StartTime = now,
                    IsScheduled = true,
                    Status = "pending"
                };

                _sessionRepo.StartSession(session);
                SessionEventHub.NotifySessionStarted(StudentIdInput);

                // Show pending modal
                PendingStudentName = student.FullName;
                PendingSubject = schedule.SubjectName;
                ShowPendingModal = true;
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                StatusMessage = $"Database connection failed. Is XAMPP MySQL running?\n\nDetails: {ex.Message}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"An unexpected error occurred: {ex.Message}";
            }
        }

        private void ExecuteDismissPending(object parameter)
        {
            ShowPendingModal = false;
        }

        private void ExecuteGoBack(object parameter)
        {
            MainViewModel.Instance.NavigateTo(new LoginViewModel(new AdminRepository()));
        }
    }
}
