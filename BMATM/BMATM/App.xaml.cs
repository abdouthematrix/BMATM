using System;
using System.IO;
using System.Windows;

namespace BMATM
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Handle global unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            // Set up application logging directory
            SetupLogging();

            // Log application startup
            LogMessage("Application starting...");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            LogMessage("Application shutting down...");
            base.OnExit(e);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogError("Unhandled domain exception", e.ExceptionObject as Exception);

            MessageBox.Show(
                "A critical error occurred. The application will now close.\n\nPlease check the logs for more details.",
                "Critical Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogError("Unhandled dispatcher exception", e.Exception);

            MessageBox.Show(
                $"An unexpected error occurred:\n\n{e.Exception.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            e.Handled = true; // Prevent application crash
        }

        private void SetupLogging()
        {
            try
            {
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string logsDirectory = Path.Combine(appDirectory, "Logs");

                if (!Directory.Exists(logsDirectory))
                {
                    Directory.CreateDirectory(logsDirectory);
                }
            }
            catch
            {
                // If we can't create logs directory, just continue
            }
        }

        private void LogMessage(string message)
        {
            try
            {
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string logsDirectory = Path.Combine(appDirectory, "Logs");
                string logFile = Path.Combine(logsDirectory, $"BMATM_{DateTime.Now:yyyyMMdd}.log");

                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INFO: {message}";
                File.AppendAllText(logFile, logEntry + Environment.NewLine);
            }
            catch
            {
                // If logging fails, don't crash the application
            }
        }

        private void LogError(string message, Exception exception)
        {
            try
            {
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string logsDirectory = Path.Combine(appDirectory, "Logs");
                string logFile = Path.Combine(logsDirectory, $"BMATM_{DateTime.Now:yyyyMMdd}.log");

                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {message}";
                if (exception != null)
                {
                    logEntry += Environment.NewLine + $"Exception: {exception}";
                }

                File.AppendAllText(logFile, logEntry + Environment.NewLine);
            }
            catch
            {
                // If logging fails, don't crash the application
            }
        }
    }
}