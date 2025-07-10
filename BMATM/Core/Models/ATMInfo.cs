namespace BMATM.Models;
public class ATMInfo : INotifyPropertyChanged
{
    private string _atmNumber;
    private ATMType _atmType;
    private string _glAccount;
    private string _branchCode;
    private string _branchName;
    private string _location;
    private int _cassette1Denomination;
    private int _cassette2Denomination;
    private int _cassette3Denomination;
    private int _cassette4Denomination;
    private decimal _cassette1Balance;
    private decimal _cassette2Balance;
    private decimal _cassette3Balance;
    private decimal _cassette4Balance;
    private string _ipAddress;
    private bool _isActive;
    private DateTime _installationDate;
    private DateTime _lastMaintenanceDate;
    private string _notes;

    public string ATMNumber
    {
        get => _atmNumber;
        set
        {
            _atmNumber = value;
            OnPropertyChanged();
        }
    }

    public ATMType ATMType
    {
        get => _atmType;
        set
        {
            _atmType = value;
            OnPropertyChanged();
        }
    }

    public string GLAccount
    {
        get => _glAccount;
        set
        {
            _glAccount = value;
            OnPropertyChanged();
        }
    }

    public string BranchCode
    {
        get => _branchCode;
        set
        {
            _branchCode = value;
            OnPropertyChanged();
        }
    }

    public string BranchName
    {
        get => _branchName;
        set
        {
            _branchName = value;
            OnPropertyChanged();
        }
    }

    public string Location
    {
        get => _location;
        set
        {
            _location = value;
            OnPropertyChanged();
        }
    }

    public int Cassette1Denomination
    {
        get => _cassette1Denomination;
        set
        {
            _cassette1Denomination = value;
            OnPropertyChanged();
        }
    }

    public int Cassette2Denomination
    {
        get => _cassette2Denomination;
        set
        {
            _cassette2Denomination = value;
            OnPropertyChanged();
        }
    }

    public int Cassette3Denomination
    {
        get => _cassette3Denomination;
        set
        {
            _cassette3Denomination = value;
            OnPropertyChanged();
        }
    }

    public int Cassette4Denomination
    {
        get => _cassette4Denomination;
        set
        {
            _cassette4Denomination = value;
            OnPropertyChanged();
        }
    }

    public decimal Cassette1Balance
    {
        get => _cassette1Balance;
        set
        {
            _cassette1Balance = value;
            OnPropertyChanged();
        }
    }

    public decimal Cassette2Balance
    {
        get => _cassette2Balance;
        set
        {
            _cassette2Balance = value;
            OnPropertyChanged();
        }
    }

    public decimal Cassette3Balance
    {
        get => _cassette3Balance;
        set
        {
            _cassette3Balance = value;
            OnPropertyChanged();
        }
    }

    public decimal Cassette4Balance
    {
        get => _cassette4Balance;
        set
        {
            _cassette4Balance = value;
            OnPropertyChanged();
        }
    }

    public string IPAddress
    {
        get => _ipAddress;
        set
        {
            _ipAddress = value;
            OnPropertyChanged();
        }
    }

    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            OnPropertyChanged();
        }
    }

    public DateTime InstallationDate
    {
        get => _installationDate;
        set
        {
            _installationDate = value;
            OnPropertyChanged();
        }
    }

    public DateTime LastMaintenanceDate
    {
        get => _lastMaintenanceDate;
        set
        {
            _lastMaintenanceDate = value;
            OnPropertyChanged();
        }
    }

    public string Notes
    {
        get => _notes;
        set
        {
            _notes = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}