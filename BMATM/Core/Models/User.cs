namespace BMATM.Models;
public class User : ViewModelBase
{
    private string _username = string.Empty;
    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    private string _displayName = string.Empty;
    public string DisplayName
    {
        get => _displayName;
        set => SetProperty(ref _displayName, value);
    }

    private string _email = string.Empty;
    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    private string _department = string.Empty;
    public string Department
    {
        get => _department;
        set => SetProperty(ref _department, value);
    }

    private string _branchNumber = string.Empty;
    private string _branchName = string.Empty;
    public string BranchNumber
    {
        get => _branchNumber;
        set => SetProperty(ref _branchNumber, value);
    }
    public string BranchName
    {
        get => _branchName;
        set => SetProperty(ref _branchName, value);
    }
    public UserRole Role { get; set; }

    private DateTime _lastLogin;    
    public DateTime LastLogin
    {
        get => _lastLogin;
        set => SetProperty(ref _lastLogin, value);
    }

    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }
}
public enum UserRole
{
    Administrator,
    Supervisor,
    Operator,
    ReadOnly
}