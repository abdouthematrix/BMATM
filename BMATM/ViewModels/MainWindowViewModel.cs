namespace BMATM.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private string _currentLanguage = "en";

    public string CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            _currentLanguage = value;
            OnPropertyChanged();
        }
    }

    public ICommand SwitchLanguageCommand { get; }

    public MainWindowViewModel()
    {
        SwitchLanguageCommand = new RelayCommand<string>(SwitchLanguage);
    }

    private void SwitchLanguage(string languageCode)
    {
        if (string.IsNullOrEmpty(languageCode) || languageCode == CurrentLanguage)
            return;

        CurrentLanguage = languageCode;
        App.SetLanguage(languageCode);
    }
}