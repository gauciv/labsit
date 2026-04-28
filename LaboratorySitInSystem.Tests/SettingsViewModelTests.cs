using System;
using System.Collections.Generic;
using LaboratorySitInSystem.DataAccess;
using LaboratorySitInSystem.Models;
using LaboratorySitInSystem.ViewModels;
using Moq;
using Xunit;

namespace LaboratorySitInSystem.Tests
{
    public class SettingsViewModelTests
    {
        private readonly Mock<ISettingsRepository> _mockSettingsRepo;

        public SettingsViewModelTests()
        {
            _mockSettingsRepo = new Mock<ISettingsRepository>();
            _mockSettingsRepo
                .Setup(r => r.GetSettings())
                .Returns(new SystemSettings { SettingsId = 1, AlarmThreshold = 30 });
        }

        private SettingsViewModel CreateVm()
        {
            return new SettingsViewModel(_mockSettingsRepo.Object);
        }

        [Fact]
        public void Constructor_ThrowsOnNullRepo()
        {
            Assert.Throws<ArgumentNullException>(() => new SettingsViewModel(null!));
        }

        [Fact]
        public void Constructor_LoadsCurrentThreshold()
        {
            var vm = CreateVm();

            Assert.Equal(30, vm.AlarmThreshold);
        }

        [Fact]
        public void SaveCommand_UpdatesThresholdAndShowsMessage()
        {
            var vm = CreateVm();
            vm.AlarmThreshold = 50;

            vm.SaveCommand.Execute(null);

            _mockSettingsRepo.Verify(r => r.UpdateAlarmThreshold(50), Times.Once);
            Assert.Equal("Settings saved successfully.", vm.StatusMessage);
        }

        [Fact]
        public void AlarmThreshold_RaisesPropertyChanged()
        {
            var vm = CreateVm();
            string lastChanged = null;
            vm.PropertyChanged += (s, e) => lastChanged = e.PropertyName;

            vm.AlarmThreshold = 42;

            Assert.Equal("AlarmThreshold", lastChanged);
        }

        [Fact]
        public void StatusMessage_RaisesPropertyChanged()
        {
            var vm = CreateVm();
            string lastChanged = null;
            vm.PropertyChanged += (s, e) => lastChanged = e.PropertyName;

            vm.StatusMessage = "Test";

            Assert.Equal("StatusMessage", lastChanged);
        }

        [Fact]
        public void SaveCommand_CanExecute()
        {
            var vm = CreateVm();

            Assert.True(vm.SaveCommand.CanExecute(null));
        }
    }
}
