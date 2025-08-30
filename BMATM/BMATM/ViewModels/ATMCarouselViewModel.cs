using BMATM.Core.Entities;
using BMATM.Data.Repositories;
using BMATM.Services.Navigation;
using BMATM.ViewModels.Base;

namespace BMATM.ViewModels
{
    /// <summary>
    /// Placeholder ATMCarouselViewModel for Phase 3 compilation
    /// This will be fully implemented in Phase 4
    /// </summary>
    public class ATMCarouselViewModel : BaseViewModel
    {
        private readonly ATMRepository _atmRepository;
        private readonly NavigationService _navigationService;
        private Supervisor _currentSupervisor;

        public ATMCarouselViewModel(ATMRepository atmRepository, NavigationService navigationService)
        {
            _atmRepository = atmRepository;
            _navigationService = navigationService;
        }

        public Supervisor CurrentSupervisor
        {
            get => _currentSupervisor;
            set => SetProperty(ref _currentSupervisor, value);
        }

        public void LoadATMs(int supervisorId)
        {
            // Placeholder - will be implemented in Phase 4
        }
    }
}