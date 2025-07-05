namespace BMATM;
public partial class App : Application
{
    private static bool _isLanguageMetadataSet = false;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Set default culture
        SetLanguage("en");
    }

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
        var dict = new ResourceDictionary();
        dict.Source = new Uri($"Resources/Strings.{languageCode}.xaml", UriKind.Relative);

        Application.Current.Resources.MergedDictionaries.Clear();
        Application.Current.Resources.MergedDictionaries.Add(dict);
        Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("Resources/Styles.xaml", UriKind.Relative) });
    }
}