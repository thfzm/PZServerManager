namespace PZServerManager.Services;

/// PZ's servertest.ini is a flat key=value file (no [sections]) with `#` comments.
/// We round-trip preserving original ordering / comments / blank lines so any field PZ added
/// that isn't in our schema survives a save unchanged.
public sealed class IniFile
{
    private abstract record Line;
    private sealed record CommentLine(string Text) : Line;
    private sealed record BlankLine : Line;
    private sealed record EntryLine(string Key, string Value) : Line;

    private readonly List<Line> _lines = new();
    private readonly Dictionary<string, int> _index = new(StringComparer.OrdinalIgnoreCase);

    public IEnumerable<string> Keys => _index.Keys;
    public int Count => _index.Count;

    public bool TryGet(string key, out string value)
    {
        if (_index.TryGetValue(key, out var idx) && _lines[idx] is EntryLine e)
        {
            value = e.Value;
            return true;
        }
        value = "";
        return false;
    }

    public string Get(string key, string defaultValue = "")
        => TryGet(key, out var v) ? v : defaultValue;

    public void Set(string key, string value)
    {
        if (_index.TryGetValue(key, out var idx))
            _lines[idx] = new EntryLine(key, value);
        else
        {
            _index[key] = _lines.Count;
            _lines.Add(new EntryLine(key, value));
        }
    }

    public static IniFile Load(string path)
    {
        var ini = new IniFile();
        if (!File.Exists(path)) return ini;
        foreach (var raw in File.ReadAllLines(path))
            ini.Append(raw);
        return ini;
    }

    private void Append(string raw)
    {
        var trimmed = raw.TrimStart();
        if (trimmed.Length == 0) { _lines.Add(new BlankLine()); return; }
        if (trimmed[0] == '#') { _lines.Add(new CommentLine(raw)); return; }
        var eq = raw.IndexOf('=');
        if (eq <= 0) { _lines.Add(new CommentLine(raw)); return; }
        var key = raw[..eq].Trim();
        var value = raw[(eq + 1)..];
        if (key.Length == 0) { _lines.Add(new CommentLine(raw)); return; }
        _index[key] = _lines.Count;
        _lines.Add(new EntryLine(key, value));
    }

    public void Save(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var sw = new StreamWriter(path, append: false, System.Text.Encoding.UTF8);
        foreach (var line in _lines)
        {
            switch (line)
            {
                case CommentLine c: sw.WriteLine(c.Text); break;
                case BlankLine: sw.WriteLine(); break;
                case EntryLine e: sw.WriteLine($"{e.Key}={e.Value}"); break;
            }
        }
    }
}
