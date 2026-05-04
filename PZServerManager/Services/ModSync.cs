using PZServerManager.Models;

namespace PZServerManager.Services;

/// Owns the rule that mod load order must satisfy `require=` chains, plus the canonical
/// shape of `servertest.ini`'s Mods= / WorkshopItems= / Map= triple. The Mods tab UI
/// produces user intent (which mods + their preferred order); this service enforces the
/// invariants PZ needs to actually boot.
public static class ModSync
{
    public const string BaseMap = "Muldraugh, KY";

    public sealed class SyncResult
    {
        public List<LocalMod> OrderedEnabled { get; init; } = new();
        public List<string> Warnings { get; init; } = new();
    }

    /// Stable topological sort: respects `require=` (deps first), preserves user-given order
    /// among siblings whose dependency relationships allow it.
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
                warnings.Add($"순환 의존성: '{m.DisplayName}' 주변");
                return;
            }
            foreach (var dep in m.Requires)
            {
                if (byModId.TryGetValue(dep, out var depMod)) Visit(depMod);
                else warnings.Add($"'{m.DisplayName}' 가 '{dep}' 를 require= 하는데 활성된 모드 중에 없음");
            }
            visiting.Remove(m.ModId);
            visited.Add(m.ModId);
            ordered.Add(m);
        }

        // Visit in user-defined order so siblings preserve their relative positions.
        foreach (var m in enabled.OrderBy(x => orderIndex[x.ModId])) Visit(m);

        return new SyncResult { OrderedEnabled = ordered, Warnings = warnings };
    }

    /// Builds canonical `Map=` value: enabled map mods' map names in load order, deduplicated,
    /// with `Muldraugh, KY` always pinned last (PZ uses the last entry as the base layer fallback).
    /// User-set custom map entries (added via Raw config) are preserved if not already covered.
    public static string BuildMapValue(IReadOnlyList<LocalMod> orderedEnabled, string? existingValue)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<string>();

        foreach (var m in orderedEnabled)
            foreach (var name in m.MapNames)
            {
                if (string.Equals(name, BaseMap, StringComparison.OrdinalIgnoreCase)) continue;
                if (seen.Add(name)) result.Add(name);
            }

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
        => string.Join(';', orderedEnabled.Select(m => m.WorkshopId.ToString()).Where(s => s != "0").Distinct());

    /// Applies the three managed fields to the in-memory ini. Caller saves.
    public static void Apply(IniFile ini, IReadOnlyList<LocalMod> orderedEnabled)
    {
        ini.Set("Mods", BuildModsValue(orderedEnabled));
        ini.Set("WorkshopItems", BuildWorkshopItemsValue(orderedEnabled));
        ini.TryGet("Map", out var existingMap);
        ini.Set("Map", BuildMapValue(orderedEnabled, existingMap));
    }

    /// Translates an existing `Mods=` value back to LocalMod instances in that order.
    /// Mods listed in ini but not present locally are dropped silently.
    public static List<LocalMod> ReadEnabledFromIni(IniFile ini, IReadOnlyList<LocalMod> available)
    {
        var modIds = new List<string>();
        if (ini.TryGet("Mods", out var raw) && !string.IsNullOrWhiteSpace(raw))
        {
            modIds = raw.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        }

        var byModId = available.ToLookup(m => m.ModId, StringComparer.OrdinalIgnoreCase);
        var result = new List<LocalMod>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var id in modIds)
        {
            if (!seen.Add(id)) continue;
            var m = byModId[id].FirstOrDefault();
            if (m is not null) result.Add(m);
        }
        return result;
    }
}
