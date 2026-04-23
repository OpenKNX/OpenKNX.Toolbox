using System.Globalization;
using System.Windows;

namespace OpenKNX.Toolbox;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        SettingsManager.Load();

        var supported = SettingsManager.GetSupportedLanguages();
        string systemLang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        string fallback = supported.Contains(systemLang, StringComparer.OrdinalIgnoreCase)
            ? systemLang : "en";

        string language = SettingsManager.Get("language", fallback);
        var culture = new CultureInfo(language);
        Thread.CurrentThread.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        base.OnStartup(e);
    }
}

