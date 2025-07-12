namespace BMATM.Services;

public interface IUserService
{
    Task InitializeAsync();
    Task<AuthenticationResult> AuthenticateAsync(string username, string password);
    Task<bool> CreateUserAsync(User user, string password);
    Task<bool> UpdateUserAsync(User user);
    Task<List<User>> GetAllUsersAsync();
    Task<bool> ResetFailedAttemptsAsync(string username);
    Task<int> GetFailedAttemptsAsync(string username);
}