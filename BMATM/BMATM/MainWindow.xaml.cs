using System;
using System.IO;
using System.Windows;
using BMATM.Data;
using BMATM.Data.Repositories;
using BMATM.Data.Schema;
using BMATM.Services.Navigation;
using BMATM.ViewModels;

namespace BMATM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;
        private SQLiteConnectionFactory _connectionFactory;

        public MainWindow()
        {
            InitializeComponent();
            InitializeApplication();
        }

        private void InitializeApplication()
        {
            try
            {
                // Initialize database
                InitializeDatabase();

                // Initialize repositories
                var supervisorRepository = new SupervisorRepository(_connectionFactory);
                var atmRepository = new ATMRepository(_connectionFactory);
                var transactionRepository = new TransactionRepository(_connectionFactory);

                // Initialize navigation service
                var navigationService = new NavigationService(supervisorRepository, atmRepository, transactionRepository);

                // Initialize main viewmodel
                _viewModel = new MainWindowViewModel(navigationService);
                DataContext = _viewModel;

                // Handle application closing
                Closing += MainWindow_Closing;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to initialize application: {ex.Message}",
                    "Initialization Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Application.Current.Shutdown(1);
            }
        }

        private void InitializeDatabase()
        {
            try
            {
                // Create database in application directory
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string databasePath = Path.Combine(appDirectory, "BMATM.db");

                _connectionFactory = new SQLiteConnectionFactory(databasePath);

                // Initialize database schema and sample data
                var databaseInitializer = new DatabaseInitializer(_connectionFactory);
                databaseInitializer.InitializeDatabase();

                // Verify database connection
                using (var connection = _connectionFactory.CreateConnection())
                {
                    connection.Open();
                    // Database is ready
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to initialize database", ex);
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Confirm exit if user is logged in
            if (_viewModel?.CurrentSupervisor != null)
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

            // Cleanup
            _viewModel?.OnViewUnloaded();
        }
    }
}