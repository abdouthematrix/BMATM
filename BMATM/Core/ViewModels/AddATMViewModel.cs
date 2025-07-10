namespace BMATM.ViewModels;

public class AddATMViewModel : ViewModelBase
{
    private readonly IATMDataService _atmDataService;
    private ATMInfo _atmInfo;
    private bool _isEditMode;
    private bool _isSaving;
    private string _originalATMNumber;

    // Form fields
    private string _atmNumber;
    private ATMType _selectedATMType;
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

    public AddATMViewModel(IATMDataService atmDataService)
    {
        _atmDataService = atmDataService;

        SaveCommand = new RelayCommand(async () => await ExecuteSaveAsync(), CanExecuteSave);
        CancelCommand = new RelayCommand(ExecuteCancel);

        InitializeDefaults();
    }

    public AddATMViewModel(IATMDataService atmDataService, ATMInfo atmToEdit) : this(atmDataService)
    {
        _isEditMode = true;
        _originalATMNumber = atmToEdit.ATMNumber;
        LoadATMData(atmToEdit);
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        set
        {
            _isEditMode = value;
            OnPropertyChanged();
        }
    }

    public bool IsSaving
    {
        get => _isSaving;
        set
        {
            _isSaving = value;
            OnPropertyChanged();
            ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }
    }

    public string ATMNumber
    {
        get => _atmNumber;
        set
        {
            _atmNumber = value;
            OnPropertyChanged();
            ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }
    }

    public ATMType SelectedATMType
    {
        get => _selectedATMType;
        set
        {
            _selectedATMType = value;
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
            ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }
    }

    public string BranchCode
    {
        get => _branchCode;
        set
        {
            _branchCode = value;
            OnPropertyChanged();
            ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }
    }

    public string BranchName
    {
        get => _branchName;
        set
        {
            _branchName = value;
            OnPropertyChanged();
            ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
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

    public List<ATMType> ATMTypes => Enum.GetValues(typeof(ATMType)).Cast<ATMType>().ToList();

    public List<int> CassetteDenominations => new List<int> { 200 , 100 ,50 ,20,10,5};

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    private void InitializeDefaults()
    {
        SelectedATMType = ATMType.NCR;
        Cassette1Denomination = 200;
        Cassette2Denomination = 200;
        Cassette3Denomination = 100;
        Cassette4Denomination = 20;
        IsActive = true;
        InstallationDate = DateTime.Now;
        LastMaintenanceDate = DateTime.Now;
    }

    private void LoadATMData(ATMInfo atm)
    {
        ATMNumber = atm.ATMNumber;
        SelectedATMType = atm.ATMType;
        GLAccount = atm.GLAccount;
        BranchCode = atm.BranchCode;
        BranchName = atm.BranchName;
        Location = atm.Location;
        Cassette1Denomination = atm.Cassette1Denomination;
        Cassette2Denomination = atm.Cassette2Denomination;
        Cassette3Denomination = atm.Cassette3Denomination;
        Cassette4Denomination = atm.Cassette4Denomination;
        Cassette1Balance = atm.Cassette1Balance;
        Cassette2Balance = atm.Cassette2Balance;
        Cassette3Balance = atm.Cassette3Balance;
        Cassette4Balance = atm.Cassette4Balance;
        IPAddress = atm.IPAddress;
        IsActive = atm.IsActive;
        InstallationDate = atm.InstallationDate;
        LastMaintenanceDate = atm.LastMaintenanceDate;
        Notes = atm.Notes;
    }

    private async Task ExecuteSaveAsync()
    {
        if (!CanExecuteSave())
            return;

        IsSaving = true;
        try
        {
            var atm = new ATMInfo
            {
                ATMNumber = ATMNumber.Trim(),
                ATMType = SelectedATMType,
                GLAccount = GLAccount?.Trim(),
                BranchCode = BranchCode?.Trim(),
                BranchName = BranchName?.Trim(),
                Location = Location?.Trim(),
                Cassette1Denomination = Cassette1Denomination,
                Cassette2Denomination = Cassette2Denomination,
                Cassette3Denomination = Cassette3Denomination,
                Cassette4Denomination = Cassette4Denomination,
                Cassette1Balance = Cassette1Balance,
                Cassette2Balance = Cassette2Balance,
                Cassette3Balance = Cassette3Balance,
                Cassette4Balance = Cassette4Balance,
                IPAddress = IPAddress?.Trim(),
                IsActive = IsActive,
                InstallationDate = InstallationDate,
                LastMaintenanceDate = LastMaintenanceDate,
                Notes = Notes?.Trim()
            };

            bool success;
            if (IsEditMode)
            {
                success = await _atmDataService.UpdateATMAsync(atm);
            }
            else
            {
                // Check if ATM number already exists
                if (await _atmDataService.ATMExistsAsync(atm.ATMNumber))
                {
                    System.Windows.MessageBox.Show(
                        "ATM number already exists. Please use a different number.",
                        "Duplicate ATM Number",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Warning);
                    return;
                }

                success = await _atmDataService.AddATMAsync(atm);
            }

            if (success)
            {
                NavigationHelper.NavigateTo<SupervisorProfileView>();
            }
            else
            {
                System.Windows.MessageBox.Show(
                    "Failed to save ATM. Please try again.",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }
        finally
        {
            IsSaving = false;
        }
    }

    private bool CanExecuteSave()
    {
        return !IsSaving &&
               !string.IsNullOrWhiteSpace(ATMNumber) &&
               !string.IsNullOrWhiteSpace(GLAccount) &&
               !string.IsNullOrWhiteSpace(BranchCode) &&
               !string.IsNullOrWhiteSpace(BranchName);
    }

    private void ExecuteCancel()
    {
        NavigationHelper.NavigateTo<SupervisorProfileView>();
    }
}