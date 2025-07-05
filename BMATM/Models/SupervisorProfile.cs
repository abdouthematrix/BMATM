namespace BMATM.Models;

public class SupervisorProfile : INotifyPropertyChanged
{
    private string _fullName = string.Empty;
    private string _branchNumber = string.Empty;
    private string _branchName = string.Empty;
    private int _atmCollectionCount;
    private string _username = string.Empty;
    private string _employeeId = string.Empty;
    private DateTime _lastLoginDate;
    private bool _isActive;

    public string FullName
    {
        get => _fullName;
        set
        {
            _fullName = value;
            OnPropertyChanged(nameof(FullName));
        }
    }

    public string BranchNumber
    {
        get => _branchNumber;
        set
        {
            _branchNumber = value;
            OnPropertyChanged(nameof(BranchNumber));
        }
    }

    public string BranchName
    {
        get => _branchName;
        set
        {
            _branchName = value;
            OnPropertyChanged(nameof(BranchName));
        }
    }

    public int AtmCollectionCount
    {
        get => _atmCollectionCount;
        set
        {
            _atmCollectionCount = value;
            OnPropertyChanged(nameof(AtmCollectionCount));
        }
    }

    public string Username
    {
        get => _username;
        set
        {
            _username = value;
            OnPropertyChanged(nameof(Username));
        }
    }

    public string EmployeeId
    {
        get => _employeeId;
        set
        {
            _employeeId = value;
            OnPropertyChanged(nameof(EmployeeId));
        }
    }

    public DateTime LastLoginDate
    {
        get => _lastLoginDate;
        set
        {
            _lastLoginDate = value;
            OnPropertyChanged(nameof(LastLoginDate));
        }
    }

    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            OnPropertyChanged(nameof(IsActive));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}