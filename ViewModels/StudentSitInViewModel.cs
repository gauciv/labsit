using System;
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

        public RelayCommand LoginCommand { get; }
        public RelayCommand GoBackCommand { get; }

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

                var activeSession = _sessionRepo.GetActiveSessionByStudent(StudentIdInput);
                if (activeSession != null)
                {
                    // Student already has an active session — show their dashboard
                    var existingSchedule = _scheduleRepo.GetActiveSchedule(
                        StudentIdInput, DateTime.Now.DayOfWeek, DateTime.Now.TimeOfDay);
                    MainViewModel.Instance.NavigateTo(new StudentSessionDashboardViewModel(
                        student, activeSession, existingSchedule, _sessionRepo, _scheduleRepo, _studentRepo));
                    return;
                }

                var now = DateTime.Now;
                var schedule = _scheduleRepo.GetActiveSchedule(StudentIdInput, now.DayOfWeek, now.TimeOfDay);
                MatchedSchedule = schedule;

                var session = new SitInSession
                {
                    StudentId = StudentIdInput,
                    StudentName = student.FullName,
                    SubjectName = schedule?.SubjectName,
                    StartTime = now,
                    IsScheduled = schedule != null
                };

                _sessionRepo.StartSession(session);

                // Notify that a session was created
                SessionEventHub.NotifySessionStarted(StudentIdInput);

                // Fetch the session back to get the auto-generated SessionId
                var startedSession = _sessionRepo.GetActiveSessionByStudent(StudentIdInput);

                // Navigate to the session dashboard
                MainViewModel.Instance.NavigateTo(new StudentSessionDashboardViewModel(
                    student, startedSession, schedule, _sessionRepo, _scheduleRepo, _studentRepo));
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                StatusMessage = $"Database connection failed. Is XAMPP MySQL running?\n\nDetails: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[SITIN ERROR] MySqlException: {ex}");
            }
            catch (Exception ex)
            {
                StatusMessage = $"An unexpected error occurred: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[SITIN ERROR] Exception: {ex}");
            }
        }

        private void ExecuteGoBack(object parameter)
        {
            MainViewModel.Instance.NavigateTo(new LoginViewModel(
                new AdminRepository()));
        }
    }
}
