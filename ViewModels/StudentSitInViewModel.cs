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

            var student = _studentRepo.GetById(StudentIdInput);
            if (student == null)
            {
                StatusMessage = "Student not found";
                return;
            }

            CurrentStudent = student;

            var activeSession = _sessionRepo.GetActiveSessionByStudent(StudentIdInput);
            if (activeSession != null)
            {
                StatusMessage = "Student already has an active session";
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

            var subjectDisplay = schedule != null ? schedule.SubjectName : "Walk-in";
            StatusMessage = $"Welcome, {student.FullName}! Session started at {now:hh:mm tt} — {subjectDisplay}";
        }

        private void ExecuteGoBack(object parameter)
        {
            MainViewModel.Instance.NavigateTo(new LoginViewModel(
                new AdminRepository()));
        }
    }
}
