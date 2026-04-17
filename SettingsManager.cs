using System.Globalization;
using System.IO;
using System.Text.Json;

namespace OpenKNX.Toolbox;

public static class SettingsManager
{
    private static readonly string _settingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "OpenKNX", "Toolbox", "settings.json");

    private static Dictionary<string, string> _settings = new();

    public static void Load()
    {
        if (!File.Exists(_settingsPath))
            return;

        try
        {
            string json = File.ReadAllText(_settingsPath);
            _settings = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
        }
        catch
        {
            _settings = new();
        }
    }

    public static void Save()
    {
        try
        {
            string? dir = Path.GetDirectoryName(_settingsPath);
            if (dir != null && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
        }
        catch
        {
            // Silently fail if settings can't be saved
        }
    }

    public static string Get(string key, string defaultValue = "")
    {
        return _settings.TryGetValue(key, out string? value) ? value : defaultValue;
    }

    public static void Set(string key, string value)
    {
        _settings[key] = value;
        Save();
    }

    /// <summary>
    /// Discovers supported languages by scanning for satellite resource assemblies.
    /// English is always included as the neutral/default culture.
    /// </summary>
    public static List<string> GetSupportedLanguages()
    {
        var languages = new List<string> { "en" };
        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        string assemblyName = typeof(SettingsManager).Assembly.GetName().Name!;

        foreach (var dir in Directory.GetDirectories(basePath))
        {
            string dirName = Path.GetFileName(dir);
            string satellitePath = Path.Combine(dir, $"{assemblyName}.resources.dll");
            if (!File.Exists(satellitePath)) continue;

            try
            {
                var culture = CultureInfo.GetCultureInfo(dirName);
                languages.Add(culture.TwoLetterISOLanguageName);
            }
            catch (CultureNotFoundException) { }
        }

        return languages.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }
}
