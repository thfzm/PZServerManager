using PZServerManager.Models;

namespace PZServerManager.Services;

public static class ModInfoParser
{
    /// Reads &lt;mod_folder&gt;/mod.info into a LocalMod (caller fills in WorkshopId).
    /// Returns null if the folder isn't a real mod (no mod.info or no `id=`).
    public static LocalMod? ReadMod(string modFolder)
    {
        var infoPath = Path.Combine(modFolder, "mod.info");
        if (!File.Exists(infoPath)) return null;

        var fields = ReadKvFile(infoPath);
        if (!fields.TryGetValue("id", out var modId) || string.IsNullOrWhiteSpace(modId))
            return null;

        var mod = new LocalMod
        {
            ModId = modId.Trim(),
            ModFolder = modFolder,
            DisplayName = fields.GetValueOrDefault("name", modId).Trim(),
            Description = fields.GetValueOrDefault("description", "").Trim(),
        };

        if (fields.TryGetValue("require", out var req) && !string.IsNullOrWhiteSpace(req))
        {
            mod.Requires = req
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
        }

        if (fields.TryGetValue("poster", out var poster) && !string.IsNullOrWhiteSpace(poster))
        {
            var p = Path.Combine(modFolder, poster.Trim());
            if (File.Exists(p)) mod.PosterPath = p;
        }

        // Map detection: <mod>/media/maps/<mapname>/map.info
        var mapsDir = Path.Combine(modFolder, "media", "maps");
        if (Directory.Exists(mapsDir))
        {
            foreach (var sub in Directory.EnumerateDirectories(mapsDir))
            {
                if (File.Exists(Path.Combine(sub, "map.info")))
                    mod.MapNames.Add(Path.GetFileName(sub)!);
            }
        }

        return mod;
    }

    private static Dictionary<string, string> ReadKvFile(string path)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var raw in File.ReadAllLines(path))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith("#")) continue;
            var eq = line.IndexOf('=');
            if (eq <= 0) continue;
            result[line[..eq].Trim()] = line[(eq + 1)..];
        }
        return result;
    }
}
