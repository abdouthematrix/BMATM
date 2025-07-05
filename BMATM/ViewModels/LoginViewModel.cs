namespace BMATM.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authenticationService;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private bool _rememberMe;
    private string _errorMessage = string.Empty;
    private bool _isLoading = false;

    public string Username
    {
        get => _username;
        set
        {
            SetProperty(ref _username, value);
            ErrorMessage = string.Empty;
        }
    }

    public bool RememberMe
    {
        get => _rememberMe;
        set => SetProperty(ref _rememberMe, value);
    }

    public string Password
    {
        get => _password;
        set
        {
            SetProperty(ref _password, value);
            ErrorMessage = string.Empty;
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public ICommand LoginCommand { get; }

    public LoginViewModel(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
        LoginCommand = new RelayCommand(async () => await LoginAsync(), CanLogin);
        // Load saved username if remember is set
        RememberMe = Properties.Settings.Default.RememberMe;
        if (RememberMe)        
            Username = Properties.Settings.Default.SavedUsername;        
    }

    private bool CanLogin()
    {
        return !string.IsNullOrWhiteSpace(Username) &&
               !string.IsNullOrWhiteSpace(Password) &&
               !IsLoading;
    }

    private async Task LoginAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _authenticationService.AuthenticateAsync(Username, Password);

            if (result.IsSuccess)
            {
                if (RememberMe)                
                    Properties.Settings.Default.SavedUsername = Username;                
                else                
                    Properties.Settings.Default.SavedUsername = string.Empty;
                Properties.Settings.Default.RememberMe = RememberMe;
                Properties.Settings.Default.Save();
                // Navigate to supervisor profile or main dashboard
                NavigationHelper.NavigateTo<SupervisorProfileView>();               
            }
            else
            {
                ErrorMessage = result.ErrorMessage;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }
}