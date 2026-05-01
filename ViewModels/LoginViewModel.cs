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
        private bool _isPasswordVisible;
        private string _passwordVisibilityText = "Show";
        private bool _rememberMe;

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

        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set => SetProperty(ref _isPasswordVisible, value);
        }

        public string PasswordVisibilityText
        {
            get => _passwordVisibilityText;
            set => SetProperty(ref _passwordVisibilityText, value);
        }

        public bool RememberMe
        {
            get => _rememberMe;
            set => SetProperty(ref _rememberMe, value);
        }

        public RelayCommand LoginCommand { get; }
        public RelayCommand GoToStudentSitInCommand { get; }
        public RelayCommand ResetPasswordCommand { get; }
        public RelayCommand ToggleResetModeCommand { get; }
        public RelayCommand TogglePasswordVisibilityCommand { get; }

        public LoginViewModel(IAdminRepository adminRepo)
        {
            _adminRepo = adminRepo ?? throw new ArgumentNullException(nameof(adminRepo));

            LoginCommand = new RelayCommand(ExecuteLogin);
            GoToStudentSitInCommand = new RelayCommand(ExecuteGoToStudentSitIn);
            ResetPasswordCommand = new RelayCommand(ExecuteResetPassword);
            ToggleResetModeCommand = new RelayCommand(ExecuteToggleResetMode);
            TogglePasswordVisibilityCommand = new RelayCommand(ExecuteTogglePasswordVisibility);

            // Load saved credentials if they exist
            LoadSavedCredentials();
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
                    // Handle Remember Me functionality
                    if (RememberMe)
                    {
                        SaveCredentials(Username, Password);
                    }
                    else
                    {
                        ClearSavedCredentials();
                    }

                    var studentRepo = new StudentRepository();
                    var sessionRepo = new SessionRepository();
                    var scheduleRepo = new ScheduleRepository();
                    var settingsRepo = new SettingsRepository();

                    // Navigate to loading screen first
                    var loadingViewModel = new LoadingViewModel(() =>
                    {
                        // After 3 seconds, navigate to admin dashboard
                        MainViewModel.Instance.NavigateTo(new AdminDashboardViewModel(
                            studentRepo, sessionRepo, scheduleRepo, settingsRepo, _adminRepo));
                    });

                    MainViewModel.Instance.NavigateTo(loadingViewModel);
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

        private void ExecuteTogglePasswordVisibility(object parameter)
        {
            IsPasswordVisible = !IsPasswordVisible;
            PasswordVisibilityText = IsPasswordVisible ? "Hide" : "Show";
        }

        private void LoadSavedCredentials()
        {
            try
            {
                var savedUsername = Properties.Settings.Default.SavedUsername;
                var savedPassword = Properties.Settings.Default.SavedPassword;
                var rememberMe = Properties.Settings.Default.RememberMe;

                if (rememberMe && !string.IsNullOrEmpty(savedUsername) && !string.IsNullOrEmpty(savedPassword))
                {
                    Username = savedUsername;
                    Password = savedPassword;
                    RememberMe = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LOAD CREDENTIALS ERROR] {ex.Message}");
            }
        }

        private void SaveCredentials(string username, string password)
        {
            try
            {
                Properties.Settings.Default.SavedUsername = username;
                Properties.Settings.Default.SavedPassword = password;
                Properties.Settings.Default.RememberMe = true;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SAVE CREDENTIALS ERROR] {ex.Message}");
            }
        }

        private void ClearSavedCredentials()
        {
            try
            {
                Properties.Settings.Default.SavedUsername = string.Empty;
                Properties.Settings.Default.SavedPassword = string.Empty;
                Properties.Settings.Default.RememberMe = false;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CLEAR CREDENTIALS ERROR] {ex.Message}");
            }
        }
    }
}
