using System;
using System.Windows;
using BMATM.ViewModels;
using BMATM.Services.Navigation;

namespace BMATM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Gets the MainWindowViewModel instance
        /// </summary>
        public MainWindowViewModel ViewModel { get; private set; }

        /// <summary>
        /// Initializes a new instance of MainWindow
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Initialize the navigation service
            var navigationService = new NavigationService();

            // Create and set the DataContext
            ViewModel = new MainWindowViewModel(navigationService);
            DataContext = ViewModel;

            // Handle window closing event
            Closing += MainWindow_Closing;
        }

        /// <summary>
        /// Alternative constructor that accepts a ViewModel (for dependency injection)
        /// </summary>
        /// <param name="viewModel">The MainWindowViewModel instance</param>
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();

            ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            DataContext = ViewModel;

            // Handle window closing event
            Closing += MainWindow_Closing;
        }

        /// <summary>
        /// Handles the logout button click event
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The event arguments</param>
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Show confirmation dialog
            var result = MessageBox.Show(
                "Are you sure you want to logout?",
                "Confirm Logout",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ViewModel.Logout();
            }
        }

        /// <summary>
        /// Handles the main window closing event
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The event arguments</param>
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Check if there's an active operation that shouldn't be interrupted
            if (ViewModel.CurrentSupervisor != null)
            {
                var result = MessageBox.Show(
                    "Are you sure you want to exit the application?",
                    "Confirm Exit",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            // Perform cleanup
            ViewModel.OnWindowClosing();
        }

        /// <summary>
        /// Shows an error message to the user
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="title">The dialog title</param>
        public void ShowError(string message, string title = "Error")
        {
            MessageBox.Show(this, message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Shows an information message to the user
        /// </summary>
        /// <param name="message">The information message</param>
        /// <param name="title">The dialog title</param>
        public void ShowInformation(string message, string title = "Information")
        {
            MessageBox.Show(this, message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Shows a confirmation dialog to the user
        /// </summary>
        /// <param name="message">The confirmation message</param>
        /// <param name="title">The dialog title</param>
        /// <returns>True if the user confirmed, false otherwise</returns>
        public bool ShowConfirmation(string message, string title = "Confirm")
        {
            var result = MessageBox.Show(this, message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }
    }
}