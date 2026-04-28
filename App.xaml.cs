using System;
using System.Configuration;
using System.Windows;
using LaboratorySitInSystem.DataAccess;
using LaboratorySitInSystem.ViewModels;

namespace LaboratorySitInSystem
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Global exception handler — prevents silent crashes
            DispatcherUnhandledException += (s, args) =>
            {
                System.Diagnostics.Debug.WriteLine($"[UNHANDLED ERROR] {args.Exception}");
                MessageBox.Show(
                    $"An error occurred:\n\n{args.Exception.Message}\n\nCheck the Output window in Visual Studio for full details.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                args.Handled = true;
            };

            string connectionString =
                ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString
                ?? "Server=localhost;Database=laboratory_sitin;Uid=root;Pwd=;";

            DatabaseHelper.Initialize(connectionString);

            var adminRepo = new AdminRepository();
            var mainViewModel = new MainViewModel();
            mainViewModel.NavigateTo(new LoginViewModel(adminRepo));

            var mainWindow = new MainWindow();
            mainWindow.DataContext = mainViewModel;
            mainWindow.Show();
        }
    }
}
