namespace BMATM.ViewModels;

public class SupervisorProfileViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authenticationService;
    private User? _currentUser;

    public User? CurrentUser
    {
        get => _currentUser;
        set => SetProperty(ref _currentUser, value);
    }

    public ICommand LogoutCommand { get; }
    public ICommand AddATMCommand { get; }

    public SupervisorProfileViewModel(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
        LogoutCommand = new RelayCommand(async () => await LogoutAsync());
        AddATMCommand = new RelayCommand(AddATM);

        LoadCurrentUser();
    }

    private async void LoadCurrentUser()
    {
        CurrentUser = await _authenticationService.GetCurrentUserAsync();
    }

    private async Task LogoutAsync()
    {
        await _authenticationService.LogoutAsync();
        NavigateToLogin();
    }

    private void AddATM()
    {
        // TODO: Navigate to Add ATM view
        // This will be implemented in the next phase
    }

    private void NavigateToLogin()
    {
        var loginView = new LoginView();
        Application.Current.MainWindow.Content = loginView;
    }
}