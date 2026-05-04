using PZServerManager.Models;

namespace PZServerManager.Services;

public static class ModSync
{
    public const string BaseMap = "Muldraugh, KY";

    public sealed class SyncResult
    {
        public List<string> Warnings { get; init; } = new();
        public List<LocalMod> OrderedEnabled { get; init; } = new();
    }

    /// Topological sort of `enabled` by mod.info `require=` (deps first), preserving the user-given
    /// relative order whenever the dependency graph allows it. Stable for ties — Up/Down moves
    /// stay where the user put them as long as they don't violate `require=`.
    public static SyncResult OrderByDependencies(IReadOnlyList<LocalMod> enabled)
    {
        var byModId = new Dictionary<string, LocalMod>(StringComparer.OrdinalIgnoreCase);
        foreach (var m in enabled) byModId[m.ModId] = m;

        var orderIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < enabled.Count; i++) orderIndex[enabled[i].ModId] = i;

        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var ordered = new List<LocalMod>();
        var warnings = new List<string>();

        void Visit(LocalMod m)
        {
            if (visited.Contains(m.ModId)) return;
            if (!visiting.Add(m.ModId))
            {
                warnings.Add($"Cycle detected involving '{m.DisplayName}'.");
                return;
            }
            // Visit dependencies in their declared order.
            foreach (var dep in m.Requires)
            {
                if (byModId.TryGetValue(dep, out var depMod))
                    Visit(depMod);
                else
                    warnings.Add($"'{m.DisplayName}' requires '{dep}' which is not enabled.");
            }
            visiting.Remove(m.ModId);
            visited.Add(m.ModId);
            ordered.Add(m);
        }

        // Visit in user-defined order so siblings preserve relative order.
        foreach (var m in enabled.OrderBy(x => orderIndex[x.ModId])) Visit(m);

        return new SyncResult { OrderedEnabled = ordered, Warnings = warnings };
    }

    /// Returns the canonical Map= value: enabled map mods' map names in order, deduplicated,
    /// with `Muldraugh, KY` always last as the base layer.
    public static string BuildMapValue(IReadOnlyList<LocalMod> orderedEnabled, string? existingValue)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<string>();

        // First, take map names from enabled map mods in load order.
        foreach (var m in orderedEnabled)
        {
            foreach (var name in m.MapNames)
            {
                if (string.Equals(name, BaseMap, StringComparison.OrdinalIgnoreCase)) continue;
                if (seen.Add(name)) result.Add(name);
            }
        }

        // Preserve any custom non-mod entries from the existing value (user may have added them
        // via Raw config tab) — but skip ones that come from currently-enabled mods (already added).
        var enabledMapNames = orderedEnabled.SelectMany(m => m.MapNames).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(existingValue))
        {
            foreach (var raw in existingValue.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                var name = raw.Trim();
                if (string.Equals(name, BaseMap, StringComparison.OrdinalIgnoreCase)) continue;
                if (enabledMapNames.Contains(name)) continue;
                if (seen.Add(name)) result.Add(name);
            }
        }

        result.Add(BaseMap);
        return string.Join(';', result);
    }

    public static string BuildModsValue(IReadOnlyList<LocalMod> orderedEnabled)
        => string.Join(';', orderedEnabled.Select(m => m.ModId).Distinct(StringComparer.OrdinalIgnoreCase));

    public static string BuildWorkshopItemsValue(IReadOnlyList<LocalMod> orderedEnabled)
        => string.Join(';', orderedEnabled
            .Select(m => m.WorkshopId.ToString())
            .Where(s => s != "0")
            .Distinct());

    /// Applies the three managed fields to the in-memory IniFile. Caller saves.
    public static void Apply(IniFile ini, IReadOnlyList<LocalMod> orderedEnabled)
    {
        ini.Set("Mods", BuildModsValue(orderedEnabled));
        ini.Set("WorkshopItems", BuildWorkshopItemsValue(orderedEnabled));
        ini.TryGet("Map", out var existingMap);
        ini.Set("Map", BuildMapValue(orderedEnabled, existingMap));
    }

    /// Reads the mod folder ids currently enabled in `Mods=` and returns LocalMods in that order.
    /// Mods listed in the ini but not present locally are dropped (and reported in warnings via the
    /// caller's UI).
    public static List<LocalMod> ReadEnabledFromIni(IniFile ini, IReadOnlyList<LocalMod> available)
    {
        var modIds = new List<string>();
        if (ini.TryGet("Mods", out var raw) && !string.IsNullOrWhiteSpace(raw))
        {
            modIds = raw
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
        }

        var byModId = available.ToLookup(m => m.ModId, StringComparer.OrdinalIgnoreCase);
        var ordered = new List<LocalMod>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var id in modIds)
        {
            if (!seen.Add(id)) continue;
            var first = byModId[id].FirstOrDefault();
            if (first is not null) ordered.Add(first);
        }
        return ordered;
    }
}
