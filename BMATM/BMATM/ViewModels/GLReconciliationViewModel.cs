using BMATM.Core.Entities;
using BMATM.Data.Repositories;
using BMATM.Services.Navigation;
using BMATM.ViewModels.Base;
using System;

namespace BMATM.ViewModels
{
    /// <summary>
    /// Placeholder GLReconciliationViewModel for Phase 3 compilation
    /// This will be fully implemented in Phase 7
    /// </summary>
    public class GLReconciliationViewModel : BaseViewModel
    {
        private readonly TransactionRepository _transactionRepository;
        private readonly NavigationService _navigationService;

        public GLReconciliationViewModel(TransactionRepository transactionRepository, NavigationService navigationService)
        {
            _transactionRepository = transactionRepository;
            _navigationService = navigationService;
        }

        public void Initialize(ATM atm, DateTime date)
        {
            // Placeholder - will be implemented in Phase 7
        }
    }
}