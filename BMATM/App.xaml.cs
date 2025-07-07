

namespace BMATM;
public partial class App : Application
{
    public static IHost AppHost { get; private set; }
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        AppHost = Host.CreateDefaultBuilder()
           .ConfigureServices((context, services) =>
           {
               ConfigureServices(services);
           })
           .Build();
        AppHost.Start();
        // Resolve and show MainWindow via DI
        var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Register Services
        services.AddSingleton<IAuthenticationService, MockAuthenticationService>();
        services.AddSingleton<IUserDataService, LocalUserDataService>();
        services.AddSingleton<IATMDataService, LocalATMDataService>();
        // ... more services

        // Register ViewModels        
        services.AddTransient<LoginViewModel>();
        services.AddTransient<SupervisorProfileViewModel>();
        services.AddTransient<ATMCollectionViewModel>();
        services.AddTransient<AddATMViewModel>();

        services.AddSingleton<MainWindowViewModel>();
        // ... more VMs

        // Register Views        
        services.AddTransient<LoginView>();
        services.AddTransient<SupervisorProfileView>();
        services.AddTransient<ATMCollectionView>();
        services.AddTransient<AddATMView>();

        services.AddSingleton<MainWindow>();
        // ... more Views
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await AppHost.StopAsync();
        AppHost.Dispose();
        base.OnExit(e);
    }

    private static bool _isLanguageMetadataSet = false;

    public static void SetLanguage(string languageCode)
    {
        var culture = new CultureInfo(languageCode);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        // Set flow direction for Arabic only once
        if (languageCode == "ar" && !_isLanguageMetadataSet)
        {
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag)));
            _isLanguageMetadataSet = true;
        }

        // Update resource dictionary
        var newLanguage = new ResourceDictionary();
        newLanguage.Source = new Uri($"Resources/Strings.{languageCode}.xaml", UriKind.Relative);

        // Remove any previous Localization dictionaries loaded
        int langDictId = -1;
        for (int i = 0; i < Application.Current.Resources.MergedDictionaries.Count; i++)
        {
            var md = Application.Current.Resources.MergedDictionaries[i];
            // Make sure your Localization ResourceDictionarys have the ResourceDictionaryName
            // key and that it is set to a value starting with "Loc-".
            if (md.Contains("ResourceDictionaryName"))
            {
                if (md["ResourceDictionaryName"].ToString().StartsWith("Strings."))
                {
                    langDictId = i;
                    break;
                }
            }
        }
        if (langDictId == -1)
        {
            // Add in newly loaded Resource Dictionary
            Application.Current.Resources.MergedDictionaries.Add(newLanguage);
        }
        else
        {
            // Replace the current langage dictionary with the new one
            Application.Current.Resources.MergedDictionaries[langDictId] = newLanguage;
        }

        // Set FlowDirection if present
        if (newLanguage["FlowDirection"] is FlowDirection flowDirection)
        {
            if (Application.Current.MainWindow != null)
                Application.Current.MainWindow.FlowDirection = flowDirection;
        }

        Settings.Default.Language = languageCode;
        Settings.Default.Save();
    }
}