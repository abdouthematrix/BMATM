

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
        // Register Data
        services.AddSingleton<DatabaseContext, DatabaseContext>();        
        services.AddSingleton<IUserRepository, UserRepository>();
        services.AddSingleton<ISupervisorProfileRepository, SupervisorProfileRepository>();
        services.AddSingleton<IATMRepository, ATMRepository>();
        // ... more services

        // Register Services
        services.AddSingleton<IAuthenticationService, SQLiteAuthenticationService>();
        services.AddSingleton<IUserDataService, SQLiteUserDataService>();
        services.AddSingleton<IATMDataService, SQLiteATMDataService>();
        // ... more services

        // Register ViewModels        
        services.AddTransient<LoginViewModel>();
        services.AddTransient<SupervisorProfileViewModel>();
        services.AddTransient<AddATMViewModel>();

        services.AddSingleton<MainWindowViewModel>();
        // ... more VMs

        // Register Views        
        services.AddTransient<LoginView>();
        services.AddTransient<SupervisorProfileView>();
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
        for (int i = 0; i < Current.Resources.MergedDictionaries.Count; i++)
        {
            var md = Current.Resources.MergedDictionaries[i];
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
            Current.Resources.MergedDictionaries.Add(newLanguage);
        }
        else
        {
            // Replace the current langage dictionary with the new one
            Current.Resources.MergedDictionaries[langDictId] = newLanguage;
        }

        // Set FlowDirection if present
        if (newLanguage["FlowDirection"] is FlowDirection flowDirection)
        {
            if (Current.MainWindow != null)
                Current.MainWindow.FlowDirection = flowDirection;
        }

        Settings.Default.Language = languageCode;
        Settings.Default.Save();
    }
}