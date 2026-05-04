using PZServerManager.Models;
using PZServerManager.Services;

namespace PZServerManager.Forms.Controls;

public partial class ConfigControl : UserControl
{
    private sealed class Editor
    {
        public required IniFieldDef Def { get; init; }
        public required Control Ctl { get; init; }
        public required Func<string> Get { get; init; }
        public required Action<string> Set { get; init; }
    }

    private PzPaths? _paths;
    private IniFile? _ini;
    private ServerProfile _profile = new() { Name = ServerProfile.Default };
    private readonly Dictionary<string, Editor> _editors = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<(string Key, TextBox Ctl)> _rawEditors = new();
    private bool _suppressDirty;
    private bool _dirty;
    private readonly ToolTip _tips = new() { AutoPopDelay = 30000, InitialDelay = 350, ReshowDelay = 200 };

    public ConfigControl()
    {
        InitializeComponent();
        BuildCategoryTabs();
        UpdateDirtyUi();
    }

    public void Bind(PzPaths paths)
    {
        _paths = paths;
        ReloadProfiles();
        LoadCurrent();
    }

    public void ReloadProfiles()
    {
        if (_paths is null) return;
        var profiles = _paths.ListProfileNames().ToList();
        if (profiles.Count == 0) profiles.Add(ServerProfile.Default);

        _profileBox.BeginUpdate();
        _profileBox.Items.Clear();
        foreach (var p in profiles) _profileBox.Items.Add(p);
        if (_profileBox.Items.Contains(_profile.Name))
            _profileBox.SelectedItem = _profile.Name;
        else
            _profileBox.SelectedIndex = 0;
        _profileBox.EndUpdate();
    }

    private void BuildCategoryTabs()
    {
        _subTabs.TabPages.Clear();
        foreach (var category in ServerConfigSchema.CategoryOrder)
            _subTabs.TabPages.Add(BuildCategoryPage(category));
        _subTabs.TabPages.Add(BuildRawPage());
    }

    private TabPage BuildCategoryPage(string category)
    {
        var page = new TabPage(category);
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(12) };
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(0, 0, 12, 0),
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

        foreach (var def in ServerConfigSchema.InCategory(category))
            AddFieldRow(table, def);

        scroll.Controls.Add(table);
        page.Controls.Add(scroll);
        return page;
    }

    private void AddFieldRow(TableLayoutPanel table, IniFieldDef def)
    {
        var label = new Label
        {
            Text = def.Label,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 8, 12, 8),
        };
        if (def.IsManagedElsewhere)
        {
            label.ForeColor = SystemColors.GrayText;
            label.Text += "  (managed elsewhere)";
        }

        Control editor;
        Func<string> getter;
        Action<string> setter;

        switch (def.Type)
        {
            case IniFieldType.Bool:
            {
                var cb = new CheckBox
                {
                    AutoSize = true,
                    Anchor = AnchorStyles.Left,
                    Margin = new Padding(0, 8, 0, 8),
                    Enabled = !def.IsManagedElsewhere,
                };
                cb.CheckedChanged += (_, _) => MarkDirty();
                editor = cb;
                getter = () => cb.Checked ? "true" : "false";
                setter = v => cb.Checked = string.Equals(v, "true", StringComparison.OrdinalIgnoreCase);
                break;
            }
            case IniFieldType.Int:
            {
                var nud = new NumericUpDown
                {
                    Width = 140,
                    Anchor = AnchorStyles.Left,
                    Margin = new Padding(0, 4, 0, 4),
                    Minimum = def.IntMin ?? int.MinValue,
                    Maximum = def.IntMax ?? int.MaxValue,
                    Enabled = !def.IsManagedElsewhere,
                    ThousandsSeparator = false,
                };
                nud.ValueChanged += (_, _) => MarkDirty();
                editor = nud;
                getter = () => ((long)nud.Value).ToString();
                setter = v =>
                {
                    if (long.TryParse(v, out var n))
                    {
                        var clamped = Math.Min(Math.Max(n, (long)nud.Minimum), (long)nud.Maximum);
                        nud.Value = clamped;
                    }
                };
                break;
            }
            case IniFieldType.Float:
            {
                var tb = new TextBox
                {
                    Width = 160,
                    Anchor = AnchorStyles.Left,
                    Margin = new Padding(0, 4, 0, 4),
                    Enabled = !def.IsManagedElsewhere,
                };
                tb.TextChanged += (_, _) => MarkDirty();
                editor = tb;
                getter = () => tb.Text;
                setter = v => tb.Text = v;
                break;
            }
            case IniFieldType.MultilineString:
            {
                var tb = new TextBox
                {
                    Multiline = true,
                    ScrollBars = ScrollBars.Vertical,
                    Height = 70,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right,
                    Margin = new Padding(0, 4, 0, 4),
                    Enabled = !def.IsManagedElsewhere,
                };
                tb.TextChanged += (_, _) => MarkDirty();
                editor = tb;
                getter = () => tb.Text;
                setter = v => tb.Text = v;
                break;
            }
            default: // String
            {
                var tb = new TextBox
                {
                    Anchor = AnchorStyles.Left | AnchorStyles.Right,
                    Margin = new Padding(0, 4, 0, 4),
                    Enabled = !def.IsManagedElsewhere,
                    UseSystemPasswordChar = def.IsPassword,
                };
                tb.TextChanged += (_, _) => MarkDirty();
                editor = tb;
                getter = () => tb.Text;
                setter = v => tb.Text = v;
                break;
            }
        }

        if (!string.IsNullOrEmpty(def.Description))
        {
            _tips.SetToolTip(label, def.Description);
            _tips.SetToolTip(editor, def.Description);
        }

        table.Controls.Add(label);
        table.Controls.Add(editor);
        _editors[def.Key] = new Editor { Def = def, Ctl = editor, Get = getter, Set = setter };
    }

    private TabPage BuildRawPage()
    {
        var page = new TabPage("Raw");
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(12) };
        var note = new Label
        {
            Dock = DockStyle.Top,
            Text = "Keys not in the schema. Edit at your own risk; values are written through unchanged.",
            ForeColor = SystemColors.GrayText,
            AutoSize = true,
            Padding = new Padding(0, 0, 0, 8),
        };
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(0, 24, 12, 0),
            Tag = "raw-table",
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

        scroll.Controls.Add(table);
        scroll.Controls.Add(note);
        page.Controls.Add(scroll);
        return page;
    }

    private void PopulateRawTab(IniFile ini)
    {
        var rawPage = _subTabs.TabPages[_subTabs.TabPages.Count - 1];
        var table = (TableLayoutPanel)rawPage.Controls[0].Controls.OfType<Control>()
            .First(c => c is TableLayoutPanel t && (t.Tag as string) == "raw-table");
        table.SuspendLayout();
        table.Controls.Clear();
        _rawEditors.Clear();

        foreach (var key in ini.Keys)
        {
            if (ServerConfigSchema.ByKey.ContainsKey(key)) continue;
            ini.TryGet(key, out var v);
            var label = new Label
            {
                Text = key,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 8, 12, 8),
                ForeColor = SystemColors.GrayText,
            };
            var tb = new TextBox
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Margin = new Padding(0, 4, 0, 4),
                Text = v,
            };
            tb.TextChanged += (_, _) => MarkDirty();
            table.Controls.Add(label);
            table.Controls.Add(tb);
            _rawEditors.Add((key, tb));
        }
        table.ResumeLayout();
    }

    private void LoadCurrent()
    {
        if (_paths is null) return;
        var path = _paths.IniPath(_profile);
        _ini = IniFile.Load(path);

        _suppressDirty = true;
        try
        {
            foreach (var (key, ed) in _editors)
            {
                if (_ini.TryGet(key, out var v))
                    ed.Set(v);
                else if (ed.Def.DefaultValue is not null)
                    ed.Set(ed.Def.DefaultValue);
                else
                    ed.Set("");
            }
            PopulateRawTab(_ini);
        }
        finally
        {
            _suppressDirty = false;
            _dirty = false;
            UpdateDirtyUi();
        }

        _statusLabel.Text = File.Exists(path)
            ? $"Loaded {Path.GetFileName(path)}"
            : $"{Path.GetFileName(path)} does not exist yet — Save to create.";
    }

    private bool ValidateAll(out string? error)
    {
        foreach (var (_, ed) in _editors)
        {
            if (ed.Def.Type == IniFieldType.Int)
            {
                var raw = ed.Get();
                if (!long.TryParse(raw, out var n))
                {
                    error = $"{ed.Def.Label}: '{raw}' is not a valid integer.";
                    return false;
                }
                if (ed.Def.IntMin is int min && n < min)
                {
                    error = $"{ed.Def.Label}: must be >= {min}.";
                    return false;
                }
                if (ed.Def.IntMax is int max && n > max)
                {
                    error = $"{ed.Def.Label}: must be <= {max}.";
                    return false;
                }
            }
            else if (ed.Def.Type == IniFieldType.Float)
            {
                var raw = ed.Get();
                if (!string.IsNullOrWhiteSpace(raw) &&
                    !double.TryParse(raw, System.Globalization.CultureInfo.InvariantCulture, out _))
                {
                    error = $"{ed.Def.Label}: '{raw}' is not a valid number.";
                    return false;
                }
            }
        }

        if (_editors.TryGetValue("DefaultPort", out var dp) &&
            _editors.TryGetValue("UDPPort", out var up) &&
            dp.Get() == up.Get() &&
            !string.IsNullOrEmpty(dp.Get()))
        {
            error = "DefaultPort and UDPPort must differ.";
            return false;
        }

        error = null;
        return true;
    }

    private void OnSave(object? sender, EventArgs e)
    {
        if (_paths is null || _ini is null) return;
        if (!ValidateAll(out var err))
        {
            MessageBox.Show(this, err, "Validation failed",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        foreach (var (key, ed) in _editors)
        {
            if (ed.Def.IsManagedElsewhere) continue;
            _ini.Set(key, ed.Get());
        }
        foreach (var (key, tb) in _rawEditors)
            _ini.Set(key, tb.Text);

        var path = _paths.IniPath(_profile);
        try
        {
            _ini.Save(path);
            _dirty = false;
            UpdateDirtyUi();
            _statusLabel.Text = $"Saved {Path.GetFileName(path)}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Save failed",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnReload(object? sender, EventArgs e)
    {
        if (_dirty)
        {
            var r = MessageBox.Show(this, "Discard unsaved changes?", "Reload",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) return;
        }
        LoadCurrent();
    }

    private void OnProfileChanged(object? sender, EventArgs e)
    {
        var name = _profileBox.SelectedItem as string;
        if (string.IsNullOrEmpty(name) || name == _profile.Name) return;

        if (_dirty)
        {
            var r = MessageBox.Show(this,
                $"Discard unsaved changes to {_profile.Name}.ini?",
                "Switch profile", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes)
            {
                _profileBox.SelectedItem = _profile.Name;
                return;
            }
        }

        _profile = new ServerProfile { Name = name };
        LoadCurrent();
    }

    private void MarkDirty()
    {
        if (_suppressDirty) return;
        if (_dirty) return;
        _dirty = true;
        UpdateDirtyUi();
    }

    private void UpdateDirtyUi()
    {
        _saveButton.Enabled = _dirty;
        _dirtyLabel.Visible = _dirty;
    }
}
