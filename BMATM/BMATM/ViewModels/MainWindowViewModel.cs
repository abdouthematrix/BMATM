using System;
using BMATM.Core.Entities;
using BMATM.Services.Navigation;
using BMATM.ViewModels.Base;

namespace BMATM.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private BaseViewModel _currentViewModel;
        private string _windowTitle;
        private Supervisor _currentSupervisor;
        private readonly NavigationService _navigationService;

        public MainWindowViewModel(NavigationService navigationService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            // Subscribe to navigation events
            _navigationService.NavigationRequested += OnNavigationRequested;

            // Initialize with login view
            WindowTitle = "BMATM - Branch ATM Management System";

            // Start with login view
            NavigateToLogin();
        }

        #region Properties

        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                if (SetProperty(ref _currentViewModel, value))
                {
                    // Notify the previous view it's being unloaded
                    if (_currentViewModel != null)
                    {
                        _currentViewModel.OnViewUnloaded();
                    }

                    // Notify the new view it's being loaded
                    if (value != null)
                    {
                        value.OnViewLoaded();
                    }

                    UpdateWindowTitle();
                }
            }
        }

        public string WindowTitle
        {
            get => _windowTitle;
            set => SetProperty(ref _windowTitle, value);
        }

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

        #endregion

        #region Navigation Event Handlers

        private void OnNavigationRequested(BaseViewModel viewModel)
        {
            CurrentViewModel = viewModel;
        }

        #endregion

        #region Navigation Methods

        private void NavigateToLogin()
        {
            // Clear current supervisor session
            CurrentSupervisor = null;
            _navigationService.NavigateToLogin();
        }

        public void HandleSuccessfulLogin(Supervisor supervisor)
        {
            CurrentSupervisor = supervisor;
            _navigationService.NavigateToATMCarousel(supervisor);
        }

        public void HandleLogout()
        {
            NavigateToLogin();
        }

        #endregion

        #region Helper Methods

        private void UpdateWindowTitle()
        {
            if (CurrentSupervisor != null)
            {
                WindowTitle = $"BMATM - {CurrentSupervisor.FullName} - {GetCurrentViewTitle()}";
            }
            else
            {
                WindowTitle = "BMATM - Branch ATM Management System";
            }
        }

        private string GetCurrentViewTitle()
        {
            switch (CurrentViewModel)
            {
                case LoginViewModel _: return "Login";
                case ATMCarouselViewModel _: return "ATM Management";
                case CashReconciliationViewModel _: return "Cash Reconciliation";
                case GLReconciliationViewModel _: return "GL Reconciliation";
                default: return "System";
            }
        }


        #endregion

        #region Cleanup

        public override void OnViewUnloaded()
        {
            base.OnViewUnloaded();

            // Unsubscribe from events to prevent memory leaks
            if (_navigationService != null)
            {
                _navigationService.NavigationRequested -= OnNavigationRequested;
            }
        }

        #endregion
    }
}