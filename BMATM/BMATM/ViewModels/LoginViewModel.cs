using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BMATM.Core.Entities;
using BMATM.Data.Repositories;
using BMATM.Services.Navigation;
using BMATM.ViewModels.Base;

namespace BMATM.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly SupervisorRepository _supervisorRepository;
        private readonly NavigationService _navigationService;

        private string _username;
        private string _password;
        private string _errorMessage;
        private bool _isLoading;

        public LoginViewModel(SupervisorRepository supervisorRepository, NavigationService navigationService)
        {
            _supervisorRepository = supervisorRepository ?? throw new ArgumentNullException(nameof(supervisorRepository));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
            ExitCommand = new RelayCommand(ExecuteExit);

            // Initialize with empty values
            Username = string.Empty;
            Password = string.Empty;
            ErrorMessage = string.Empty;
        }

        #region Properties

        public string Username
        {
            get => _username;
            set
            {
                if (SetProperty(ref _username, value))
                {
                    ClearError();
                    OnPropertyChanged(nameof(IsLoginEnabled));
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    ClearError();
                    OnPropertyChanged(nameof(IsLoginEnabled));
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    OnPropertyChanged(nameof(IsLoginEnabled));
                }
            }
        }

        public bool IsLoginEnabled => !IsLoading && !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);

        #endregion

        #region Commands

        public ICommand LoginCommand { get; }
        public ICommand ExitCommand { get; }

        #endregion

        #region Command Implementations

        private async void ExecuteLogin()
        {
            if (!CanExecuteLogin())
                return;

            try
            {
                IsLoading = true;
                ClearError();

                // Validate input
                if (string.IsNullOrWhiteSpace(Username))
                {
                    ShowError("Please enter your username.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(Password))
                {
                    ShowError("Please enter your password.");
                    return;
                }

                // Simulate async operation for better UX
                await Task.Delay(500);

                // Hash the password for comparison
                string passwordHash = HashPassword(Password);

                // Validate credentials against database
                var supervisor = _supervisorRepository.GetByUsername(Username.Trim());

                if (supervisor == null)
                {
                    ShowError("Invalid username or password.");
                    return;
                }

                if (!supervisor.IsActive)
                {
                    ShowError("This account has been disabled. Please contact your administrator.");
                    return;
                }

                // Verify password
                if (supervisor.PasswordHash != passwordHash)
                {
                    ShowError("Invalid username or password.");
                    return;
                }

                // Update last login date
                _supervisorRepository.UpdateLastLoginDate(supervisor.Id);

                // Clear form and navigate to ATM carousel
                ClearForm();
                _navigationService.NavigateToATMCarousel(supervisor);
            }
            catch (Exception ex)
            {
                ShowError($"Login failed: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanExecuteLogin()
        {
            return !IsLoading && !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
        }

        private void ExecuteExit()
        {
            var result = MessageBox.Show(
                "Are you sure you want to exit the application?",
                "Exit Application",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        #endregion

        #region Helper Methods

        private string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;

            using (var sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "BMATM_SALT"));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private void ShowError(string message)
        {
            ErrorMessage = message;
        }

        private void ClearError()
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ErrorMessage = string.Empty;
            }
        }

        private void ClearForm()
        {
            Username = string.Empty;
            Password = string.Empty;
            ClearError();
        }

        #endregion

        #region View Events

        public override void OnViewLoaded()
        {
            base.OnViewLoaded();
            ClearForm();
        }

        public override void OnViewUnloaded()
        {
            base.OnViewUnloaded();
            ClearForm();
        }

        #endregion
    }
}