using System.Globalization;
using System.Windows.Controls;

namespace OpenKNX.Toolbox.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        private readonly Dictionary<string, string> _languages = new(StringComparer.OrdinalIgnoreCase);
        private bool _initialized;

        public SettingsPage()
        {
            InitializeComponent();

            foreach (string code in SettingsManager.GetSupportedLanguages())
            {
                var culture = new CultureInfo(code);
                _languages[code] = culture.NativeName;
            }

            foreach (var lang in _languages)
                LanguageComboBox.Items.Add(lang.Value);

            string currentLang = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
            if (_languages.TryGetValue(currentLang, out string? displayName))
                LanguageComboBox.SelectedItem = displayName;
            else
                LanguageComboBox.SelectedIndex = 0;

            _initialized = true;
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_initialized) return;

            string selectedDisplay = LanguageComboBox.SelectedItem?.ToString() ?? "";
            string code = _languages.FirstOrDefault(x => x.Value == selectedDisplay).Key ?? "en";

            string currentLang = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
            if (!string.Equals(code, currentLang, StringComparison.OrdinalIgnoreCase))
            {
                SettingsManager.Set("language", code);
                RestartInfoBar.IsOpen = true;
            }
        }
    }
}
