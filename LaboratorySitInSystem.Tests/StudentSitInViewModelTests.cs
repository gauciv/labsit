using System;
using LaboratorySitInSystem.DataAccess;
using LaboratorySitInSystem.Models;
using LaboratorySitInSystem.ViewModels;
using Moq;
using Xunit;

namespace LaboratorySitInSystem.Tests
{
    public class StudentSitInViewModelTests
    {
        private readonly Mock<IStudentRepository> _mockStudentRepo;
        private readonly Mock<IScheduleRepository> _mockScheduleRepo;
        private readonly Mock<ISessionRepository> _mockSessionRepo;
        private readonly StudentSitInViewModel _vm;

        public StudentSitInViewModelTests()
        {
            _mockStudentRepo = new Mock<IStudentRepository>();
            _mockScheduleRepo = new Mock<IScheduleRepository>();
            _mockSessionRepo = new Mock<ISessionRepository>();
            _vm = new StudentSitInViewModel(
                _mockStudentRepo.Object,
                _mockScheduleRepo.Object,
                _mockSessionRepo.Object);
        }

        [Fact]
        public void LoginCommand_ValidStudentWithMatchingSchedule_StartsSessionWithIsScheduledTrue()
        {
            var student = new Student
            {
                StudentId = "S001",
                FirstName = "John",
                LastName = "Doe",
                Course = "BSCS",
                YearLevel = 2
            };
            var schedule = new ClassSchedule
            {
                ScheduleId = 1,
                StudentId = "S001",
                SubjectName = "Data Structures",
                DayOfWeek = DateTime.Now.DayOfWeek,
                StartTime = DateTime.Now.TimeOfDay.Subtract(TimeSpan.FromMinutes(30)),
                EndTime = DateTime.Now.TimeOfDay.Add(TimeSpan.FromMinutes(30))
            };

            _mockStudentRepo.Setup(r => r.GetById("S001")).Returns(student);
            _mockSessionRepo.Setup(r => r.GetActiveSessionByStudent("S001")).Returns((SitInSession?)null);
            _mockScheduleRepo.Setup(r => r.GetActiveSchedule("S001", It.IsAny<DayOfWeek>(), It.IsAny<TimeSpan>()))
                .Returns(schedule);

            _vm.StudentIdInput = "S001";
            _vm.LoginCommand.Execute(null);

            _mockSessionRepo.Verify(r => r.StartSession(It.Is<SitInSession>(s =>
                s.StudentId == "S001" &&
                s.IsScheduled == true &&
                s.SubjectName == "Data Structures" &&
                s.StudentName == "John Doe"
            )), Times.Once);

            Assert.Equal(student, _vm.CurrentStudent);
            Assert.Equal(schedule, _vm.MatchedSchedule);
            Assert.Contains("John Doe", _vm.StatusMessage);
            Assert.Contains("Data Structures", _vm.StatusMessage);
        }

        [Fact]
        public void LoginCommand_ValidStudentNoMatchingSchedule_StartsSessionWithIsScheduledFalse()
        {
            var student = new Student
            {
                StudentId = "S002",
                FirstName = "Jane",
                LastName = "Smith",
                Course = "BSIT",
                YearLevel = 1
            };

            _mockStudentRepo.Setup(r => r.GetById("S002")).Returns(student);
            _mockSessionRepo.Setup(r => r.GetActiveSessionByStudent("S002")).Returns((SitInSession?)null);
            _mockScheduleRepo.Setup(r => r.GetActiveSchedule("S002", It.IsAny<DayOfWeek>(), It.IsAny<TimeSpan>()))
                .Returns((ClassSchedule?)null);

            _vm.StudentIdInput = "S002";
            _vm.LoginCommand.Execute(null);

            _mockSessionRepo.Verify(r => r.StartSession(It.Is<SitInSession>(s =>
                s.StudentId == "S002" &&
                s.IsScheduled == false &&
                s.SubjectName == null
            )), Times.Once);

            Assert.Null(_vm.MatchedSchedule);
            Assert.Contains("Walk-in", _vm.StatusMessage);
        }

        [Fact]
        public void LoginCommand_StudentWithActiveSession_RejectsWithErrorMessage()
        {
            var student = new Student
            {
                StudentId = "S003",
                FirstName = "Bob",
                LastName = "Lee",
                Course = "BSCS",
                YearLevel = 3
            };
            var activeSession = new SitInSession
            {
                SessionId = 10,
                StudentId = "S003",
                StartTime = DateTime.Now.AddHours(-1)
            };

            _mockStudentRepo.Setup(r => r.GetById("S003")).Returns(student);
            _mockSessionRepo.Setup(r => r.GetActiveSessionByStudent("S003")).Returns(activeSession);

            _vm.StudentIdInput = "S003";
            _vm.LoginCommand.Execute(null);

            Assert.Equal("Student already has an active session", _vm.StatusMessage);
            _mockSessionRepo.Verify(r => r.StartSession(It.IsAny<SitInSession>()), Times.Never);
        }

        [Fact]
        public void LoginCommand_InvalidStudentId_ShowsErrorMessage()
        {
            _mockStudentRepo.Setup(r => r.GetById("INVALID")).Returns((Student?)null);

            _vm.StudentIdInput = "INVALID";
            _vm.LoginCommand.Execute(null);

            Assert.Equal("Student not found", _vm.StatusMessage);
            Assert.Null(_vm.CurrentStudent);
            _mockSessionRepo.Verify(r => r.StartSession(It.IsAny<SitInSession>()), Times.Never);
        }
    }
}
