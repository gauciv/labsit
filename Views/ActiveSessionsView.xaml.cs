using System.Windows.Controls;
using System.Windows.Input;
using LaboratorySitInSystem.ViewModels;

namespace LaboratorySitInSystem.Views
{
    public partial class ActiveSessionsView : UserControl
    {
        public ActiveSessionsView()
        {
            InitializeComponent();
        }

        private void Overlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Close popup when clicking on overlay
            if (DataContext is ActiveSessionsViewModel viewModel)
            {
                viewModel.IsApprovalPopupOpen = false;
            }
        }

        private void Dialog_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Prevent click from bubbling to overlay
            e.Handled = true;
        }
    }
}
