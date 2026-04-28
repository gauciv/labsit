using System;
using System.Collections.Generic;
using LaboratorySitInSystem.DataAccess;
using LaboratorySitInSystem.Models;
using LaboratorySitInSystem.ViewModels;
using Moq;
using Xunit;

namespace LaboratorySitInSystem.Tests
{
    public class ScheduleManagementViewModelTests
    {
        private readonly Mock<IScheduleRepository> _mockScheduleRepo;
        private readonly Mock<IStudentRepository> _mockStudentRepo;

        private readonly Student _testStudent = new Student
        {
            StudentId = "S001", FirstName = "John", LastName = "Doe", Course = "BSCS", YearLevel = 2
        };

        public ScheduleManagementViewModelTests()
        {
            _mockScheduleRepo = new Mock<IScheduleRepository>();
            _mockStudentRepo = new Mock<IStudentRepository>();
            _mockStudentRepo.Setup(r => r.GetAll()).Returns(new List<Student>());
            _mockScheduleRepo.Setup(r => r.GetByStudentId(It.IsAny<string>())).Returns(new List<ClassSchedule>());
        }

        private ScheduleManagementViewModel CreateVm(List<Student> students = null)
        {
            if (students != null)
                _mockStudentRepo.Setup(r => r.GetAll()).Returns(students);
            return new ScheduleManagementViewModel(_mockScheduleRepo.Object, _mockStudentRepo.Object);
        }

        [Fact]
        public void Constructor_LoadsStudents()
        {
            var students = new List<Student> { _testStudent };
            var vm = CreateVm(students);
            Assert.Single(vm.Students);
            Assert.Equal("S001", vm.Students[0].StudentId);
        }

        [Fact]
        public void Constructor_InitializesAllCommands()
        {
            var vm = CreateVm();
            Assert.NotNull(vm.AddCommand);
            Assert.NotNull(vm.UpdateCommand);
            Assert.NotNull(vm.DeleteCommand);
            Assert.NotNull(vm.ClearFormCommand);
        }

        [Fact]
        public void Constructor_ThrowsOnNullScheduleRepo()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ScheduleManagementViewModel(null!, _mockStudentRepo.Object));
        }

        [Fact]
        public void Constructor_ThrowsOnNullStudentRepo()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ScheduleManagementViewModel(_mockScheduleRepo.Object, null!));
        }

        [Fact]
        public void SelectedStudent_LoadsSchedules()
        {
            var schedules = new List<ClassSchedule>
            {
                new ClassSchedule { ScheduleId = 1, StudentId = "S001", SubjectName = "Math", DayOfWeek = DayOfWeek.Monday, StartTime = TimeSpan.FromHours(8), EndTime = TimeSpan.FromHours(10) }
            };
            _mockScheduleRepo.Setup(r => r.GetByStudentId("S001")).Returns(schedules);

            var vm = CreateVm(new List<Student> { _testStudent });
            vm.SelectedStudent = _testStudent;

            Assert.Single(vm.Schedules);
            Assert.Equal("Math", vm.Schedules[0].SubjectName);
        }

        [Fact]
        public void SelectedStudent_SetToNull_ClearsSchedules()
        {
            var vm = CreateVm(new List<Student> { _testStudent });
            _mockScheduleRepo.Setup(r => r.GetByStudentId("S001")).Returns(new List<ClassSchedule>
            {
                new ClassSchedule { ScheduleId = 1, StudentId = "S001", SubjectName = "Math", DayOfWeek = DayOfWeek.Monday, StartTime = TimeSpan.FromHours(8), EndTime = TimeSpan.FromHours(10) }
            });
            vm.SelectedStudent = _testStudent;
            Assert.Single(vm.Schedules);

            vm.SelectedStudent = null;
            Assert.Empty(vm.Schedules);
        }

        [Fact]
        public void SelectedSchedule_PopulatesFormFields()
        {
            var schedule = new ClassSchedule
            {
                ScheduleId = 1, StudentId = "S001", SubjectName = "Physics",
                DayOfWeek = DayOfWeek.Wednesday, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(11)
            };

            var vm = CreateVm();
            vm.SelectedSchedule = schedule;

            Assert.Equal("Physics", vm.EditSubjectName);
            Assert.Equal(DayOfWeek.Wednesday, vm.EditDayOfWeek);
            Assert.Equal(TimeSpan.FromHours(9), vm.EditStartTime);
            Assert.Equal(TimeSpan.FromHours(11), vm.EditEndTime);
        }

        [Fact]
        public void AddCommand_NoStudentSelected_ShowsError()
        {
            var vm = CreateVm();
            vm.EditSubjectName = "Math";
            vm.EditStartTime = TimeSpan.FromHours(8);
            vm.EditEndTime = TimeSpan.FromHours(10);

            vm.AddCommand.Execute(null);

            _mockScheduleRepo.Verify(r => r.Add(It.IsAny<ClassSchedule>()), Times.Never);
            Assert.Contains("select a student", vm.StatusMessage);
        }

        [Fact]
        public void AddCommand_EmptySubjectName_ShowsError()
        {
            var vm = CreateVm(new List<Student> { _testStudent });
            vm.SelectedStudent = _testStudent;
            vm.EditSubjectName = "";
            vm.EditStartTime = TimeSpan.FromHours(8);
            vm.EditEndTime = TimeSpan.FromHours(10);

            vm.AddCommand.Execute(null);

            _mockScheduleRepo.Verify(r => r.Add(It.IsAny<ClassSchedule>()), Times.Never);
            Assert.Contains("fill in all required fields", vm.StatusMessage);
        }

        [Fact]
        public void AddCommand_StartTimeAfterEndTime_ShowsError()
        {
            var vm = CreateVm(new List<Student> { _testStudent });
            vm.SelectedStudent = _testStudent;
            vm.EditSubjectName = "Math";
            vm.EditStartTime = TimeSpan.FromHours(10);
            vm.EditEndTime = TimeSpan.FromHours(8);

            vm.AddCommand.Execute(null);

            _mockScheduleRepo.Verify(r => r.Add(It.IsAny<ClassSchedule>()), Times.Never);
            Assert.Contains("Start time must be before end time", vm.StatusMessage);
        }

        [Fact]
        public void AddCommand_WithOverlap_ShowsError()
        {
            _mockScheduleRepo.Setup(r => r.HasOverlap("S001", DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(10), null)).Returns(true);

            var vm = CreateVm(new List<Student> { _testStudent });
            vm.SelectedStudent = _testStudent;
            vm.EditSubjectName = "Math";
            vm.EditDayOfWeek = DayOfWeek.Monday;
            vm.EditStartTime = TimeSpan.FromHours(8);
            vm.EditEndTime = TimeSpan.FromHours(10);

            vm.AddCommand.Execute(null);

            _mockScheduleRepo.Verify(r => r.Add(It.IsAny<ClassSchedule>()), Times.Never);
            Assert.Contains("overlaps", vm.StatusMessage);
        }

        [Fact]
        public void AddCommand_Valid_AddsScheduleAndRefreshes()
        {
            _mockScheduleRepo.Setup(r => r.HasOverlap("S001", DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(10), null)).Returns(false);
            _mockScheduleRepo.Setup(r => r.GetByStudentId("S001")).Returns(new List<ClassSchedule>
            {
                new ClassSchedule { ScheduleId = 1, StudentId = "S001", SubjectName = "Math", DayOfWeek = DayOfWeek.Monday, StartTime = TimeSpan.FromHours(8), EndTime = TimeSpan.FromHours(10) }
            });

            var vm = CreateVm(new List<Student> { _testStudent });
            vm.SelectedStudent = _testStudent;
            vm.EditSubjectName = "Math";
            vm.EditDayOfWeek = DayOfWeek.Monday;
            vm.EditStartTime = TimeSpan.FromHours(8);
            vm.EditEndTime = TimeSpan.FromHours(10);

            vm.AddCommand.Execute(null);

            _mockScheduleRepo.Verify(r => r.Add(It.Is<ClassSchedule>(s =>
                s.StudentId == "S001" &&
                s.SubjectName == "Math" &&
                s.DayOfWeek == DayOfWeek.Monday &&
                s.StartTime == TimeSpan.FromHours(8) &&
                s.EndTime == TimeSpan.FromHours(10)
            )), Times.Once);
            Assert.Single(vm.Schedules);
            Assert.Contains("added successfully", vm.StatusMessage);
        }

        [Fact]
        public void UpdateCommand_NoScheduleSelected_ShowsError()
        {
            var vm = CreateVm();
            vm.UpdateCommand.Execute(null);

            _mockScheduleRepo.Verify(r => r.Update(It.IsAny<ClassSchedule>()), Times.Never);
            Assert.Contains("select a schedule to update", vm.StatusMessage);
        }

        [Fact]
        public void UpdateCommand_WithOverlap_ShowsError()
        {
            var schedule = new ClassSchedule
            {
                ScheduleId = 1, StudentId = "S001", SubjectName = "Math",
                DayOfWeek = DayOfWeek.Monday, StartTime = TimeSpan.FromHours(8), EndTime = TimeSpan.FromHours(10)
            };
            _mockScheduleRepo.Setup(r => r.HasOverlap("S001", DayOfWeek.Tuesday, TimeSpan.FromHours(9), TimeSpan.FromHours(11), 1)).Returns(true);

            var vm = CreateVm(new List<Student> { _testStudent });
            vm.SelectedStudent = _testStudent;
            vm.SelectedSchedule = schedule;
            vm.EditSubjectName = "Physics";
            vm.EditDayOfWeek = DayOfWeek.Tuesday;
            vm.EditStartTime = TimeSpan.FromHours(9);
            vm.EditEndTime = TimeSpan.FromHours(11);

            vm.UpdateCommand.Execute(null);

            _mockScheduleRepo.Verify(r => r.Update(It.IsAny<ClassSchedule>()), Times.Never);
            Assert.Contains("overlaps", vm.StatusMessage);
        }

        [Fact]
        public void UpdateCommand_Valid_UpdatesScheduleAndRefreshes()
        {
            var schedule = new ClassSchedule
            {
                ScheduleId = 1, StudentId = "S001", SubjectName = "Math",
                DayOfWeek = DayOfWeek.Monday, StartTime = TimeSpan.FromHours(8), EndTime = TimeSpan.FromHours(10)
            };
            _mockScheduleRepo.Setup(r => r.HasOverlap("S001", DayOfWeek.Tuesday, TimeSpan.FromHours(9), TimeSpan.FromHours(11), 1)).Returns(false);
            _mockScheduleRepo.Setup(r => r.GetByStudentId("S001")).Returns(new List<ClassSchedule> { schedule });

            var vm = CreateVm(new List<Student> { _testStudent });
            vm.SelectedStudent = _testStudent;
            vm.SelectedSchedule = schedule;
            vm.EditSubjectName = "Physics";
            vm.EditDayOfWeek = DayOfWeek.Tuesday;
            vm.EditStartTime = TimeSpan.FromHours(9);
            vm.EditEndTime = TimeSpan.FromHours(11);

            vm.UpdateCommand.Execute(null);

            _mockScheduleRepo.Verify(r => r.Update(It.Is<ClassSchedule>(s =>
                s.ScheduleId == 1 &&
                s.SubjectName == "Physics" &&
                s.DayOfWeek == DayOfWeek.Tuesday
            )), Times.Once);
            Assert.Contains("updated successfully", vm.StatusMessage);
        }

        [Fact]
        public void DeleteCommand_NoScheduleSelected_ShowsError()
        {
            var vm = CreateVm();
            vm.DeleteCommand.Execute(null);

            _mockScheduleRepo.Verify(r => r.Delete(It.IsAny<int>()), Times.Never);
            Assert.Contains("select a schedule to delete", vm.StatusMessage);
        }

        [Fact]
        public void DeleteCommand_Valid_DeletesAndRefreshes()
        {
            var schedule = new ClassSchedule
            {
                ScheduleId = 1, StudentId = "S001", SubjectName = "Math",
                DayOfWeek = DayOfWeek.Monday, StartTime = TimeSpan.FromHours(8), EndTime = TimeSpan.FromHours(10)
            };

            var vm = CreateVm(new List<Student> { _testStudent });
            vm.SelectedStudent = _testStudent;
            vm.SelectedSchedule = schedule;

            vm.DeleteCommand.Execute(null);

            _mockScheduleRepo.Verify(r => r.Delete(1), Times.Once);
            Assert.Contains("deleted successfully", vm.StatusMessage);
        }

        [Fact]
        public void ClearFormCommand_ClearsAllFields()
        {
            var schedule = new ClassSchedule
            {
                ScheduleId = 1, StudentId = "S001", SubjectName = "Math",
                DayOfWeek = DayOfWeek.Wednesday, StartTime = TimeSpan.FromHours(8), EndTime = TimeSpan.FromHours(10)
            };

            var vm = CreateVm();
            vm.SelectedSchedule = schedule;
            vm.ClearFormCommand.Execute(null);

            Assert.Equal(string.Empty, vm.EditSubjectName);
            Assert.Equal(DayOfWeek.Monday, vm.EditDayOfWeek);
            Assert.Equal(TimeSpan.Zero, vm.EditStartTime);
            Assert.Equal(TimeSpan.Zero, vm.EditEndTime);
            Assert.Null(vm.SelectedSchedule);
            Assert.Equal(string.Empty, vm.StatusMessage);
        }

        [Fact]
        public void AddCommand_StartTimeEqualsEndTime_ShowsError()
        {
            var vm = CreateVm(new List<Student> { _testStudent });
            vm.SelectedStudent = _testStudent;
            vm.EditSubjectName = "Math";
            vm.EditStartTime = TimeSpan.FromHours(8);
            vm.EditEndTime = TimeSpan.FromHours(8);

            vm.AddCommand.Execute(null);

            _mockScheduleRepo.Verify(r => r.Add(It.IsAny<ClassSchedule>()), Times.Never);
            Assert.Contains("Start time must be before end time", vm.StatusMessage);
        }
    }
}
