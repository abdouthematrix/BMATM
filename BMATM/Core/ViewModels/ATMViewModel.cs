namespace BMATM.ViewModels;

public class ATMViewModel : ViewModelBase
{
    private readonly DatabaseInitializationManager database;    
    private bool _isEditMode;
    private bool _isSaving;   
    // Form fields
    private string _atmNumber;
    private ATMType _selectedATMType;
    private string _glAccount;
    private string _branchCode;
    private string _branchName;
    private int _cassette1Denomination;
    private int _cassette2Denomination;
    private int _cassette3Denomination;
    private int _cassette4Denomination; 
    private bool _isActive; 
    public ATMViewModel(DatabaseInitializationManager DatabaseManager)
    {
        database = DatabaseManager;

        SaveCommand = new RelayCommand(async () => await ExecuteSaveAsync(), CanExecuteSave);
        CancelCommand = new RelayCommand(ExecuteCancel);

        InitializeDefaults();
    }

    public void LoadATMData(ATMInfo atm)
    {
        ATMNumber = atm.ATMNumber;
        SelectedATMType = atm.ATMType;
        GLAccount = atm.GLAccount;
        BranchCode = atm.BranchCode;
        BranchName = atm.BranchName;
        Cassette1Denomination = atm.Cassette1Denomination;
        Cassette2Denomination = atm.Cassette2Denomination;
        Cassette3Denomination = atm.Cassette3Denomination;
        Cassette4Denomination = atm.Cassette4Denomination;      
        IsActive = atm.IsActive;        
    }

    public SupervisorProfile SupervisorProfile;

    public string SavingText => IsSaving ? "Saving..." : "Save";
    public string EditTitle => IsEditMode ? "Edit ATM" : "Add ATM";
    public bool IsEditMode
    {
        get => _isEditMode;
        set
        {
            if (SetProperty(ref _isEditMode, value))
                OnPropertyChanged(nameof(EditTitle));
        }
    }

    public bool IsSaving
    {
        get => _isSaving;
        set
        {
            if (SetProperty(ref _isSaving, value))
            {
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(EditTitle));
            }
        }
    }

    public string ATMNumber
    {
        get => _atmNumber;
        set
        {
            if (SetProperty(ref _atmNumber, value))
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }       
    }

    public ATMType SelectedATMType
    {
        get => _selectedATMType;
        set => SetProperty(ref _selectedATMType, value);
    }

    public string GLAccount
    {
        get => _glAccount;
        set
        {
            if (SetProperty(ref _glAccount, value))
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }        
    }

    public string BranchCode
    {
        get => _branchCode;
        set
        {
            if (SetProperty(ref _branchCode, value))
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }        
    }

    public string BranchName
    {
        get => _branchName;
        set
        {
            if (SetProperty(ref _branchName, value))
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }        
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
        set => SetProperty(ref _isActive, value);
    }
    public List<ATMType> ATMTypes => Enum.GetValues(typeof(ATMType)).Cast<ATMType>().ToList();
    public List<int> CassetteDenominations => new List<int> { 200 , 100 ,50 ,20, 10, 5};
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
                Cassette1Denomination = Cassette1Denomination,
                Cassette2Denomination = Cassette2Denomination,
                Cassette3Denomination = Cassette3Denomination,
                Cassette4Denomination = Cassette4Denomination,               
                IsActive = IsActive,
                Username = SupervisorProfile.User.Username,
            };

            bool success;
            if (IsEditMode)
            {
                success = await database._atmService.UpdateATMAsync(atm);
            }
            else
            {
                // Check if ATM number already exists
                if (await database._atmService.GetATMByNumberAsync(atm.ATMNumber) != null)
                {
                    System.Windows.MessageBox.Show(
                        "ATM number already exists. Please use a different number.",
                        "Duplicate ATM Number",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Warning);
                    return;
                }

                success = await database._atmService.CreateATMAsync(atm);
            }

            if (success)
            {
                // Navigate to supervisor profile or main dashboard
                NavigationHelper.NavigateTo<SupervisorProfileView, User>(SupervisorProfile.User);
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
        // Navigate to supervisor profile or main dashboard
        NavigationHelper.NavigateTo<SupervisorProfileView, User>(SupervisorProfile.User);
    }
}