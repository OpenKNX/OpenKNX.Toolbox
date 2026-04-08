using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenKNX.Toolbox.Localization;

public class Localizer
{
    public static Localizer Instance { get; } = new();

    private const string SettingsKey = "language";
    private const string ResourceDictionaryMarker = "_IsLocalizationDictionary";

    public event Action? LanguageChanged;

    private readonly LanguageBase[] _languages = { new LanguageDe(), new LanguageEn() };
    public string[] SupportedLanguages { get; }
    public Dictionary<string, string> LanguageNames { get; }

    private LanguageBase _current;
    public LanguageBase Strings => _current;

    public string Language
    {
        get => _current.Code;
        set
        {
            if (_current.Code == value) return;
            _current = _languages.First(l => l.Code == value);
            ApplyLanguage();
            SettingsManager.Instance.Set(SettingsKey, _current.Code);
            LanguageChanged?.Invoke();
        }
    }

    private Localizer()
    {
        SupportedLanguages = _languages.Select(l => l.Code).ToArray();
        LanguageNames = _languages.ToDictionary(l => l.Code, l => l.DisplayName);
        _current = _languages[0];

        string savedLang = SettingsManager.Instance.Get(SettingsKey);
        var match = _languages.FirstOrDefault(l => l.Code == savedLang);
        if (match != null)
            _current = match;
    }

    public void Initialize()
    {
        ApplyLanguage();
    }

    private void ApplyLanguage()
    {
        if (Application.Current == null) return;

        var toRemove = new List<IResourceProvider>();
        foreach (var dict in Application.Current.Resources.MergedDictionaries)
        {
            if (dict is ResourceDictionary rd && rd.ContainsKey(ResourceDictionaryMarker))
                toRemove.Add(dict);
        }
        foreach (var dict in toRemove)
            Application.Current.Resources.MergedDictionaries.Remove(dict);

        var langDict = new ResourceDictionary();
        langDict[ResourceDictionaryMarker] = true;

        foreach (var kvp in _current.ToDictionary())
            langDict[kvp.Key] = kvp.Value;

        Application.Current.Resources.MergedDictionaries.Add(langDict);
    }
}
