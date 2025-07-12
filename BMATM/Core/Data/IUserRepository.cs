namespace BMATM.Data;

// ==================== USER REPOSITORY ====================
public interface IUserRepository
{
    Task InitializeAsync();
    Task<User> GetByUsernameAsync(string username);
    Task<User> ValidateCredentialsAsync(string username, string password);
    Task<bool> CreateUserAsync(User user, string password);
    Task<bool> UpdateUserAsync(User user);
    Task<bool> UpdateLastLoginAsync(string username);
    Task<List<User>> GetAllUsersAsync();
}