namespace BMATM.Models;
public class ATMInfo : ViewModelBase
{
    public string Username { get; set; }
    private string _atmNumber;
    private ATMType _atmType;
    private string _glAccount;
    private string _branchCode;
    private string _branchName;
    private int _cassette1Denomination;
    private int _cassette2Denomination;
    private int _cassette3Denomination;
    private int _cassette4Denomination;      
    private bool _isActive;
    public string ATMNumber
    {
        get => _atmNumber;
        set => SetProperty(ref _atmNumber,value);        
    }

    public ATMType ATMType
    {
        get => _atmType;
        set => SetProperty(ref _atmType, value);
    }

    public string GLAccount
    {
        get => _glAccount;
        set => SetProperty(ref _glAccount, value);
    }

    public string BranchCode
    {
        get => _branchCode;
        set => SetProperty(ref _branchCode, value);
    }

    public string BranchName
    {
        get => _branchName;
        set => SetProperty(ref _branchName, value);
    }
    public int Cassette1Denomination
    {
        get => _cassette1Denomination;
        set => SetProperty(ref _cassette1Denomination, value);
    }

    public int Cassette2Denomination
    {
        get => _cassette2Denomination;
        set => SetProperty(ref _cassette2Denomination, value);
    }

    public int Cassette3Denomination
    {
        get => _cassette3Denomination;
        set => SetProperty(ref _cassette3Denomination, value);
    }

    public int Cassette4Denomination
    {
        get => _cassette4Denomination;
        set => SetProperty(ref _cassette4Denomination, value);
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
}