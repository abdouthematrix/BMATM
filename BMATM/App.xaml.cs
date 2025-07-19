using Microsoft.Extensions.DependencyInjection;
using System.Windows.Input;
using System.Windows.Threading;

namespace BMATM;
public partial class App : Application
{
    public static IHost AppHost { get; private set; }
    private async void Application_Startup(object sender, StartupEventArgs e)
    {
        AppHost = Host.CreateDefaultBuilder()
           .ConfigureServices((context, services) =>
           {
               ConfigureServices(services);
           })
           .Build();
        AppHost.Start();

        // Resolve and show MainWindow via DI
        var manager = AppHost.Services.GetRequiredService<DatabaseInitializationManager>();
        await manager.InitializeAllAsync();

        // Resolve and show MainWindow via DI
        var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
    private async void ConfigureServices(IServiceCollection services)
    {
        services.AddBMATMDataServices("Data Source=bmatm.db;");

        // Register ViewModels        
        services.AddTransient<LoginViewModel>();
        services.AddTransient<SupervisorProfileViewModel>();
        services.AddTransient<ATMViewModel>();
        services.AddTransient<GLReconciliationViewModel>();

        services.AddSingleton<MainWindowViewModel>();
        // ... more VMs

        // Register Views        
        services.AddTransient<LoginView>();
        services.AddTransient<SupervisorProfileView>();
        services.AddTransient<ATMView>();
        services.AddTransient<GLReconciliationView>();

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
    public static string GetLocalizedString(string key, params object[] args)
    {
        try
        {
            var resourceDict = Application.Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source?.OriginalString?.Contains($"Strings.{Settings.Default.Language}") == true);

            if (resourceDict != null && resourceDict.Contains(key))
            {
                var format = resourceDict[key].ToString();
                return args?.Length > 0 ? string.Format(format, args) : format;
            }

            // Fallback to English if current language resource not found
            var fallbackDict = Application.Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source?.OriginalString?.Contains("Strings.en") == true);

            if (fallbackDict != null && fallbackDict.Contains(key))
            {
                var format = fallbackDict[key].ToString();
                return args?.Length > 0 ? string.Format(format, args) : format;
            }

            return key; // Return key as fallback
        }
        catch
        {
            return key; // Return key as fallback if any error occurs
        }
    }
}
    