using System.Collections.Generic;
using System.IO;

namespace OpenKNX.Toolbox;

public class SettingsManager
{
    public static SettingsManager Instance { get; } = new();

    private static readonly string StoragePath = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
    private static readonly string SettingsFilePath = Path.Combine(StoragePath, "settings.json");

    private Dictionary<string, string> _settings = new();

    private SettingsManager()
    {
        Load();
    }

    public string Get(string key, string defaultValue = "")
    {
        return _settings.TryGetValue(key, out var value) ? value : defaultValue;
    }

    public void Set(string key, string value)
    {
        _settings[key] = value;
        Save();
    }

    private void Load()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                var loaded = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(
                    File.ReadAllText(SettingsFilePath));
                if (loaded != null)
                    _settings = loaded;
            }
        }
        catch { /* Use defaults */ }
    }

    private void Save()
    {
        try
        {
            if (!Directory.Exists(StoragePath))
                Directory.CreateDirectory(StoragePath);

            File.WriteAllText(SettingsFilePath,
                Newtonsoft.Json.JsonConvert.SerializeObject(_settings, Newtonsoft.Json.Formatting.Indented));
        }
        catch { /* Silently fail */ }
    }
}
