using System.Text.Json;
using PZServerManager.Models;

namespace PZServerManager.Services;

public static class AppConfigStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static AppConfig Load()
    {
        if (!File.Exists(AppPaths.ConfigFile)) return new AppConfig();
        try
        {
            var json = File.ReadAllText(AppPaths.ConfigFile);
            return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
        }
        catch
        {
            return new AppConfig();
        }
    }

    public static void Save(AppConfig config)
    {
        Directory.CreateDirectory(AppPaths.AppDataDir);
        File.WriteAllText(AppPaths.ConfigFile, JsonSerializer.Serialize(config, JsonOptions));
    }
}
