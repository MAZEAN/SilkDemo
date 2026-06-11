namespace Centauri.Config;

using System.Text.Json;
using Utils.Misc;

public static class ConfigLoader
{
    public static AppConfig Load(string path)
    {
        var fullPath = PathResolver.Resolve(path);

        if (!File.Exists(fullPath))
        {
            Console.WriteLine("Config not found. Creating default config.");
            var defaultConfig = new AppConfig();
            Save(defaultConfig, fullPath); // fullPath already absolute — Resolve is a no-op
            return defaultConfig;
        }

        try
        {
            var json   = File.ReadAllText(fullPath);
            var config = JsonSerializer.Deserialize<AppConfig>(json, JsonDefaults.Options);

            if (config == null)
                throw new Exception("Deserialized config is null");

            return config;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public static void Save(AppConfig config, string path)
    {
        var fullPath = PathResolver.Resolve(path);
        var json     = JsonSerializer.Serialize(config, JsonDefaults.Options);
        File.WriteAllText(fullPath, json);
        Console.WriteLine($"✅ Config saved to: {fullPath}");
    }
}