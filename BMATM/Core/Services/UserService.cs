namespace BMATM.Services;
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly Dictionary<string, int> _failedAttempts = new();
    private readonly Dictionary<string, DateTime> _lastAttempt = new();
    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 15;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task InitializeAsync()
    {
        await _userRepository.InitializeAsync().ConfigureAwait(false);
    }

    // Updated AuthenticateAsync method with localized messages
    public async Task<AuthenticationResult> AuthenticateAsync(string username, string password)
    {
        try
        {
            // Validate input parameters
            if (string.IsNullOrWhiteSpace(username))
            {
                return AuthenticationResult.Failure(
                    App.GetLocalizedString("AuthUsernameRequired"),
                    AuthenticationError.EmptyUsername);
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                return AuthenticationResult.Failure(
                    App.GetLocalizedString("AuthPasswordRequired"),
                    AuthenticationError.EmptyPassword);
            }

            // Trim whitespace
            username = username.Trim();

            // Check for too many failed attempts
            if (IsAccountLocked(username))
            {
                var lockoutTimeRemaining = GetLockoutTimeRemaining(username);
                return AuthenticationResult.Failure(
                    App.GetLocalizedString("AuthAccountLocked", lockoutTimeRemaining),
                    AuthenticationError.TooManyAttempts);
            }

            // Check if user exists
            var existingUser = await _userRepository.GetByUsernameAsync(username).ConfigureAwait(false);
            if (existingUser == null)
            {
                RecordFailedAttempt(username);
                return AuthenticationResult.Failure(
                    App.GetLocalizedString("AuthUserNotFound"),
                    AuthenticationError.UserNotFound);
            }

            // Check if user is active
            if (!existingUser.IsActive)
            {
                return AuthenticationResult.Failure(
                    App.GetLocalizedString("AuthUserInactive"),
                    AuthenticationError.UserInactive);
            }

            // Validate credentials
            var user = await _userRepository.ValidateCredentialsAsync(username, password).ConfigureAwait(false);
            if (user == null)
            {
                RecordFailedAttempt(username);
                var remainingAttempts = MaxFailedAttempts - GetFailedAttempts(username);
                var invalidCredsMessage = App.GetLocalizedString("AuthInvalidCredentials");
                var attemptsMessage = App.GetLocalizedString("AuthAttemptsRemaining", remainingAttempts);
                return AuthenticationResult.Failure(
                    $"{invalidCredsMessage} {attemptsMessage}",
                    AuthenticationError.InvalidCredentials);
            }

            // Successful authentication
            await _userRepository.UpdateLastLoginAsync(username).ConfigureAwait(false);
            user.LastLogin = DateTime.Now;
            ResetFailedAttempts(username);
            return AuthenticationResult.Success(user);
        }
        catch (Exception ex)
        {
            // Log the exception here if you have logging
            return AuthenticationResult.Failure(
                App.GetLocalizedString("AuthDatabaseError"),
                AuthenticationError.DatabaseError);
        }
    }

    public async Task<bool> CreateUserAsync(User user, string password)
    {
        if (user == null || string.IsNullOrWhiteSpace(password))
            return false;

        return await _userRepository.CreateUserAsync(user, password).ConfigureAwait(false);
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        if (user == null)
            return false;

        return await _userRepository.UpdateUserAsync(user).ConfigureAwait(false);
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllUsersAsync().ConfigureAwait(false);
    }

    public async Task<bool> ResetFailedAttemptsAsync(string username)
    {
        ResetFailedAttempts(username);
        return await Task.FromResult(true);
    }

    public async Task<int> GetFailedAttemptsAsync(string username)
    {
        return await Task.FromResult(GetFailedAttempts(username));
    }

    private void RecordFailedAttempt(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return;

        username = username.ToLower();
        _failedAttempts[username] = GetFailedAttempts(username) + 1;
        _lastAttempt[username] = DateTime.Now;
    }

    private void ResetFailedAttempts(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return;

        username = username.ToLower();
        _failedAttempts.Remove(username);
        _lastAttempt.Remove(username);
    }

    private int GetFailedAttempts(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return 0;

        username = username.ToLower();
        return _failedAttempts.ContainsKey(username) ? _failedAttempts[username] : 0;
    }

    private bool IsAccountLocked(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return false;

        username = username.ToLower();
        var attempts = GetFailedAttempts(username);

        if (attempts < MaxFailedAttempts) return false;

        if (!_lastAttempt.ContainsKey(username)) return false;

        var timeSinceLastAttempt = DateTime.Now - _lastAttempt[username];
        if (timeSinceLastAttempt.TotalMinutes >= LockoutMinutes)
        {
            ResetFailedAttempts(username);
            return false;
        }

        return true;
    }

    private int GetLockoutTimeRemaining(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return 0;

        username = username.ToLower();
        if (!_lastAttempt.ContainsKey(username)) return 0;

        var timeSinceLastAttempt = DateTime.Now - _lastAttempt[username];
        var remainingMinutes = LockoutMinutes - (int)timeSinceLastAttempt.TotalMinutes;
        return Math.Max(0, remainingMinutes);
    }
}
