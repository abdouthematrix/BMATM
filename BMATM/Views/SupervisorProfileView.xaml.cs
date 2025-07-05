using BMATM.ViewModels;
using System.Windows.Controls;

namespace BMATM.Views
{
    /// <summary>
    /// Interaction logic for SupervisorProfileView.xaml
    /// </summary>
    public partial class SupervisorProfileView : UserControl
    {
        public SupervisorProfileView(SupervisorProfileViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Optional: Any initialization logic when the view is loaded
            if (DataContext is SupervisorProfileViewModel viewModel)
            {
                // The view model will handle data loading in its constructor
                // Additional UI-specific initialization can be added here if needed
            }
        }
    }
}