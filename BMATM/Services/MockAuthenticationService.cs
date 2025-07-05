namespace BMATM.Services;

public class MockAuthenticationService : IAuthenticationService
{
    private User? _currentUser;
    private readonly List<User> _mockUsers;

    public bool IsAuthenticated => _currentUser != null;

    public MockAuthenticationService()
    {
        _mockUsers = new List<User>
        {
            new User
            {
                Username = "abdelrahmanhas",
                DisplayName = "Abdelrahman Hassan",
                Email = "abdelrahmanhas@banquemisr.com",
                Role = UserRole.Supervisor,
                Department = "Branchs",
                BranchCode = "707",
                BranchName = "Sheraton Cairo Branch"
            },
            new User
            {
                Username = "admin",
                DisplayName = "Administrator",
                Email = "admin@bank.com",
                Role = UserRole.Administrator,
                Department = "IT",
                BranchCode = "001",
                BranchName = "Main Branch"
            },
            new User
            {
                Username = "supervisor",
                DisplayName = "Branch Supervisor",
                Email = "supervisor@bank.com",
                Role = UserRole.Supervisor,
                Department = "Operations",
                BranchCode = "002",
                BranchName = "Downtown Branch"
            },
            new User
            {
                Username = "operator",
                DisplayName = "ATM Operator",
                Email = "operator@bank.com",
                Role = UserRole.Operator,
                Department = "Operations",
                BranchCode = "003",
                BranchName = "Mall Branch"
            }
        };
    }

    public async Task<AuthenticationResult> AuthenticateAsync(string username, string password)
    {
        await Task.Delay(1000); // Simulate network delay

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            return new AuthenticationResult
            {
                IsSuccess = false,
                ErrorMessage = "Username and password are required"
            };
        }

        // Simple mock authentication - in real app, this would validate against AD/database
        var user = _mockUsers.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        if (user != null && (password == "password" || password == "123")) // Mock password check
        {
            _currentUser = user;
            _currentUser.LastLogin = DateTime.Now;

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
}