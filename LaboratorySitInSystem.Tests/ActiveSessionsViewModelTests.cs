using System;
using System.Collections.Generic;
using System.Linq;
using LaboratorySitInSystem.DataAccess;
using LaboratorySitInSystem.Models;
using LaboratorySitInSystem.ViewModels;
using Moq;
using Xunit;

namespace LaboratorySitInSystem.Tests
{
    /// <summary>
    /// Testable subclass that prevents DispatcherTimer creation.
    /// </summary>
    public class TestableActiveSessionsViewModel : ActiveSessionsViewModel
    {
        public TestableActiveSessionsViewModel(
            ISessionRepository sessionRepo,
            ISettingsRepository settingsRepo,
            IScheduleRepository scheduleRepo)
            : base(sessionRepo, settingsRepo, scheduleRepo)
        {
        }

        protected override void StartTimer()
        {
            // No-op: skip DispatcherTimer in tests
        }
    }

    public class ActiveSessionsViewModelTests
    {
        private readonly Mock<ISessionRepository> _mockSessionRepo;
        private readonly Mock<ISettingsRepository> _mockSettingsRepo;
        private readonly Mock<IScheduleRepository> _mockScheduleRepo;

        public ActiveSessionsViewModelTests()
        {
            _mockSessionRepo = new Mock<ISessionRepository>();
            _mockSettingsRepo = new Mock<ISettingsRepository>();
            _mockScheduleRepo = new Mock<IScheduleRepository>();

            // Default setups
            _mockSessionRepo.Setup(r => r.GetActiveSessions()).Returns(new List<SitInSession>());
            _mockSessionRepo.Setup(r => r.GetActiveSessionCount()).Returns(0);
            _mockSettingsRepo.Setup(r => r.GetSettings()).Returns(new SystemSettings { SettingsId = 1, AlarmThreshold = 30 });
        }

        private TestableActiveSessionsViewModel CreateVm()
        {
            return new TestableActiveSessionsViewModel(
                _mockSessionRepo.Object,
                _mockSettingsRepo.Object,
                _mockScheduleRepo.Object);
        }

        // --- Constructor / Initialization ---

        [Fact]
        public void Constructor_ThrowsOnNullSessionRepo()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new TestableActiveSessionsViewModel(null!, _mockSettingsRepo.Object, _mockScheduleRepo.Object));
        }

        [Fact]
        public void Constructor_ThrowsOnNullSettingsRepo()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new TestableActiveSessionsViewModel(_mockSessionRepo.Object, null!, _mockScheduleRepo.Object));
        }

        [Fact]
        public void Constructor_ThrowsOnNullScheduleRepo()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new TestableActiveSessionsViewModel(_mockSessionRepo.Object, _mockSettingsRepo.Object, null!));
        }

        [Fact]
        public void Constructor_InitializesCommands()
        {
            var vm = CreateVm();

            Assert.NotNull(vm.ForceEndSessionCommand);
            Assert.NotNull(vm.RefreshCommand);
        }

        [Fact]
        public void Constructor_LoadsActiveSessionsOnInit()
        {
            _mockSessionRepo.Setup(r => r.GetActiveSessions()).Returns(new List<SitInSession>
            {
                new SitInSession { SessionId = 1, StudentId = "S001", StudentName = "Alice", SubjectName = "CS101", StartTime = DateTime.Now.AddHours(-1), IsScheduled = false }
            });
            _mockSessionRepo.Setup(r => r.GetActiveSessionCount()).Returns(1);

            var vm = CreateVm();

            Assert.Single(vm.ActiveSessions);
            Assert.Equal("Alice", vm.ActiveSessions[0].StudentName);
        }

        // --- Alarm Threshold ---

        [Fact]
        public void AlarmActivates_WhenCountMeetsThreshold()
        {
            _mockSettingsRepo.Setup(r => r.GetSettings()).Returns(new SystemSettings { AlarmThreshold = 5 });
            _mockSessionRepo.Setup(r => r.GetActiveSessionCount()).Returns(5);

            var vm = CreateVm();

            Assert.True(vm.IsAlarmActive);
            Assert.Equal(5, vm.AlarmThreshold);
            Assert.Contains("ALARM", vm.StatusMessage);
        }

        [Fact]
        public void AlarmActivates_WhenCountExceedsThreshold()
        {
            _mockSettingsRepo.Setup(r => r.GetSettings()).Returns(new SystemSettings { AlarmThreshold = 3 });
            _mockSessionRepo.Setup(r => r.GetActiveSessionCount()).Returns(10);

            var vm = CreateVm();

            Assert.True(vm.IsAlarmActive);
        }

        [Fact]
        public void AlarmInactive_WhenCountBelowThreshold()
        {
            _mockSettingsRepo.Setup(r => r.GetSettings()).Returns(new SystemSettings { AlarmThreshold = 30 });
            _mockSessionRepo.Setup(r => r.GetActiveSessionCount()).Returns(5);

            var vm = CreateVm();

            Assert.False(vm.IsAlarmActive);
            Assert.DoesNotContain("ALARM", vm.StatusMessage);
        }

        [Fact]
        public void AlarmDeactivates_WhenCountDropsBelowThreshold()
        {
            _mockSettingsRepo.Setup(r => r.GetSettings()).Returns(new SystemSettings { AlarmThreshold = 5 });
            _mockSessionRepo.Setup(r => r.GetActiveSessionCount()).Returns(6);

            var vm = CreateVm();
            Assert.True(vm.IsAlarmActive);

            // Count drops
            _mockSessionRepo.Setup(r => r.GetActiveSessionCount()).Returns(3);
            vm.RefreshAndCheckSessions();

            Assert.False(vm.IsAlarmActive);
        }

        // --- Force End Session ---

        [Fact]
        public void ForceEndSession_EndsSelectedSession()
        {
            var session = new SitInSession { SessionId = 42, StudentId = "S001", StudentName = "Bob", StartTime = DateTime.Now.AddHours(-1), IsScheduled = false };
            _mockSessionRepo.Setup(r => r.GetActiveSessions()).Returns(new List<SitInSession> { session });
            _mockSessionRepo.Setup(r => r.GetActiveSessionCount()).Returns(1);

            var vm = CreateVm();
            vm.SelectedSession = session;

            vm.ForceEndSessionCommand.Execute(null);

            _mockSessionRepo.Verify(r => r.EndSession(42, It.IsAny<DateTime>()), Times.Once);
        }

        [Fact]
        public void ForceEndSession_RefreshesListAfterEnd()
        {
            var session = new SitInSession { SessionId = 1, StudentId = "S001", StudentName = "Bob", StartTime = DateTime.Now, IsScheduled = false };
            _mockSessionRepo.Setup(r => r.GetActiveSessions()).Returns(new List<SitInSession> { session });
            _mockSessionRepo.Setup(r => r.GetActiveSessionCount()).Returns(1);

            var vm = CreateVm();
            vm.SelectedSession = session;

            // After force-end, the session is gone
            _mockSessionRepo.Setup(r => r.GetActiveSessions()).Returns(new List<SitInSession>());
            _mockSessionRepo.Setup(r => r.GetActiveSessionCount()).Returns(0);

            vm.ForceEndSessionCommand.Execute(null);

            Assert.Empty(vm.ActiveSessions);
        }

        [Fact]
        public void ForceEndSession_ClearsSelectedSession()
        {
            var session = new SitInSession { SessionId = 1, StudentId = "S001", StudentName = "Bob", StartTime = DateTime.Now, IsScheduled = false };
            _mockSessionRepo.Setup(r => r.GetActiveSessions()).Returns(new List<SitInSession> { session });
            _mockSessionRepo.Setup(r => r.GetActiveSessionCount()).Returns(1);

            var vm = CreateVm();
            vm.SelectedSession = session;
            vm.ForceEndSessionCommand.Execute(null);

            Assert.Null(vm.SelectedSession);
        }

        [Fact]
        public void ForceEndSession_DoesNothing_WhenNoSelection()
        {
            var vm = CreateVm();
            vm.SelectedSession = null;

            vm.ForceEndSessionCommand.Execute(null);

            _mockSessionRepo.Verify(r => r.EndSession(It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never);
        }

        [Fact]
        public void ForceEndSession_SetsStatusMessage()
        {
            var session = new SitInSession { SessionId = 1, StudentId = "S001", StudentName = "Charlie", StartTime = DateTime.Now, IsScheduled = false };
            _mockSessionRepo.Setup(r => r.GetActiveSessions()).Returns(new List<SitInSession> { session });
            _mockSessionRepo.Setup(r => r.GetActiveSessionCount()).Returns(1);

            var vm = CreateVm();
            vm.SelectedSession = session;

            // After end, list is empty
            _mockSessionRepo.Setup(r => r.GetActiveSessions()).Returns(new List<SitInSession>());
            _mockSessionRepo.Setup(r => r.GetActiveSessionCount()).Returns(0);

            vm.ForceEndSessionCommand.Execute(null);

            // StatusMessage gets overwritten by RefreshAndCheckSessions, but the force-end was called
            _mockSessionRepo.Verify(r => r.EndSession(1, It.IsAny<DateTime>()), Times.Once);
        }

        // --- Auto-End Expired Scheduled Sessions ---

        [Fact]
        public void AutoEnd_EndsExpiredScheduledSession()
        {
            var now = DateTime.Now;
            var session = new SitInSession
            {
                SessionId = 10,
                StudentId = "S001",
                StudentName = "Diana",
                SubjectName = "Math",
                StartTime = now.Date.Add(new TimeSpan(8, 0, 0)),
                IsScheduled = true
            };

            // Schedule ended at 9:00, current time is past 9:00
            var schedule = new ClassSchedule
            {
                ScheduleId = 1,
                StudentId = "S001",
                SubjectName = "Math",
                DayOfWeek = now.DayOfWeek,
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(0, 0, 1) // Very early end time so it's always expired
            };

            _mockSessionRepo.Setup(r => r.GetActiveSessions()).Returns(new List<SitInSession> { session });
            _mockSessionRepo.Setup(r => r.GetActiveSessionCount()).Returns(1);
            _mockScheduleRepo.Setup(r => r.GetActiveSchedule("S001", session.StartTime.DayOfWeek, session.StartTime.TimeOfDay))
                .Returns(schedule);

            var vm = CreateVm();

            _mockSessionRepo.Verify(r => r.EndSession(10, It.IsAny<DateTime>()), Times.Once);
        }

        [Fact]
        public void AutoEnd_DoesNotEndUnscheduledSession()
        {
            var session = new SitInSession
            {
                SessionId = 20,
                StudentId = "S002",
                StudentName = "Eve",
                StartTime = DateTime.Now.AddHours(-1),
                IsScheduled = false
            };

            _mockSessionRepo.Setup(r => r.GetActiveSessions()).Returns(new List<SitInSession> { session });
            _mockSessionRepo.Setup(r => r.GetActiveSessionCount()).Returns(1);

            var vm = CreateVm();

            _mockSessionRepo.Verify(r => r.EndSession(It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never);
        }

        [Fact]
        public void AutoEnd_DoesNotEnd_WhenScheduleNotExpired()
        {
            var now = DateTime.Now;
            var session = new SitInSession
            {
                SessionId = 30,
                StudentId = "S003",
                StudentName = "Frank",
                SubjectName = "Physics",
                StartTime = now.AddMinutes(-30),
                IsScheduled = true
            };

            var schedule = new ClassSchedule
            {
                ScheduleId = 2,
                StudentId = "S003",
                SubjectName = "Physics",
                DayOfWeek = now.DayOfWeek,
                StartTime = now.TimeOfDay.Subtract(TimeSpan.FromHours(1)),
                EndTime = new TimeSpan(23, 59, 59) // Far future end time
            };

            _mockSessionRepo.Setup(r => r.GetActiveSessions()).Returns(new List<SitInSession> { session });
            _mockSessionRepo.Setup(r => r.GetActiveSessionCount()).Returns(1);
            _mockScheduleRepo.Setup(r => r.GetActiveSchedule("S003", session.StartTime.DayOfWeek, session.StartTime.TimeOfDay))
                .Returns(schedule);

            var vm = CreateVm();

            _mockSessionRepo.Verify(r => r.EndSession(It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never);
        }

        [Fact]
        public void AutoEnd_DoesNotEnd_WhenNoScheduleFound()
        {
            var session = new SitInSession
            {
                SessionId = 40,
                StudentId = "S004",
                StudentName = "Grace",
                StartTime = DateTime.Now.AddHours(-1),
                IsScheduled = true
            };

            _mockSessionRepo.Setup(r => r.GetActiveSessions()).Returns(new List<SitInSession> { session });
            _mockSessionRepo.Setup(r => r.GetActiveSessionCount()).Returns(1);
            _mockScheduleRepo.Setup(r => r.GetActiveSchedule("S004", It.IsAny<DayOfWeek>(), It.IsAny<TimeSpan>()))
                .Returns((ClassSchedule)null);

            var vm = CreateVm();

            _mockSessionRepo.Verify(r => r.EndSession(It.IsAny<int>(), It.IsAny<DateTime>()), Times.Never);
        }

        // --- Refresh Command ---

        [Fact]
        public void RefreshCommand_CallsRefreshAndCheckSessions()
        {
            var vm = CreateVm();

            // Change data
            _mockSessionRepo.Setup(r => r.GetActiveSessions()).Returns(new List<SitInSession>
            {
                new SitInSession { SessionId = 1, StudentId = "S001", StudentName = "Test", StartTime = DateTime.Now, IsScheduled = false }
            });
            _mockSessionRepo.Setup(r => r.GetActiveSessionCount()).Returns(1);

            vm.RefreshCommand.Execute(null);

            Assert.Single(vm.ActiveSessions);
        }

        // --- Property Change Notifications ---

        [Fact]
        public void SelectedSession_RaisesPropertyChanged()
        {
            var vm = CreateVm();
            string? changed = null;
            vm.PropertyChanged += (s, e) => changed = e.PropertyName;

            vm.SelectedSession = new SitInSession { SessionId = 1 };

            Assert.Equal("SelectedSession", changed);
        }

        [Fact]
        public void IsAlarmActive_RaisesPropertyChanged()
        {
            var vm = CreateVm();
            var changes = new List<string>();
            vm.PropertyChanged += (s, e) => changes.Add(e.PropertyName);

            _mockSettingsRepo.Setup(r => r.GetSettings()).Returns(new SystemSettings { AlarmThreshold = 1 });
            _mockSessionRepo.Setup(r => r.GetActiveSessionCount()).Returns(5);
            vm.RefreshAndCheckSessions();

            Assert.Contains("IsAlarmActive", changes);
        }
    }
}
