using BMATM.Views;

namespace BMATM.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private string _currentLanguage = "en";
    private UserControl? _currentView;

    public string CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            _currentLanguage = value;
            OnPropertyChanged();
        }
    }

    public UserControl? CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    public ICommand SwitchLanguageCommand { get; }

    public MainWindowViewModel()
    {
        SwitchLanguageCommand = new RelayCommand<string>(SwitchLanguage);

        // Subscribe to navigation events
        NavigationHelper.NavigationRequested += OnNavigationRequested;

        // Start with login view
        NavigationHelper.NavigateTo<LoginView>();
    }

    private void OnNavigationRequested(UserControl view)
    {
        CurrentView = view;
    }

    private void SwitchLanguage(string languageCode)
    {
        if (string.IsNullOrEmpty(languageCode) || languageCode == CurrentLanguage)
            return;

        CurrentLanguage = languageCode;
        App.SetLanguage(languageCode);
    }
}