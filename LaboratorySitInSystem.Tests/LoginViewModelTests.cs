using LaboratorySitInSystem.DataAccess;
using LaboratorySitInSystem.Models;
using LaboratorySitInSystem.ViewModels;
using Moq;
using Xunit;

namespace LaboratorySitInSystem.Tests
{
    public class LoginViewModelTests
    {
        private readonly Mock<IAdminRepository> _mockAdminRepo;
        private readonly LoginViewModel _vm;

        public LoginViewModelTests()
        {
            _mockAdminRepo = new Mock<IAdminRepository>();
            _vm = new LoginViewModel(_mockAdminRepo.Object);
        }

        [Fact]
        public void Constructor_InitializesCommands()
        {
            Assert.NotNull(_vm.LoginCommand);
            Assert.NotNull(_vm.GoToStudentSitInCommand);
        }

        [Fact]
        public void Username_SetAndGet_RaisesPropertyChanged()
        {
            string? changed = null;
            _vm.PropertyChanged += (s, e) => changed = e.PropertyName;

            _vm.Username = "admin";

            Assert.Equal("admin", _vm.Username);
            Assert.Equal("Username", changed);
        }

        [Fact]
        public void Password_SetAndGet_RaisesPropertyChanged()
        {
            string? changed = null;
            _vm.PropertyChanged += (s, e) => changed = e.PropertyName;

            _vm.Password = "secret";

            Assert.Equal("secret", _vm.Password);
            Assert.Equal("Password", changed);
        }

        [Fact]
        public void ErrorMessage_SetAndGet_RaisesPropertyChanged()
        {
            string? changed = null;
            _vm.PropertyChanged += (s, e) => changed = e.PropertyName;

            _vm.ErrorMessage = "error";

            Assert.Equal("error", _vm.ErrorMessage);
            Assert.Equal("ErrorMessage", changed);
        }

        [Fact]
        public void LoginCommand_InvalidCredentials_SetsErrorMessage()
        {
            _mockAdminRepo
                .Setup(r => r.Authenticate(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((AdminUser?)null);

            _vm.Username = "wrong";
            _vm.Password = "wrong";
            _vm.LoginCommand.Execute(null);

            Assert.Equal("Invalid username or password", _vm.ErrorMessage);
        }

        [Fact]
        public void LoginCommand_ValidCredentials_ClearsErrorMessage()
        {
            _mockAdminRepo
                .Setup(r => r.Authenticate("admin", "admin123"))
                .Returns(new AdminUser { AdminId = 1, Username = "admin", PasswordHash = "hash" });

            _vm.ErrorMessage = "previous error";
            _vm.Username = "admin";
            _vm.Password = "admin123";
            _vm.LoginCommand.Execute(null);

            Assert.NotEqual("Invalid username or password", _vm.ErrorMessage);
        }

        [Fact]
        public void LoginCommand_CallsAuthenticateWithCorrectCredentials()
        {
            _mockAdminRepo
                .Setup(r => r.Authenticate(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((AdminUser?)null);

            _vm.Username = "testuser";
            _vm.Password = "testpass";
            _vm.LoginCommand.Execute(null);

            _mockAdminRepo.Verify(r => r.Authenticate("testuser", "testpass"), Times.Once);
        }

        [Fact]
        public void GoToStudentSitInCommand_CanExecute()
        {
            Assert.True(_vm.GoToStudentSitInCommand.CanExecute(null));
        }

        [Fact]
        public void GoToStudentSitInCommand_DoesNotThrow()
        {
            var ex = Record.Exception(() => _vm.GoToStudentSitInCommand.Execute(null));
            Assert.Null(ex);
        }

        [Fact]
        public void Constructor_ThrowsOnNullRepository()
        {
            Assert.Throws<System.ArgumentNullException>(() => new LoginViewModel(null!));
        }

        [Fact]
        public void Constructor_InitializesResetCommands()
        {
            Assert.NotNull(_vm.ResetPasswordCommand);
            Assert.NotNull(_vm.ToggleResetModeCommand);
        }

        [Fact]
        public void NewPassword_SetAndGet_RaisesPropertyChanged()
        {
            string? changed = null;
            _vm.PropertyChanged += (s, e) => changed = e.PropertyName;

            _vm.NewPassword = "newpass";

            Assert.Equal("newpass", _vm.NewPassword);
            Assert.Equal("NewPassword", changed);
        }

        [Fact]
        public void ConfirmPassword_SetAndGet_RaisesPropertyChanged()
        {
            string? changed = null;
            _vm.PropertyChanged += (s, e) => changed = e.PropertyName;

            _vm.ConfirmPassword = "newpass";

            Assert.Equal("newpass", _vm.ConfirmPassword);
            Assert.Equal("ConfirmPassword", changed);
        }

        [Fact]
        public void IsResetMode_SetAndGet_RaisesPropertyChanged()
        {
            string? changed = null;
            _vm.PropertyChanged += (s, e) => changed = e.PropertyName;

            _vm.IsResetMode = true;

            Assert.True(_vm.IsResetMode);
            Assert.Equal("IsResetMode", changed);
        }

        [Fact]
        public void ResetMessage_SetAndGet_RaisesPropertyChanged()
        {
            string? changed = null;
            _vm.PropertyChanged += (s, e) => changed = e.PropertyName;

            _vm.ResetMessage = "success";

            Assert.Equal("success", _vm.ResetMessage);
            Assert.Equal("ResetMessage", changed);
        }

        [Fact]
        public void ToggleResetModeCommand_TogglesIsResetMode()
        {
            Assert.False(_vm.IsResetMode);

            _vm.ToggleResetModeCommand.Execute(null);
            Assert.True(_vm.IsResetMode);

            _vm.ToggleResetModeCommand.Execute(null);
            Assert.False(_vm.IsResetMode);
        }

        [Fact]
        public void ToggleResetModeCommand_ClearsMessages()
        {
            _vm.ErrorMessage = "some error";
            _vm.ResetMessage = "some message";

            _vm.ToggleResetModeCommand.Execute(null);

            Assert.Equal(string.Empty, _vm.ResetMessage);
            Assert.Equal(string.Empty, _vm.ErrorMessage);
        }

        [Fact]
        public void ResetPasswordCommand_EmptyUsername_SetsResetMessage()
        {
            _vm.Username = "";
            _vm.NewPassword = "newpass";
            _vm.ConfirmPassword = "newpass";

            _vm.ResetPasswordCommand.Execute(null);

            Assert.Equal("Username is required", _vm.ResetMessage);
            _mockAdminRepo.Verify(r => r.UpdatePassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void ResetPasswordCommand_EmptyNewPassword_SetsResetMessage()
        {
            _vm.Username = "admin";
            _vm.NewPassword = "";
            _vm.ConfirmPassword = "";

            _vm.ResetPasswordCommand.Execute(null);

            Assert.Equal("New password is required", _vm.ResetMessage);
            _mockAdminRepo.Verify(r => r.UpdatePassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void ResetPasswordCommand_PasswordsMismatch_SetsResetMessage()
        {
            _vm.Username = "admin";
            _vm.NewPassword = "newpass";
            _vm.ConfirmPassword = "different";

            _vm.ResetPasswordCommand.Execute(null);

            Assert.Equal("Passwords do not match", _vm.ResetMessage);
            _mockAdminRepo.Verify(r => r.UpdatePassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void ResetPasswordCommand_ValidInput_CallsUpdatePasswordAndSetsSuccessMessage()
        {
            _vm.Username = "admin";
            _vm.NewPassword = "newpass123";
            _vm.ConfirmPassword = "newpass123";
            _vm.IsResetMode = true;

            _vm.ResetPasswordCommand.Execute(null);

            _mockAdminRepo.Verify(r => r.UpdatePassword("admin", "newpass123"), Times.Once);
            Assert.Equal("Password reset successful", _vm.ResetMessage);
            Assert.False(_vm.IsResetMode);
            Assert.Equal(string.Empty, _vm.NewPassword);
            Assert.Equal(string.Empty, _vm.ConfirmPassword);
        }

        [Fact]
        public void ResetPasswordCommand_RepositoryThrows_SetsErrorResetMessage()
        {
            _mockAdminRepo
                .Setup(r => r.UpdatePassword(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new System.Exception("DB error"));

            _vm.Username = "admin";
            _vm.NewPassword = "newpass";
            _vm.ConfirmPassword = "newpass";

            _vm.ResetPasswordCommand.Execute(null);

            Assert.Contains("Password reset failed", _vm.ResetMessage);
            Assert.Contains("DB error", _vm.ResetMessage);
        }
    }
}
