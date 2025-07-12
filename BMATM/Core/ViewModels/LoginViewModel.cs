namespace BMATM.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly DatabaseInitializationManager database;
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

    public LoginViewModel(DatabaseInitializationManager DatabaseManager)
    {
        database = DatabaseManager;
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
            var result = await database._userService.AuthenticateAsync(Username, Password);

            if (result.IsSuccess)
            {
                if (RememberMe)
                    Settings.Default.SavedUsername = Username;
                else
                    Settings.Default.SavedUsername = string.Empty;
                Settings.Default.RememberMe = RememberMe;
                Settings.Default.Save();
                // Navigate to supervisor profile or main dashboard
                NavigationHelper.NavigateTo<SupervisorProfileView, User>(result.User);
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