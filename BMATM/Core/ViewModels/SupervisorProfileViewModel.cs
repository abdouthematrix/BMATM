namespace BMATM.ViewModels;
public class SupervisorProfileViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IUserDataService _userDataService;
    private readonly IATMDataService _atmDataService;
    private SupervisorProfile? _supervisorProfile;
    private bool _isLoading;
    private ObservableCollection<ATMInfo> _atms;
    private ATMInfo _selectedATM;

    public SupervisorProfile? SupervisorProfile
    {
        get => _supervisorProfile;
        set => SetProperty(ref _supervisorProfile, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public ObservableCollection<ATMInfo> ATMs
    {
        get => _atms;
        set => SetProperty(ref _atms, value);
    }

    public ATMInfo SelectedATM
    {
        get => _selectedATM;
        set
        {
            SetProperty(ref _selectedATM, value);
            ((RelayCommand<ATMInfo>)EditATMCommand).RaiseCanExecuteChanged();
            ((RelayCommand<ATMInfo>)DeleteATMCommand).RaiseCanExecuteChanged();
        }
    }

    public ICommand LogoutCommand { get; }
    public ICommand AddATMCommand { get; }
    public ICommand EditATMCommand { get; }
    public ICommand DeleteATMCommand { get; }
    public ICommand RefreshCommand { get; }

    public SupervisorProfileViewModel(
        IAuthenticationService authenticationService,
        IUserDataService userDataService,
        IATMDataService atmDataService)
    {
        _authenticationService = authenticationService;
        _userDataService = userDataService;
        _atmDataService = atmDataService;
        _atms = new ObservableCollection<ATMInfo>();

        LogoutCommand = new RelayCommand(async () => await LogoutAsync());
        AddATMCommand = new RelayCommand(AddATM);
        EditATMCommand = new RelayCommand<ATMInfo>(ExecuteEditATM, CanExecuteEditATM);
        DeleteATMCommand = new RelayCommand<ATMInfo>(ExecuteDeleteATM, CanExecuteDeleteATM);
        RefreshCommand = new RelayCommand(async () => await RefreshDataAsync());

        _ = LoadUserDataAsync();
    }

    private async Task LoadUserDataAsync()
    {
        try
        {
            IsLoading = true;

            // Load current user from authentication service
            var currentUser = await _authenticationService.GetCurrentUserAsync();

            if (currentUser != null)
            {
                // Load or create supervisor profile
                SupervisorProfile = await _userDataService.GetSupervisorProfileAsync(currentUser.Username);

                if (SupervisorProfile == null)
                {
                    // Create new supervisor profile if it doesn't exist
                    SupervisorProfile = new SupervisorProfile
                    {
                        User = currentUser,
                        ATMs = new ObservableCollection<ATMInfo>()
                    };

                    await _userDataService.SaveSupervisorProfileAsync(SupervisorProfile);
                }
                else
                {
                    // Update last login date
                    SupervisorProfile.User.LastLogin = DateTime.Now;
                    await _userDataService.UpdateSupervisorProfileAsync(SupervisorProfile);
                }

                // Load ATMs
                await LoadATMsAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading user data: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadATMsAsync()
    {
        try
        {
            var atms = await _atmDataService.GetAllATMsAsync();
            ATMs.Clear();

            foreach (var atm in atms)
            {
                ATMs.Add(atm);
            }

            // Update supervisor profile ATMs if needed
            if (SupervisorProfile != null)
            {
                SupervisorProfile.ATMs = new ObservableCollection<ATMInfo>(atms);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading ATMs: {ex.Message}");
        }
    }

    private async Task LogoutAsync()
    {
        try
        {
            await _authenticationService.LogoutAsync();
            NavigationHelper.NavigateTo<LoginView>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during logout: {ex.Message}");
        }
    }

    private void AddATM()
    {
        NavigationHelper.NavigateTo<AddATMView>();
    }

    private void ExecuteEditATM(ATMInfo atm)
    {
        if (atm != null)
        {
            NavigationHelper.NavigateTo<AddATMView>();
        }
    }

    private bool CanExecuteEditATM(ATMInfo atm)
    {
        return atm != null;
    }

    private async void ExecuteDeleteATM(ATMInfo atm)
    {
        if (atm != null)
        {
            var result = System.Windows.MessageBox.Show(
                $"Are you sure you want to delete ATM {atm.ATMNumber}?",
                "Confirm Delete",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                var success = await _atmDataService.DeleteATMAsync(atm.ATMNumber);
                if (success)
                {
                    ATMs.Remove(atm);
                    if (SupervisorProfile?.ATMs != null)
                    {
                        var profileAtm = SupervisorProfile.ATMs.FirstOrDefault(a => a.ATMNumber == atm.ATMNumber);
                        if (profileAtm != null)
                        {
                            SupervisorProfile.ATMs.Remove(profileAtm);
                        }
                    }

                    if (SelectedATM == atm)
                    {
                        SelectedATM = null;
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show(
                        "Failed to delete ATM. Please try again.",
                        "Error",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);
                }
            }
        }
    }

    private bool CanExecuteDeleteATM(ATMInfo atm)
    {
        return atm != null;
    }

    private async Task RefreshDataAsync()
    {
        await LoadUserDataAsync();
    }
}