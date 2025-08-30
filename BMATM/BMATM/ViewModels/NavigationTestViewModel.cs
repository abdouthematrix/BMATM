using System.Windows.Input;
using BMATM.ViewModels.Base;
using BMATM.Services.Navigation;
using BMATM.Core.Entities;
using System;

namespace BMATM.ViewModels
{
    /// <summary>
    /// Test ViewModel to verify navigation functionality in Phase 2
    /// This will be removed once actual ViewModels are implemented
    /// </summary>
    public class NavigationTestViewModel : BaseViewModel
    {
        private readonly NavigationService _navigationService;
        private string _currentView;

        public NavigationTestViewModel(NavigationService navigationService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _currentView = "Navigation Test";

            // Initialize commands
            NavigateToLoginCommand = new RelayCommand(ExecuteNavigateToLogin);
            NavigateToATMCarouselCommand = new RelayCommand(ExecuteNavigateToATMCarousel);
            NavigateToCashReconciliationCommand = new RelayCommand(ExecuteNavigateToCashReconciliation);
            NavigateToGLReconciliationCommand = new RelayCommand(ExecuteNavigateToGLReconciliation);
            NavigateBackCommand = new RelayCommand(ExecuteNavigateBack, CanNavigateBack);
            ShowPlaceholderCommand = new RelayCommand(ExecuteShowPlaceholder);
        }

        /// <summary>
        /// Gets or sets the current view name
        /// </summary>
        public string CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        /// <summary>
        /// Command to navigate to login view
        /// </summary>
        public ICommand NavigateToLoginCommand { get; }

        /// <summary>
        /// Command to navigate to ATM carousel view
        /// </summary>
        public ICommand NavigateToATMCarouselCommand { get; }

        /// <summary>
        /// Command to navigate to cash reconciliation view
        /// </summary>
        public ICommand NavigateToCashReconciliationCommand { get; }

        /// <summary>
        /// Command to navigate to GL reconciliation view
        /// </summary>
        public ICommand NavigateToGLReconciliationCommand { get; }

        /// <summary>
        /// Command to navigate back
        /// </summary>
        public ICommand NavigateBackCommand { get; }

        /// <summary>
        /// Command to show placeholder view
        /// </summary>
        public ICommand ShowPlaceholderCommand { get; }

        private void ExecuteNavigateToLogin()
        {
            _navigationService.NavigateTo<LoginViewModel>();
            CurrentView = "Navigated to Login View";
        }

        private void ExecuteNavigateToATMCarousel()
        {
            // Create a test supervisor for navigation
            var testSupervisor = new Supervisor
            {
                Id = 1,
                Username = "testuser",
                FullName = "Test User",
                Email = "test@example.com"
            };

            _navigationService.NavigateToATMCarousel(testSupervisor);
            CurrentView = "Navigated to ATM Carousel View";
        }

        private void ExecuteNavigateToCashReconciliation()
        {
            // Create test ATM for navigation
            var testATM = new ATM
            {
                Id = 1,
                Name = "Test ATM",
                Branch = "Test Branch",
                GLNumber = "123456789"
            };

            _navigationService.NavigateToCashReconciliation(testATM, DateTime.Today);
            CurrentView = "Navigated to Cash Reconciliation View";
        }

        private void ExecuteNavigateToGLReconciliation()
        {
            // Create test ATM for navigation
            var testATM = new ATM
            {
                Id = 1,
                Name = "Test ATM",
                Branch = "Test Branch",
                GLNumber = "123456789"
            };

            _navigationService.NavigateToGLReconciliation(testATM, DateTime.Today);
            CurrentView = "Navigated to GL Reconciliation View";
        }

        private void ExecuteNavigateBack()
        {
            _navigationService.NavigateBack();
            CurrentView = "Navigated Back";
        }

        private bool CanNavigateBack()
        {
            return _navigationService.CanNavigateBack;
        }

        private void ExecuteShowPlaceholder()
        {
            // Create a custom placeholder view
            var placeholder = new PlaceholderViewModel("Custom Test View");
            CurrentView = "Showing Custom Placeholder";
        }

        public override void OnViewLoaded()
        {
            base.OnViewLoaded();
            CurrentView = "Navigation Test View Loaded";
        }

        public override void OnViewUnloaded()
        {
            base.OnViewUnloaded();
            CurrentView = "Navigation Test View Unloaded";
        }
    }
}