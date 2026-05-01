using System.Windows;
using System.Windows.Controls;
using LaboratorySitInSystem.ViewModels;

namespace LaboratorySitInSystem.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
            Loaded += LoginView_Loaded;
        }

        private void LoginView_Loaded(object sender, RoutedEventArgs e)
        {
            // Sync PasswordBox with saved password if RememberMe is enabled
            if (DataContext is LoginViewModel viewModel && !string.IsNullOrEmpty(viewModel.Password))
            {
                PasswordBox.Password = viewModel.Password;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel && sender is PasswordBox passwordBox)
            {
                viewModel.Password = passwordBox.Password;
            }
        }
    }
}
