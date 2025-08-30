using System;
using System.Collections.Generic;
using BMATM.ViewModels.Base;
using BMATM.Core.Entities;

namespace BMATM.Services.Navigation
{
    /// <summary>
    /// Service for managing navigation between different views in the application
    /// </summary>
    public class NavigationService
    {
        private readonly Stack<BaseViewModel> _navigationHistory;

        /// <summary>
        /// Initializes a new instance of NavigationService
        /// </summary>
        public NavigationService()
        {
            _navigationHistory = new Stack<BaseViewModel>();
        }

        /// <summary>
        /// Event raised when navigation to a new view is requested
        /// </summary>
        public event Action<BaseViewModel> NavigationRequested;

        /// <summary>
        /// Navigates to a view of the specified ViewModel type
        /// </summary>
        /// <typeparam name="T">The ViewModel type to navigate to</typeparam>
        public void NavigateTo<T>() where T : BaseViewModel, new()
        {
            var viewModel = new T();
            NavigateToViewModel(viewModel);
        }

        /// <summary>
        /// Navigates to a view of the specified ViewModel type with a parameter
        /// </summary>
        /// <typeparam name="T">The ViewModel type to navigate to</typeparam>
        /// <param name="parameter">Parameter to pass to the ViewModel</param>
        public void NavigateTo<T>(object parameter) where T : BaseViewModel, new()
        {
            var viewModel = new T();

            // If the ViewModel has a method to accept parameters, call it
            if (viewModel is IParameterizedViewModel parameterized)
            {
                parameterized.SetParameter(parameter);
            }

            NavigateToViewModel(viewModel);
        }

        /// <summary>
        /// Navigates to the login view
        /// </summary>
        public void NavigateToLogin()
        {
            ClearHistory();
            // Will be implemented when LoginViewModel is created
            // NavigateTo<LoginViewModel>();
        }

        /// <summary>
        /// Navigates to the ATM carousel view with supervisor context
        /// </summary>
        /// <param name="supervisor">The logged-in supervisor</param>
        public void NavigateToATMCarousel(Supervisor supervisor)
        {
            // Will be implemented when ATMCarouselViewModel is created
            // NavigateTo<ATMCarouselViewModel>(supervisor);
        }

        /// <summary>
        /// Navigates to the cash reconciliation view
        /// </summary>
        /// <param name="atm">The ATM to reconcile</param>
        /// <param name="date">The reconciliation date</param>
        public void NavigateToCashReconciliation(ATM atm, DateTime date)
        {
            var parameter = new ReconciliationParameter { ATM = atm, Date = date };
            // Will be implemented when CashReconciliationViewModel is created
            // NavigateTo<CashReconciliationViewModel>(parameter);
        }

        /// <summary>
        /// Navigates to the GL reconciliation view
        /// </summary>
        /// <param name="atm">The ATM to reconcile</param>
        /// <param name="date">The reconciliation date</param>
        public void NavigateToGLReconciliation(ATM atm, DateTime date)
        {
            var parameter = new ReconciliationParameter { ATM = atm, Date = date };
            // Will be implemented when GLReconciliationViewModel is created
            // NavigateTo<GLReconciliationViewModel>(parameter);
        }

        /// <summary>
        /// Navigates back to the previous view if available
        /// </summary>
        public void NavigateBack()
        {
            if (_navigationHistory.Count > 1)
            {
                // Remove current view
                _navigationHistory.Pop();

                // Navigate to previous view
                var previousViewModel = _navigationHistory.Peek();
                NavigationRequested?.Invoke(previousViewModel);
            }
        }

        /// <summary>
        /// Clears the navigation history
        /// </summary>
        public void ClearHistory()
        {
            _navigationHistory.Clear();
        }

        /// <summary>
        /// Gets whether there is a previous view to navigate back to
        /// </summary>
        public bool CanNavigateBack => _navigationHistory.Count > 1;

        /// <summary>
        /// Common method for navigating to a ViewModel instance
        /// </summary>
        /// <param name="viewModel">The ViewModel to navigate to</param>
        private void NavigateToViewModel(BaseViewModel viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            _navigationHistory.Push(viewModel);
            NavigationRequested?.Invoke(viewModel);
        }
    }

    /// <summary>
    /// Interface for ViewModels that can accept parameters
    /// </summary>
    public interface IParameterizedViewModel
    {
        /// <summary>
        /// Sets a parameter for the ViewModel
        /// </summary>
        /// <param name="parameter">The parameter to set</param>
        void SetParameter(object parameter);
    }

    /// <summary>
    /// Parameter class for reconciliation views
    /// </summary>
    public class ReconciliationParameter
    {
        /// <summary>
        /// The ATM to reconcile
        /// </summary>
        public ATM ATM { get; set; }

        /// <summary>
        /// The reconciliation date
        /// </summary>
        public DateTime Date { get; set; }
    }
}