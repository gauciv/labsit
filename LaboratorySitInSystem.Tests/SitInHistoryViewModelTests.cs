using System;
using System.Collections.Generic;
using LaboratorySitInSystem.DataAccess;
using LaboratorySitInSystem.Models;
using LaboratorySitInSystem.ViewModels;
using Moq;
using Xunit;

namespace LaboratorySitInSystem.Tests
{
    public class SitInHistoryViewModelTests
    {
        private readonly Mock<ISessionRepository> _mockSessionRepo;

        public SitInHistoryViewModelTests()
        {
            _mockSessionRepo = new Mock<ISessionRepository>();
            _mockSessionRepo
                .Setup(r => r.GetHistory(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new List<SitInSession>());
        }

        private SitInHistoryViewModel CreateVm()
        {
            return new SitInHistoryViewModel(_mockSessionRepo.Object);
        }

        [Fact]
        public void Constructor_ThrowsOnNullRepo()
        {
            Assert.Throws<ArgumentNullException>(() => new SitInHistoryViewModel(null!));
        }

        [Fact]
        public void Constructor_LoadsAllHistory()
        {
            var sessions = new List<SitInSession>
            {
                new SitInSession { SessionId = 1, StudentName = "Alice", SubjectName = "Math", StartTime = DateTime.Now.AddHours(-2), EndTime = DateTime.Now.AddHours(-1), IsScheduled = true },
                new SitInSession { SessionId = 2, StudentName = "Bob", SubjectName = "Science", StartTime = DateTime.Now.AddHours(-3), EndTime = DateTime.Now.AddHours(-2), IsScheduled = false }
            };
            _mockSessionRepo
                .Setup(r => r.GetHistory(null, null, null, null))
                .Returns(sessions);

            var vm = CreateVm();

            Assert.Equal(2, vm.Sessions.Count);
            Assert.Equal("2 record(s) found.", vm.StatusMessage);
        }

        [Fact]
        public void SearchCommand_AppliesFilters()
        {
            var filtered = new List<SitInSession>
            {
                new SitInSession { SessionId = 1, StudentName = "Alice", SubjectName = "Math" }
            };
            var from = new DateTime(2024, 1, 1);
            var to = new DateTime(2024, 12, 31);

            _mockSessionRepo
                .Setup(r => r.GetHistory(from, to, "S001", "Math"))
                .Returns(filtered);

            var vm = CreateVm();
            vm.FromDate = from;
            vm.ToDate = to;
            vm.StudentIdFilter = "S001";
            vm.SubjectFilter = "Math";

            vm.SearchCommand.Execute(null);

            Assert.Single(vm.Sessions);
            _mockSessionRepo.Verify(r => r.GetHistory(from, to, "S001", "Math"), Times.Once);
        }

        [Fact]
        public void ClearFiltersCommand_ResetsFiltersAndReloads()
        {
            var allSessions = new List<SitInSession>
            {
                new SitInSession { SessionId = 1, StudentName = "Alice" },
                new SitInSession { SessionId = 2, StudentName = "Bob" }
            };
            _mockSessionRepo
                .Setup(r => r.GetHistory(null, null, null, null))
                .Returns(allSessions);

            var vm = CreateVm();
            vm.FromDate = DateTime.Now;
            vm.ToDate = DateTime.Now;
            vm.StudentIdFilter = "S001";
            vm.SubjectFilter = "Math";

            vm.ClearFiltersCommand.Execute(null);

            Assert.Null(vm.FromDate);
            Assert.Null(vm.ToDate);
            Assert.Null(vm.StudentIdFilter);
            Assert.Null(vm.SubjectFilter);
            Assert.Equal(2, vm.Sessions.Count);
        }

        [Fact]
        public void FilterProperties_RaisePropertyChanged()
        {
            var vm = CreateVm();
            var changed = new List<string>();
            vm.PropertyChanged += (s, e) => changed.Add(e.PropertyName);

            vm.FromDate = DateTime.Now;
            vm.ToDate = DateTime.Now;
            vm.StudentIdFilter = "test";
            vm.SubjectFilter = "test";

            Assert.Contains("FromDate", changed);
            Assert.Contains("ToDate", changed);
            Assert.Contains("StudentIdFilter", changed);
            Assert.Contains("SubjectFilter", changed);
        }

        [Fact]
        public void StatusMessage_RaisesPropertyChanged()
        {
            var vm = CreateVm();
            string lastChanged = null;
            vm.PropertyChanged += (s, e) => lastChanged = e.PropertyName;

            vm.StatusMessage = "Updated";

            Assert.Equal("StatusMessage", lastChanged);
        }

        [Fact]
        public void Constructor_EmptyHistory_ShowsZeroRecords()
        {
            var vm = CreateVm();

            Assert.Empty(vm.Sessions);
            Assert.Equal("0 record(s) found.", vm.StatusMessage);
        }
    }
}
