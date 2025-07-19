using BMATM.Views;
using System.Windows.Threading;

namespace BMATM.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private string _currentLanguage;
    private UserControl? _currentView;

    public string CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            _currentLanguage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CurrentLanguageLabel));
        }
    }

    public string CurrentLanguageLabel => "🌐 " +
     (CurrentLanguage == "en"
         ? "العربية"
         : "English");
   
    public UserControl? CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }
    public ICommand SwitchLanguageCommand { get; }

    public MainWindowViewModel()
    {
        CurrentLanguage = Settings.Default.Language;
        App.SetLanguage(CurrentLanguage);       
        SwitchLanguageCommand = new RelayCommand(SwitchLanguage);
        // Subscribe to navigation events
        NavigationHelper.NavigationRequested += OnNavigationRequested;
        // Start with login view
        NavigationHelper.NavigateTo<LoginView>();
    }

    private void OnNavigationRequested(UserControl view, object? parameter)
    {
        CurrentView = view;       
    }

    private void SwitchLanguage()
    {
        var newLanguage = CurrentLanguage switch
        {
            "en" => "ar",
            "ar" => "en",
            _ => "en"
        };

        CurrentLanguage = newLanguage;        
        App.SetLanguage(newLanguage);
        if (CurrentView.DataContext is GLReconciliationViewModel model)
        {
            model.Refresh();
        }
    }
}