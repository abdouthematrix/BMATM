namespace BMATM.ViewModels;
public class ATMCollectionViewModel : ViewModelBase
{
    private readonly IATMDataService _atmDataService;
    private ObservableCollection<ATMInfo> _atms;
    private ATMInfo _selectedATM;
    private bool _isLoading;

    public ATMCollectionViewModel(IATMDataService atmDataService)
    {
        _atmDataService = atmDataService;
        _atms = new ObservableCollection<ATMInfo>();

        AddATMCommand = new RelayCommand(ExecuteAddATM);
        EditATMCommand = new RelayCommand<ATMInfo>(ExecuteEditATM, CanExecuteEditATM);
        DeleteATMCommand = new RelayCommand<ATMInfo>(ExecuteDeleteATM, CanExecuteDeleteATM);
        RefreshCommand = new RelayCommand(async () => await LoadATMsAsync());

        _ = LoadATMsAsync();
    }

    public ObservableCollection<ATMInfo> ATMs
    {
        get => _atms;
        set
        {
            _atms = value;
            OnPropertyChanged();
        }
    }

    public ATMInfo SelectedATM
    {
        get => _selectedATM;
        set
        {
            _selectedATM = value;
            OnPropertyChanged();
            ((RelayCommand<ATMInfo>)EditATMCommand).RaiseCanExecuteChanged();
            ((RelayCommand<ATMInfo>)DeleteATMCommand).RaiseCanExecuteChanged();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public ICommand AddATMCommand { get; }
    public ICommand EditATMCommand { get; }
    public ICommand DeleteATMCommand { get; }
    public ICommand RefreshCommand { get; }

    private async Task LoadATMsAsync()
    {
        IsLoading = true;
        try
        {
            var atms = await _atmDataService.GetAllATMsAsync();
            ATMs.Clear();
            foreach (var atm in atms)
            {
                ATMs.Add(atm);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ExecuteAddATM()
    {
        NavigationHelper.NavigateTo<AddATMView>();
    }

    private void ExecuteEditATM(ATMInfo atm)
    {
        if (atm != null)
        {
            NavigationHelper.NavigateTo<AddATMView>();
        }
    }

    private bool CanExecuteEditATM(ATMInfo atm)
    {
        return atm != null;
    }

    private async void ExecuteDeleteATM(ATMInfo atm)
    {
        if (atm != null)
        {
            var result = System.Windows.MessageBox.Show(
                $"Are you sure you want to delete ATM {atm.ATMNumber}?",
                "Confirm Delete",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                var success = await _atmDataService.DeleteATMAsync(atm.ATMNumber);
                if (success)
                {
                    ATMs.Remove(atm);
                    if (SelectedATM == atm)
                    {
                        SelectedATM = null;
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show(
                        "Failed to delete ATM. Please try again.",
                        "Error",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);
                }
            }
        }
    }

    private bool CanExecuteDeleteATM(ATMInfo atm)
    {
        return atm != null;
    }

    public async Task RefreshATMsAsync()
    {
        await LoadATMsAsync();
    }
}