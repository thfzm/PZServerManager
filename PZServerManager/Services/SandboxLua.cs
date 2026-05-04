using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace PZServerManager.Services;

/// Reads/writes &lt;profile&gt;_SandboxVars.lua. PZ writes the file as a single top-level
/// table assignment (`SandboxVars = { ... }`) with up to one level of nested subtables
/// (ZombieLore, ZombieConfig). We don't preserve original whitespace — the GUI is the
/// source of truth, so we re-emit canonical indentation on save.
public sealed class SandboxLua
{
    public sealed class Subtable
    {
        public string Key { get; init; } = "";
        public List<KeyValuePair<string, string>> Entries { get; } = new();

        public bool TryGet(string key, out string value)
        {
            foreach (var e in Entries)
                if (string.Equals(e.Key, key, StringComparison.OrdinalIgnoreCase))
                { value = e.Value; return true; }
            value = "";
            return false;
        }

        public void Set(string key, string value)
        {
            for (int i = 0; i < Entries.Count; i++)
                if (string.Equals(Entries[i].Key, key, StringComparison.OrdinalIgnoreCase))
                { Entries[i] = new KeyValuePair<string, string>(Entries[i].Key, value); return; }
            Entries.Add(new KeyValuePair<string, string>(key, value));
        }
    }

    public List<KeyValuePair<string, string>> Scalars { get; } = new();
    public List<Subtable> Subtables { get; } = new();

    public bool TryGet(string path, out string value)
    {
        var dot = path.IndexOf('.');
        if (dot < 0)
        {
            foreach (var e in Scalars)
                if (string.Equals(e.Key, path, StringComparison.OrdinalIgnoreCase))
                { value = e.Value; return true; }
            value = "";
            return false;
        }
        var parent = path[..dot];
        var child = path[(dot + 1)..];
        var sub = Subtables.FirstOrDefault(s => string.Equals(s.Key, parent, StringComparison.OrdinalIgnoreCase));
        if (sub is null) { value = ""; return false; }
        return sub.TryGet(child, out value);
    }

    public string Get(string path, string defaultValue = "")
        => TryGet(path, out var v) ? v : defaultValue;

    public void Set(string path, string value)
    {
        var dot = path.IndexOf('.');
        if (dot < 0)
        {
            for (int i = 0; i < Scalars.Count; i++)
                if (string.Equals(Scalars[i].Key, path, StringComparison.OrdinalIgnoreCase))
                { Scalars[i] = new KeyValuePair<string, string>(Scalars[i].Key, value); return; }
            Scalars.Add(new KeyValuePair<string, string>(path, value));
            return;
        }
        var parent = path[..dot];
        var child = path[(dot + 1)..];
        var sub = Subtables.FirstOrDefault(s => string.Equals(s.Key, parent, StringComparison.OrdinalIgnoreCase));
        if (sub is null) { sub = new Subtable { Key = parent }; Subtables.Add(sub); }
        sub.Set(child, value);
    }

    public IEnumerable<string> AllPaths()
    {
        foreach (var s in Scalars) yield return s.Key;
        foreach (var t in Subtables)
            foreach (var e in t.Entries)
                yield return $"{t.Key}.{e.Key}";
    }

    public static SandboxLua Load(string path)
    {
        if (!File.Exists(path)) return new SandboxLua();
        return Parse(File.ReadAllText(path));
    }

    public static SandboxLua Parse(string content)
    {
        var lua = new SandboxLua();
        var lines = content.Replace("\r\n", "\n").Split('\n');
        Subtable? currentSub = null;

        foreach (var raw in lines)
        {
            var line = StripComment(raw).Trim().TrimEnd(',', ';');
            if (line.Length == 0) continue;
            if (line.StartsWith("SandboxVars", StringComparison.OrdinalIgnoreCase)) continue;
            if (line == "{") continue;
            if (line == "}") { currentSub = null; continue; }

            var sub = Regex.Match(line, @"^(\w+)\s*=\s*\{$");
            if (sub.Success)
            {
                currentSub = new Subtable { Key = sub.Groups[1].Value };
                lua.Subtables.Add(currentSub);
                continue;
            }

            var kv = Regex.Match(line, @"^(\w+)\s*=\s*(.+?)\s*$");
            if (kv.Success)
            {
                var k = kv.Groups[1].Value;
                var v = kv.Groups[2].Value.Trim().TrimEnd(',', ';').Trim();
                if (currentSub is not null) currentSub.Set(k, v);
                else lua.Scalars.Add(new KeyValuePair<string, string>(k, v));
            }
        }
        return lua;
    }

    private static string StripComment(string line)
    {
        var idx = line.IndexOf("--", StringComparison.Ordinal);
        return idx >= 0 ? line[..idx] : line;
    }

    public void Save(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, Render(), Encoding.UTF8);
    }

    public string Render()
    {
        var sb = new StringBuilder();
        sb.AppendLine("SandboxVars = {");
        foreach (var s in Scalars) sb.AppendLine($"    {s.Key} = {s.Value},");
        foreach (var t in Subtables)
        {
            sb.AppendLine($"    {t.Key} = {{");
            foreach (var e in t.Entries) sb.AppendLine($"        {e.Key} = {e.Value},");
            sb.AppendLine("    },");
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

    public static string FormatBool(bool b) => b ? "true" : "false";
    public static string FormatFloat(double d) => d.ToString("0.0##", CultureInfo.InvariantCulture);
    public static bool TryParseBool(string s, out bool b)
    {
        if (string.Equals(s, "true", StringComparison.OrdinalIgnoreCase)) { b = true; return true; }
        if (string.Equals(s, "false", StringComparison.OrdinalIgnoreCase)) { b = false; return true; }
        b = false;
        return false;
    }
}
