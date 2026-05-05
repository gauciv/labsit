using System;
using System.Linq;
using System.Windows.Media;
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
        private bool _showModal;
        private string _modalTitle;
        private string _modalMessage;
        private string _modalStudentName;
        private string _modalSubject;
        private string _modalStatusText;
        private Brush _modalStatusColor;
        private Brush _modalIconBg;
        private string _modalIcon;

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

        public bool ShowModal
        {
            get => _showModal;
            set => SetProperty(ref _showModal, value);
        }

        public string ModalTitle
        {
            get => _modalTitle;
            set => SetProperty(ref _modalTitle, value);
        }

        public string ModalMessage
        {
            get => _modalMessage;
            set => SetProperty(ref _modalMessage, value);
        }

        public string ModalStudentName
        {
            get => _modalStudentName;
            set => SetProperty(ref _modalStudentName, value);
        }

        public string ModalSubject
        {
            get => _modalSubject;
            set => SetProperty(ref _modalSubject, value);
        }

        public string ModalStatusText
        {
            get => _modalStatusText;
            set => SetProperty(ref _modalStatusText, value);
        }

        public Brush ModalStatusColor
        {
            get => _modalStatusColor;
            set => SetProperty(ref _modalStatusColor, value);
        }

        public Brush ModalIconBg
        {
            get => _modalIconBg;
            set => SetProperty(ref _modalIconBg, value);
        }

        public string ModalIcon
        {
            get => _modalIcon;
            set => SetProperty(ref _modalIcon, value);
        }

        public RelayCommand LoginCommand { get; }
        public RelayCommand GoBackCommand { get; }
        public RelayCommand DismissModalCommand { get; }

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
            DismissModalCommand = new RelayCommand(ExecuteDismissModal);
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
                    ShowPendingModalState(student.FullName, pendingSession.SubjectName);
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

                // Check if student was rejected for this schedule today
                var rejectedSession = _sessionRepo.GetRejectedSessionToday(StudentIdInput, schedule.SubjectName, now);
                if (rejectedSession != null)
                {
                    ShowRejectedModalState(student.FullName, schedule.SubjectName);
                    return;
                }

                // Check if student was force-ended for this schedule today
                var forceEndedSession = _sessionRepo.GetForceEndedSessionToday(StudentIdInput, schedule.SubjectName, now);
                if (forceEndedSession != null)
                {
                    ShowForceEndedModalState(student.FullName, schedule.SubjectName);
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

                ShowPendingModalState(student.FullName, schedule.SubjectName);
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

        private void ShowPendingModalState(string studentName, string subject)
        {
            ModalIcon = "⏳";
            ModalIconBg = (Brush)new BrushConverter().ConvertFrom("#FEF3C7");
            ModalTitle = "Pending Approval";
            ModalMessage = "Your sit-in request has been submitted. Please wait for admin approval before you can access the laboratory.";
            ModalStudentName = studentName;
            ModalSubject = subject;
            ModalStatusText = "Status: Waiting for admin...";
            ModalStatusColor = (Brush)new BrushConverter().ConvertFrom("#D97706");
            ShowModal = true;
        }

        private void ShowRejectedModalState(string studentName, string subject)
        {
            ModalIcon = "🚫";
            ModalIconBg = (Brush)new BrushConverter().ConvertFrom("#FEE2E2");
            ModalTitle = "Request Rejected";
            ModalMessage = "Your sit-in request has been rejected by the admin. You are not allowed to enter the laboratory for this schedule today.";
            ModalStudentName = studentName;
            ModalSubject = subject;
            ModalStatusText = "Status: Rejected by admin";
            ModalStatusColor = (Brush)new BrushConverter().ConvertFrom("#DC2626");
            ShowModal = true;
        }

        private void ShowForceEndedModalState(string studentName, string subject)
        {
            ModalIcon = "⛔";
            ModalIconBg = (Brush)new BrushConverter().ConvertFrom("#FEE2E2");
            ModalTitle = "Session Forcefully Closed";
            ModalMessage = "Your session was forcefully ended by the admin. You are not allowed to re-enter the laboratory for this schedule today.";
            ModalStudentName = studentName;
            ModalSubject = subject;
            ModalStatusText = "Status: Forcefully ended by admin";
            ModalStatusColor = (Brush)new BrushConverter().ConvertFrom("#DC2626");
            ShowModal = true;
        }

        private void ExecuteDismissModal(object parameter)
        {
            ShowModal = false;
        }

        private void ExecuteGoBack(object parameter)
        {
            MainViewModel.Instance.NavigateTo(new LoginViewModel(new AdminRepository()));
        }
    }
}
