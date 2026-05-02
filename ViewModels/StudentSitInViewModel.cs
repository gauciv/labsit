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
                System.Diagnostics.Debug.WriteLine($"[SITIN] Student found: {student.FullName} ({student.StudentId})");

                var activeSession = _sessionRepo.GetActiveSessionByStudent(StudentIdInput);
                if (activeSession != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[SITIN] Student already has active session");
                    // Student already has an active session — show their dashboard
                    var existingSchedule = _scheduleRepo.GetActiveSchedule(
                        StudentIdInput, DateTime.Now.DayOfWeek, DateTime.Now.TimeOfDay);
                    MainViewModel.Instance.NavigateTo(new StudentSessionDashboardViewModel(
                        student, activeSession, existingSchedule, _sessionRepo, _scheduleRepo, _studentRepo));
                    return;
                }

                var now = DateTime.Now;
                var currentDay = now.DayOfWeek;
                var currentTime = now.TimeOfDay;
                
                System.Diagnostics.Debug.WriteLine($"[SITIN] Current time: {currentDay} {currentTime:hh\\:mm\\:ss}");
                
                // Check if student has any schedules at all
                var allSchedules = _scheduleRepo.GetByStudentId(StudentIdInput);
                System.Diagnostics.Debug.WriteLine($"[SITIN] Student has {allSchedules?.Count ?? 0} total schedules");
                
                // If student has no schedules, they cannot sit in
                if (allSchedules == null || allSchedules.Count == 0)
                {
                    StatusMessage = "Access denied. You have no class schedules registered in the system.";
                    System.Diagnostics.Debug.WriteLine($"[SITIN] DENIED: No schedules found");
                    return;
                }
                
                // Log all student's schedules for debugging
                foreach (var sched in allSchedules)
                {
                    System.Diagnostics.Debug.WriteLine($"[SITIN] Schedule: {sched.SubjectName} on {sched.DayOfWeek} from {sched.StartTime} to {sched.EndTime}");
                }
                
                // Check if current time matches any schedule
                var schedule = _scheduleRepo.GetActiveSchedule(StudentIdInput, currentDay, currentTime);
                System.Diagnostics.Debug.WriteLine($"[SITIN] Active schedule found: {schedule?.SubjectName ?? "None"}");
                
                // If student has schedules but current time doesn't match any, they cannot sit in
                if (schedule == null)
                {
                    // Get today's schedules to show in error message
                    var todaySchedules = _scheduleRepo.GetTodaySchedules(StudentIdInput, currentDay);
                    
                    if (todaySchedules.Count > 0)
                    {
                        var scheduleList = string.Join(", ", todaySchedules.Select(s => $"{s.SubjectName} ({s.StartTime:hh\\:mm}-{s.EndTime:hh\\:mm})"));
                        StatusMessage = $"Access denied. Current time ({currentTime:hh\\:mm}) does not fall within your scheduled classes today.\n\nToday's schedules: {scheduleList}";
                    }
                    else
                    {
                        StatusMessage = $"Access denied. You have no scheduled classes today ({currentDay}).";
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"[SITIN] DENIED: No active schedule at current time");
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine($"[SITIN] ACCESS GRANTED: {schedule.SubjectName}");
                MatchedSchedule = schedule;

                var session = new SitInSession
                {
                    StudentId = StudentIdInput,
                    StudentName = student.FullName,
                    SubjectName = schedule.SubjectName,
                    StartTime = now,
                    IsScheduled = true
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
