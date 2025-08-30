using System;
using System.Collections.Generic;
using BMATM.Core.Entities;
using BMATM.Data.Repositories;
using BMATM.ViewModels;
using BMATM.ViewModels.Base;

namespace BMATM.Services.Navigation
{
    public class NavigationService
    {
        private readonly SupervisorRepository _supervisorRepository;
        private readonly ATMRepository _atmRepository;
        private readonly TransactionRepository _transactionRepository;
        private readonly Stack<BaseViewModel> _navigationStack;

        public NavigationService(
            SupervisorRepository supervisorRepository,
            ATMRepository atmRepository,
            TransactionRepository transactionRepository)
        {
            _supervisorRepository = supervisorRepository ?? throw new ArgumentNullException(nameof(supervisorRepository));
            _atmRepository = atmRepository ?? throw new ArgumentNullException(nameof(atmRepository));
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
            _navigationStack = new Stack<BaseViewModel>();
        }

        public event Action<BaseViewModel> NavigationRequested;

        #region Navigation Methods

        public void NavigateTo<T>() where T : BaseViewModel, new()
        {
            var viewModel = new T();
            NavigateToViewModel(viewModel);
        }

        public void NavigateTo<T>(object parameter) where T : BaseViewModel, new()
        {
            var viewModel = new T();

            // Pass parameter to viewmodel if it has a parameter property or method
            if (viewModel is IParameterizedViewModel parameterized)
            {
                parameterized.SetParameter(parameter);
            }

            NavigateToViewModel(viewModel);
        }

        public void NavigateToLogin()
        {
            var loginViewModel = new LoginViewModel(_supervisorRepository, this);
            NavigateToViewModel(loginViewModel, clearStack: true);
        }

        public void NavigateToATMCarousel(Supervisor supervisor)
        {
            if (supervisor == null)
                throw new ArgumentNullException(nameof(supervisor));

            var atmCarouselViewModel = new ATMCarouselViewModel(_atmRepository, this);
            atmCarouselViewModel.LoadATMs(supervisor.Id);
            atmCarouselViewModel.CurrentSupervisor = supervisor;

            NavigateToViewModel(atmCarouselViewModel, clearStack: true);
        }

        public void NavigateToCashReconciliation(ATM atm, DateTime date)
        {
            if (atm == null)
                throw new ArgumentNullException(nameof(atm));

            var cashReconciliationViewModel = new CashReconciliationViewModel(_transactionRepository, this);
            cashReconciliationViewModel.Initialize(atm, date);

            NavigateToViewModel(cashReconciliationViewModel);
        }

        public void NavigateToGLReconciliation(ATM atm, DateTime date)
        {
            if (atm == null)
                throw new ArgumentNullException(nameof(atm));

            var glReconciliationViewModel = new GLReconciliationViewModel(_transactionRepository, this);
            glReconciliationViewModel.Initialize(atm, date);

            NavigateToViewModel(glReconciliationViewModel);
        }

        public void NavigateBack()
        {
            if (_navigationStack.Count > 1)
            {
                // Remove current view
                _navigationStack.Pop();

                // Navigate to previous view
                var previousViewModel = _navigationStack.Peek();
                NavigationRequested?.Invoke(previousViewModel);
            }
        }

        #endregion

        #region Helper Methods

        private void NavigateToViewModel(BaseViewModel viewModel, bool clearStack = false)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            if (clearStack)
            {
                _navigationStack.Clear();
            }

            _navigationStack.Push(viewModel);
            NavigationRequested?.Invoke(viewModel);
        }

        public bool CanNavigateBack => _navigationStack.Count > 1;

        #endregion
    }

    // Interface for ViewModels that accept parameters
    public interface IParameterizedViewModel
    {
        void SetParameter(object parameter);
    }
}