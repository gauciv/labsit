using System;
using System.Diagnostics;
using LaboratorySitInSystem.DataAccess;
using LaboratorySitInSystem.Helpers;

namespace LaboratorySitInSystem.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IAdminRepository _adminRepo;

        private string _username;
        private string _password;
        private string _errorMessage;
        private string _newPassword;
        private string _confirmPassword;
        private bool _isResetMode;
        private string _resetMessage;

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string NewPassword
        {
            get => _newPassword;
            set => SetProperty(ref _newPassword, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        public bool IsResetMode
        {
            get => _isResetMode;
            set => SetProperty(ref _isResetMode, value);
        }

        public string ResetMessage
        {
            get => _resetMessage;
            set => SetProperty(ref _resetMessage, value);
        }

        public RelayCommand LoginCommand { get; }
        public RelayCommand GoToStudentSitInCommand { get; }
        public RelayCommand ResetPasswordCommand { get; }
        public RelayCommand ToggleResetModeCommand { get; }

        public LoginViewModel(IAdminRepository adminRepo)
        {
            _adminRepo = adminRepo ?? throw new ArgumentNullException(nameof(adminRepo));

            LoginCommand = new RelayCommand(ExecuteLogin);
            GoToStudentSitInCommand = new RelayCommand(ExecuteGoToStudentSitIn);
            ResetPasswordCommand = new RelayCommand(ExecuteResetPassword);
            ToggleResetModeCommand = new RelayCommand(ExecuteToggleResetMode);
        }

        private void ExecuteLogin(object parameter)
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter both username and password.";
                return;
            }

            try
            {
                var admin = _adminRepo.Authenticate(Username, Password);
                if (admin != null)
                {
                    var studentRepo = new StudentRepository();
                    var sessionRepo = new SessionRepository();
                    var scheduleRepo = new ScheduleRepository();
                    var settingsRepo = new SettingsRepository();

                    MainViewModel.Instance.NavigateTo(new AdminDashboardViewModel(
                        studentRepo, sessionRepo, scheduleRepo, settingsRepo, _adminRepo));
                }
                else
                {
                    ErrorMessage = "Invalid username or password.";
                }
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                ErrorMessage = $"Database connection failed. Is XAMPP MySQL running?\n\nDetails: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[LOGIN ERROR] MySqlException: {ex}");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[LOGIN ERROR] Exception: {ex}");
            }
        }

        private void ExecuteGoToStudentSitIn(object parameter)
        {
            try
            {
                var studentRepo = new StudentRepository();
                var scheduleRepo = new ScheduleRepository();
                var sessionRepo = new SessionRepository();

                MainViewModel.Instance.NavigateTo(new StudentSitInViewModel(
                    studentRepo, scheduleRepo, sessionRepo));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to navigate: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[NAV ERROR] Exception: {ex}");
            }
        }

        private void ExecuteResetPassword(object parameter)
        {
            ResetMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Username))
            {
                ResetMessage = "Username is required";
                return;
            }

            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                ResetMessage = "New password is required";
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                ResetMessage = "Passwords do not match";
                return;
            }

            try
            {
                _adminRepo.UpdatePassword(Username, NewPassword);
                ResetMessage = "Password reset successful";
                NewPassword = string.Empty;
                ConfirmPassword = string.Empty;
                IsResetMode = false;
            }
            catch (Exception ex)
            {
                ResetMessage = $"Password reset failed: {ex.Message}";
            }
        }

        private void ExecuteToggleResetMode(object parameter)
        {
            IsResetMode = !IsResetMode;
            ResetMessage = string.Empty;
            ErrorMessage = string.Empty;
        }
    }
}
