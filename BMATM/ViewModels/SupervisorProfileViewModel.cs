namespace BMATM.ViewModels;

public class SupervisorProfileViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IUserDataService _userDataService;
    private User? _currentUser;
    private SupervisorProfile? _supervisorProfile;
    private bool _isLoading;
    private string _atmCountDisplay = "0";

    public User? CurrentUser
    {
        get => _currentUser;
        set => SetProperty(ref _currentUser, value);
    }

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

    public string AtmCountDisplay
    {
        get => _atmCountDisplay;
        set => SetProperty(ref _atmCountDisplay, value);
    }

    public string FullName => SupervisorProfile?.FullName ?? CurrentUser?.DisplayName ?? "N/A";
    public string BranchNumber => SupervisorProfile?.BranchNumber ?? CurrentUser?.BranchCode ?? "N/A";
    public string BranchName => SupervisorProfile?.BranchName ?? CurrentUser?.BranchName ?? "N/A";
    public string Username => SupervisorProfile?.Username ?? CurrentUser?.Username ?? "N/A";
    public string LastLoginDisplay => SupervisorProfile?.LastLoginDate.ToString("dd/MM/yyyy HH:mm") ?? "N/A";

    public ICommand LogoutCommand { get; }
    public ICommand AddATMCommand { get; }
    public ICommand RefreshCommand { get; }

    public SupervisorProfileViewModel(IAuthenticationService authenticationService, IUserDataService userDataService)
    {
        _authenticationService = authenticationService;
        _userDataService = userDataService;

        LogoutCommand = new RelayCommand(async () => await LogoutAsync());
        AddATMCommand = new RelayCommand(AddATM);
        RefreshCommand = new RelayCommand(async () => await RefreshDataAsync());

        _ = LoadUserDataAsync();
    }

    private async Task LoadUserDataAsync()
    {
        try
        {
            IsLoading = true;

            // Load current user from authentication service
            CurrentUser = await _authenticationService.GetCurrentUserAsync();

            if (CurrentUser != null)
            {
                // Load or create supervisor profile
                SupervisorProfile = await _userDataService.GetSupervisorProfileAsync(CurrentUser.Username);

                if (SupervisorProfile == null)
                {
                    // Create new profile if it doesn't exist
                    SupervisorProfile = new SupervisorProfile
                    {
                        Username = CurrentUser.Username,
                        FullName = CurrentUser.DisplayName,
                        BranchNumber = CurrentUser.BranchCode,
                        BranchName = CurrentUser.BranchName,
                        AtmCollectionCount = 0,
                        EmployeeId = CurrentUser.Username,
                        LastLoginDate = DateTime.Now,
                        IsActive = true
                    };

                    await _userDataService.SaveSupervisorProfileAsync(SupervisorProfile);
                }
                else
                {
                    // Update last login date
                    SupervisorProfile.LastLoginDate = DateTime.Now;
                    await _userDataService.UpdateSupervisorProfileAsync(SupervisorProfile);
                }

                UpdateAtmCountDisplay();

                // Notify property changes
                OnPropertyChanged(nameof(FullName));
                OnPropertyChanged(nameof(BranchNumber));
                OnPropertyChanged(nameof(BranchName));
                OnPropertyChanged(nameof(Username));
                OnPropertyChanged(nameof(LastLoginDisplay));
            }
        }
        catch (Exception ex)
        {
            // Handle error - in a real app, you might want to show a message to the user
            System.Diagnostics.Debug.WriteLine($"Error loading user data: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void UpdateAtmCountDisplay()
    {
        if (SupervisorProfile != null)
        {
            AtmCountDisplay = SupervisorProfile.AtmCollectionCount.ToString();
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
        // Navigate to Add ATM view
        NavigationHelper.NavigateTo<ATMCollectionView>();
    }

    private async Task RefreshDataAsync()
    {
        await LoadUserDataAsync();
    }
}