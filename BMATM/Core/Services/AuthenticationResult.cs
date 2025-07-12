namespace BMATM.Services;
public class AuthenticationResult
{
    public bool IsSuccess { get; set; }
    public User User { get; set; }
    public string ErrorMessage { get; set; }
    public AuthenticationError ErrorType { get; set; }

    public static AuthenticationResult Success(User user)
    {
        return new AuthenticationResult
        {
            IsSuccess = true,
            User = user,
            ErrorMessage = null,
            ErrorType = AuthenticationError.None
        };
    }

    public static AuthenticationResult Failure(string message, AuthenticationError errorType)
    {
        return new AuthenticationResult
        {
            IsSuccess = false,
            User = null,
            ErrorMessage = message,
            ErrorType = errorType
        };
    }
}

public enum AuthenticationError
{
    None,
    InvalidCredentials,
    UserNotFound,
    UserInactive,
    EmptyUsername,
    EmptyPassword,
    DatabaseError,
    TooManyAttempts
}
