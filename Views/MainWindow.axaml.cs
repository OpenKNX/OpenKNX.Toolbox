using System.Linq;
using Avalonia.Controls;
using OpenKNX.Toolbox.Localization;

namespace OpenKNX.Toolbox.Views;

public partial class MainWindow : Window
{
    public static MainWindow? Instance = null; 
    public MainWindow()
    {
        InitializeComponent();
        Instance = this;
        var assembly = System.Reflection.Assembly.GetEntryAssembly();
        if(assembly == null) return;
        var vers = assembly.GetName().Version;
        if(vers == null) return;
        this.Title += " - v" + string.Join('.', vers.ToString().Split('.').Take(3));

        var langBox = this.FindControl<ComboBox>("LanguageComboBox");
        if (langBox != null)
        {
            var langNames = Localizer.Instance.LanguageNames;
            langBox.ItemsSource = Localizer.Instance.SupportedLanguages
                .Select(l => langNames[l])
                .ToList();
            langBox.SelectedIndex = System.Array.IndexOf(Localizer.Instance.SupportedLanguages, Localizer.Instance.Language);
        }
    }

    private void LanguageComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox combo && combo.SelectedIndex >= 0 && combo.SelectedIndex < Localizer.Instance.SupportedLanguages.Length)
        {
            Localizer.Instance.Language = Localizer.Instance.SupportedLanguages[combo.SelectedIndex];
        }
    }
}