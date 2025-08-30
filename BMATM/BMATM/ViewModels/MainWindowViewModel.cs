using System;
using BMATM.ViewModels.Base;
using BMATM.Services.Navigation;
using BMATM.Core.Entities;

namespace BMATM.ViewModels
{
    /// <summary>
    /// ViewModel for the main application window
    /// </summary>
    public class MainWindowViewModel : BaseViewModel
    {
        private BaseViewModel _currentViewModel;
        private string _windowTitle;
        private Supervisor _currentSupervisor;
        private readonly NavigationService _navigationService;

        /// <summary>
        /// Initializes a new instance of MainWindowViewModel
        /// </summary>
        /// <param name="navigationService">The navigation service</param>
        public MainWindowViewModel(NavigationService navigationService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            // Subscribe to navigation events
            _navigationService.NavigationRequested += OnNavigationRequested;

            // Set initial window title
            WindowTitle = "BMATM - Bank Misr ATM Management System";

            // Navigate to login view initially (will be implemented in Phase 3)
            // For now, we'll set a placeholder
            CurrentViewModel = new PlaceholderViewModel("Login View");
        }

        /// <summary>
        /// Gets or sets the currently displayed ViewModel
        /// </summary>
        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        /// <summary>
        /// Gets or sets the main window title
        /// </summary>
        public string WindowTitle
        {
            get => _windowTitle;
            set => SetProperty(ref _windowTitle, value);
        }

        /// <summary>
        /// Gets or sets the currently logged-in supervisor
        /// </summary>
        public Supervisor CurrentSupervisor
        {
            get => _currentSupervisor;
            set
            {
                if (SetProperty(ref _currentSupervisor, value))
                {
                    UpdateWindowTitle();
                }
            }
        }

        /// <summary>
        /// Gets the navigation service instance
        /// </summary>
        public NavigationService NavigationService => _navigationService;

        /// <summary>
        /// Handles navigation requests from the NavigationService
        /// </summary>
        /// <param name="viewModel">The ViewModel to navigate to</param>
        private void OnNavigationRequested(BaseViewModel viewModel)
        {
            if (viewModel != null)
            {
                // Notify previous ViewModel that it's being unloaded
                CurrentViewModel?.OnViewUnloaded();

                // Set the new ViewModel
                CurrentViewModel = viewModel;

                // Notify new ViewModel that it's being loaded
                CurrentViewModel.OnViewLoaded();

                // Update window title based on current view
                UpdateWindowTitle();
            }
        }

        /// <summary>
        /// Updates the window title based on the current context
        /// </summary>
        private void UpdateWindowTitle()
        {
            var baseTitle = "BMATM - Bank Misr ATM Management System";

            if (CurrentSupervisor != null)
            {
                WindowTitle = $"{baseTitle} - {CurrentSupervisor.FullName}";
            }
            else
            {
                WindowTitle = baseTitle;
            }
        }

        /// <summary>
        /// Logs out the current supervisor and returns to login
        /// </summary>
        public void Logout()
        {
            CurrentSupervisor = null;
            _navigationService.NavigateToLogin();
        }

        /// <summary>
        /// Called when the main window is closing
        /// </summary>
        public void OnWindowClosing()
        {
            // Perform cleanup
            CurrentViewModel?.OnViewUnloaded();

            // Unsubscribe from navigation events
            _navigationService.NavigationRequested -= OnNavigationRequested;
        }
    }

    /// <summary>
    /// Placeholder ViewModel for testing navigation during development
    /// </summary>
    public class PlaceholderViewModel : BaseViewModel
    {
        private string _content;

        /// <summary>
        /// Initializes a new instance of PlaceholderViewModel
        /// </summary>
        /// <param name="content">The content to display</param>
        public PlaceholderViewModel(string content)
        {
            Content = content;
        }

        /// <summary>
        /// Gets or sets the content to display
        /// </summary>
        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }
    }
}