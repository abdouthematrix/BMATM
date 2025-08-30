using System;
using System.IO;
using System.Windows;
using BMATM.Data;
using BMATM.Services.Navigation;
using BMATM.ViewModels;

namespace BMATM
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Gets the database connection factory instance
        /// </summary>
        public static SQLiteConnectionFactory DatabaseFactory { get; private set; }

        /// <summary>
        /// Gets the navigation service instance
        /// </summary>
        public static NavigationService NavigationService { get; private set; }

        /// <summary>
        /// Application startup event handler
        /// </summary>
        /// <param name="e">Startup event arguments</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Initialize application services
                InitializeServices();

                // Initialize database (skip for Phase 2 testing)
                // InitializeDatabase();

                // Create and show main window
                var mainWindow = CreateMainWindow();
                mainWindow.Show();

                // For Phase 2 testing, start with NavigationTestViewModel
                InitializePhase2Testing();
            }
            catch (Exception ex)
            {
                ShowFatalError($"Failed to start application: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Application exit event handler
        /// </summary>
        /// <param name="e">Exit event arguments</param>
        protected override void OnExit(ExitEventArgs e)
        {
            // Perform cleanup
            CleanupServices();

            base.OnExit(e);
        }

        /// <summary>
        /// Global exception handler for unhandled exceptions
        /// </summary>
        /// <param name="e">Exception event arguments</param>
        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            // Handle Windows shutdown/logoff
            CleanupServices();
            base.OnSessionEnding(e);
        }

        /// <summary>
        /// Initializes application services
        /// </summary>
        private void InitializeServices()
        {
            // Initialize navigation service
            NavigationService = new NavigationService();

            // Set up global exception handling
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        /// <summary>
        /// Initializes the database connection and schema
        /// Note: Commented out for Phase 2 testing, will be enabled in Phase 1 integration
        /// </summary>
        private void InitializeDatabase()
        {
            try
            {
                // Get the database path in the application's directory
                var appDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                var databasePath = Path.Combine(appDirectory, "BMATM.db");

                // Create the database factory
                DatabaseFactory = new SQLiteConnectionFactory(databasePath);

                // Initialize the database (create tables if they don't exist)
                DatabaseFactory.InitializeDatabase();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Failed to initialize database: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Creates and configures the main application window
        /// </summary>
        /// <returns>The configured main window</returns>
        private MainWindow CreateMainWindow()
        {
            // Create the main window ViewModel
            var mainWindowViewModel = new MainWindowViewModel(NavigationService);

            // Create and configure the main window
            var mainWindow = new MainWindow(mainWindowViewModel);

            // Set as the main window
            MainWindow = mainWindow;

            return mainWindow;
        }

        /// <summary>
        /// Initializes Phase 2 testing by navigating to the test view
        /// </summary>
        private void InitializePhase2Testing()
        {
            // Navigate to the navigation test view to verify Phase 2 functionality
            var testViewModel = new NavigationTestViewModel(NavigationService);

            // Get the main window ViewModel and set the test view
            if (MainWindow?.DataContext is MainWindowViewModel mainViewModel)
            {
                mainViewModel.CurrentViewModel = testViewModel;
            }
        }

        /// <summary>
        /// Cleans up application services
        /// </summary>
        private void CleanupServices()
        {
            // Clear navigation history
            NavigationService?.ClearHistory();

            // Remove event handlers
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
            DispatcherUnhandledException -= App_DispatcherUnhandledException;
        }

        /// <summary>
        /// Handles unhandled exceptions in the current domain
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Exception event arguments</param>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            ShowFatalError("An unhandled error occurred in the application.", exception);
        }

        /// <summary>
        /// Handles unhandled dispatcher exceptions
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Exception event arguments</param>
        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ShowError("An unexpected error occurred.", e.Exception);
            e.Handled = true; // Prevent application crash
        }

        /// <summary>
        /// Shows a fatal error message and shuts down the application
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="exception">Exception details</param>
        private void ShowFatalError(string message, Exception exception = null)
        {
            var detailMessage = message;
            if (exception != null)
            {
                detailMessage += $"\n\nDetails: {exception.Message}";

                // Log the full exception details
                LogException(exception);
            }

            MessageBox.Show(
                detailMessage,
                "Fatal Error - Application will exit",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            // Force application shutdown
            Environment.Exit(1);
        }

        /// <summary>
        /// Shows a recoverable error message
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="exception">Exception details</param>
        private void ShowError(string message, Exception exception = null)
        {
            var detailMessage = message;
            if (exception != null)
            {
                detailMessage += $"\n\nDetails: {exception.Message}";
                LogException(exception);
            }

            MessageBox.Show(
                detailMessage,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        /// <summary>
        /// Logs exception details to a file
        /// </summary>
        /// <param name="exception">Exception to log</param>
        private void LogException(Exception exception)
        {
            try
            {
                var appDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                var logPath = Path.Combine(appDirectory, "error.log");

                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {exception}\n\n";
                File.AppendAllText(logPath, logEntry);
            }
            catch
            {
                // Ignore logging errors to prevent recursive exceptions
            }
        }

        /// <summary>
        /// Gets the application version
        /// </summary>
        /// <returns>Application version string</returns>
        public static string GetVersion()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }

        /// <summary>
        /// Gets the application title with version
        /// </summary>
        /// <returns>Application title with version</returns>
        public static string GetApplicationTitle()
        {
            return $"BMATM v{GetVersion()} - Bank Misr ATM Management System";
        }
    }
}