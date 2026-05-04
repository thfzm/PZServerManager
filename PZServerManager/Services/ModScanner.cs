using PZServerManager.Models;

namespace PZServerManager.Services;

public sealed class ModScanner
{
    private readonly PzPaths _paths;
    public ModScanner(PzPaths paths) { _paths = paths; }

    /// Walks &lt;server&gt;/steamapps/workshop/content/108600/&lt;workshop_id&gt;/mods/&lt;mod_folder&gt;/mod.info
    /// One workshop item can contain multiple mods.
    public List<LocalMod> Scan()
    {
        var result = new List<LocalMod>();
        var root = _paths.WorkshopContentDir;
        if (!Directory.Exists(root)) return result;

        foreach (var workshopDir in Directory.EnumerateDirectories(root))
        {
            var workshopId = Path.GetFileName(workshopDir);
            if (!long.TryParse(workshopId, out var id)) continue;

            var modsDir = Path.Combine(workshopDir, "mods");
            if (!Directory.Exists(modsDir)) continue;

            foreach (var modDir in Directory.EnumerateDirectories(modsDir))
            {
                var mod = ModInfoParser.ReadMod(modDir);
                if (mod is null) continue;
                mod.WorkshopId = id;
                result.Add(mod);
            }
        }

        result.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase));
        return result;
    }
}
