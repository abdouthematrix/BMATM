namespace BMATM.Services;

public interface IAuthenticationService
{
    Task<AuthenticationResult> AuthenticateAsync(string username, string password);
    Task<User?> GetCurrentUserAsync();
    Task LogoutAsync();
    bool IsAuthenticated { get; }
}

public class AuthenticationResult
{
    public bool IsSuccess { get; set; }
    public User? User { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}