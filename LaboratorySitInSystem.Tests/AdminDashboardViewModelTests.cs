using System;
using System.Collections.Generic;
using LaboratorySitInSystem.DataAccess;
using LaboratorySitInSystem.Models;
using LaboratorySitInSystem.ViewModels;
using Moq;
using Xunit;

namespace LaboratorySitInSystem.Tests
{
    public class AdminDashboardViewModelTests
    {
        private readonly Mock<IStudentRepository> _mockStudentRepo;
        private readonly Mock<ISessionRepository> _mockSessionRepo;
        private readonly Mock<IScheduleRepository> _mockScheduleRepo;
        private readonly Mock<ISettingsRepository> _mockSettingsRepo;
        private readonly Mock<IAdminRepository> _mockAdminRepo;

        public AdminDashboardViewModelTests()
        {
            _mockStudentRepo = new Mock<IStudentRepository>();
            _mockSessionRepo = new Mock<ISessionRepository>();
            _mockScheduleRepo = new Mock<IScheduleRepository>();
            _mockSettingsRepo = new Mock<ISettingsRepository>();
            _mockAdminRepo = new Mock<IAdminRepository>();

            _mockStudentRepo.Setup(r => r.GetAll()).Returns(new List<Student>());
            _mockSessionRepo.Setup(r => r.GetActiveSessionCount()).Returns(0);
            _mockSessionRepo.Setup(r => r.GetActiveSessions()).Returns(new List<SitInSession>());
            _mockSessionRepo.Setup(r => r.GetHistory(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new List<SitInSession>());
            _mockSettingsRepo.Setup(r => r.GetSettings()).Returns(new SystemSettings { SettingsId = 1, AlarmThreshold = 30 });
        }

        private AdminDashboardViewModel CreateVm()
        {
            return new AdminDashboardViewModel(
                _mockStudentRepo.Object,
                _mockSessionRepo.Object,
                _mockScheduleRepo.Object,
                _mockSettingsRepo.Object,
                _mockAdminRepo.Object);
        }

        [Fact]
        public void Constructor_InitializesAllCommands()
        {
            var vm = CreateVm();

            Assert.NotNull(vm.GoToStudentManagementCommand);
            Assert.NotNull(vm.GoToScheduleManagementCommand);
            Assert.NotNull(vm.GoToActiveSessionsCommand);
            Assert.NotNull(vm.GoToSitInHistoryCommand);
            Assert.NotNull(vm.GoToSettingsCommand);
            Assert.NotNull(vm.GoToAboutCommand);
            Assert.NotNull(vm.LogoutCommand);
        }

        [Fact]
        public void Constructor_CallsRefreshDashboard_LoadsCounts()
        {
            _mockStudentRepo.Setup(r => r.GetAll()).Returns(new List<Student>
            {
                new Student { StudentId = "S001", FirstName = "A", LastName = "B", Course = "CS", YearLevel = 1 },
                new Student { StudentId = "S002", FirstName = "C", LastName = "D", Course = "IT", YearLevel = 2 },
                new Student { StudentId = "S003", FirstName = "E", LastName = "F", Course = "CS", YearLevel = 3 }
            });
            _mockSessionRepo.Setup(r => r.GetActiveSessionCount()).Returns(5);

            var vm = CreateVm();

            Assert.Equal(3, vm.TotalStudents);
            Assert.Equal(5, vm.ActiveSessionCount);
        }

        [Fact]
        public void Constructor_NoStudentsNoSessions_ZeroCounts()
        {
            var vm = CreateVm();

            Assert.Equal(0, vm.TotalStudents);
            Assert.Equal(0, vm.ActiveSessionCount);
        }

        [Fact]
        public void RefreshDashboard_UpdatesCounts()
        {
            var vm = CreateVm();
            Assert.Equal(0, vm.TotalStudents);
            Assert.Equal(0, vm.ActiveSessionCount);

            _mockStudentRepo.Setup(r => r.GetAll()).Returns(new List<Student>
            {
                new Student { StudentId = "S001", FirstName = "A", LastName = "B", Course = "CS", YearLevel = 1 }
            });
            _mockSessionRepo.Setup(r => r.GetActiveSessionCount()).Returns(2);

            vm.RefreshDashboard();

            Assert.Equal(1, vm.TotalStudents);
            Assert.Equal(2, vm.ActiveSessionCount);
        }

        [Fact]
        public void TotalStudents_RaisesPropertyChanged()
        {
            var vm = CreateVm();
            string? changed = null;
            vm.PropertyChanged += (s, e) => changed = e.PropertyName;

            _mockStudentRepo.Setup(r => r.GetAll()).Returns(new List<Student>
            {
                new Student { StudentId = "S001", FirstName = "A", LastName = "B", Course = "CS", YearLevel = 1 }
            });
            vm.RefreshDashboard();

            Assert.Equal("TotalStudents", changed);
        }

        [Fact]
        public void ActiveSessionCount_RaisesPropertyChanged()
        {
            var vm = CreateVm();
            string? changed = null;
            vm.PropertyChanged += (s, e) => changed = e.PropertyName;

            _mockSessionRepo.Setup(r => r.GetActiveSessionCount()).Returns(7);
            vm.RefreshDashboard();

            Assert.Equal("ActiveSessionCount", changed);
        }

        [Fact]
        public void CurrentSubView_SetAndGet_RaisesPropertyChanged()
        {
            var vm = CreateVm();
            string? changed = null;
            vm.PropertyChanged += (s, e) => changed = e.PropertyName;

            vm.CurrentSubView = "test";

            Assert.Equal("test", vm.CurrentSubView);
            Assert.Equal("CurrentSubView", changed);
        }

        [Fact]
        public void LogoutCommand_NavigatesToLoginViewModel()
        {
            // Ensure MainViewModel.Instance is set
            var mainVm = new MainViewModel();
            var vm = CreateVm();

            vm.LogoutCommand.Execute(null);

            Assert.IsType<LoginViewModel>(MainViewModel.Instance.CurrentView);
        }

        [Fact]
        public void NavigationCommands_CanExecute()
        {
            var vm = CreateVm();

            Assert.True(vm.GoToStudentManagementCommand.CanExecute(null));
            Assert.True(vm.GoToScheduleManagementCommand.CanExecute(null));
            Assert.True(vm.GoToActiveSessionsCommand.CanExecute(null));
            Assert.True(vm.GoToSitInHistoryCommand.CanExecute(null));
            Assert.True(vm.GoToSettingsCommand.CanExecute(null));
            Assert.True(vm.GoToAboutCommand.CanExecute(null));
            Assert.True(vm.LogoutCommand.CanExecute(null));
        }

        [Fact]
        public void NavigationCommands_DoNotThrow()
        {
            var vm = CreateVm();

            var ex1 = Record.Exception(() => vm.GoToStudentManagementCommand.Execute(null));
            var ex2 = Record.Exception(() => vm.GoToScheduleManagementCommand.Execute(null));
            var ex3 = Record.Exception(() => vm.GoToActiveSessionsCommand.Execute(null));
            var ex4 = Record.Exception(() => vm.GoToSitInHistoryCommand.Execute(null));
            var ex5 = Record.Exception(() => vm.GoToSettingsCommand.Execute(null));
            var ex6 = Record.Exception(() => vm.GoToAboutCommand.Execute(null));

            Assert.Null(ex1);
            Assert.Null(ex2);
            Assert.Null(ex3);
            Assert.Null(ex4);
            Assert.Null(ex5);
            Assert.Null(ex6);
        }

        [Fact]
        public void Constructor_ThrowsOnNullStudentRepo()
        {
            Assert.Throws<System.ArgumentNullException>(() =>
                new AdminDashboardViewModel(null!, _mockSessionRepo.Object, _mockScheduleRepo.Object, _mockSettingsRepo.Object, _mockAdminRepo.Object));
        }

        [Fact]
        public void Constructor_ThrowsOnNullSessionRepo()
        {
            Assert.Throws<System.ArgumentNullException>(() =>
                new AdminDashboardViewModel(_mockStudentRepo.Object, null!, _mockScheduleRepo.Object, _mockSettingsRepo.Object, _mockAdminRepo.Object));
        }

        [Fact]
        public void Constructor_ThrowsOnNullScheduleRepo()
        {
            Assert.Throws<System.ArgumentNullException>(() =>
                new AdminDashboardViewModel(_mockStudentRepo.Object, _mockSessionRepo.Object, null!, _mockSettingsRepo.Object, _mockAdminRepo.Object));
        }

        [Fact]
        public void Constructor_ThrowsOnNullSettingsRepo()
        {
            Assert.Throws<System.ArgumentNullException>(() =>
                new AdminDashboardViewModel(_mockStudentRepo.Object, _mockSessionRepo.Object, _mockScheduleRepo.Object, null!, _mockAdminRepo.Object));
        }

        [Fact]
        public void Constructor_ThrowsOnNullAdminRepo()
        {
            Assert.Throws<System.ArgumentNullException>(() =>
                new AdminDashboardViewModel(_mockStudentRepo.Object, _mockSessionRepo.Object, _mockScheduleRepo.Object, _mockSettingsRepo.Object, null!));
        }
    }
}
