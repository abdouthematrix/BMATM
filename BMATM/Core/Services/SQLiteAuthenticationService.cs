namespace BMATM.Services;

// Updated Authentication Service using Repository
public class SQLiteAuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private User? _currentUser;

    public bool IsAuthenticated => _currentUser != null;

    public SQLiteAuthenticationService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<AuthenticationResult> AuthenticateAsync(string username, string password)
    {
        try
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return new AuthenticationResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Username and password are required"
                };
            }

            var user = await _userRepository.GetByUsernameAsync(username);

            if (user != null && user.IsActive && IsValidPassword(password))
            {
                _currentUser = user;
                _currentUser.LastLogin = DateTime.Now;

                // Update last login in database
                await _userRepository.UpdateLastLoginAsync(username, DateTime.Now);

                return new AuthenticationResult
                {
                    IsSuccess = true,
                    User = _currentUser
                };
            }

            return new AuthenticationResult
            {
                IsSuccess = false,
                ErrorMessage = "Invalid username or password"
            };
        }
        catch (Exception ex)
        {
            return new AuthenticationResult
            {
                IsSuccess = false,
                ErrorMessage = "Authentication failed: " + ex.Message
            };
        }
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        await Task.CompletedTask;
        return _currentUser;
    }

    public async Task LogoutAsync()
    {
        await Task.CompletedTask;
        _currentUser = null;
    }

    private bool IsValidPassword(string password)
    {
        // Simple mock password validation - replace with proper authentication
        return password == "password" || password == "123";
    }
}