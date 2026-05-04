using System.Globalization;
using PZServerManager.Models;
using PZServerManager.Services;

namespace PZServerManager.Forms.Controls;

public partial class SandboxControl : UserControl
{
    private sealed class Editor
    {
        public required SandboxFieldDef Def { get; init; }
        public required Func<string> Get { get; init; }
        public required Action<string> Set { get; init; }
    }

    private PzPaths? _paths;
    private SandboxLua? _lua;
    private ServerProfile _profile = new() { Name = ServerProfile.Default };
    private readonly Dictionary<string, Editor> _editors = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<(string Path, TextBox Ctl)> _rawEditors = new();
    private bool _suppressDirty;
    private bool _dirty;
    private readonly ToolTip _tips = new() { AutoPopDelay = 30000, InitialDelay = 350, ReshowDelay = 200 };

    public SandboxControl()
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
        foreach (var category in SandboxSchema.CategoryOrder)
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
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

        foreach (var def in SandboxSchema.InCategory(category))
            AddFieldRow(table, def);

        scroll.Controls.Add(table);
        page.Controls.Add(scroll);
        return page;
    }

    private void AddFieldRow(TableLayoutPanel table, SandboxFieldDef def)
    {
        var label = new Label
        {
            Text = def.Label,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 8, 12, 8),
        };

        Control editor;
        Func<string> getter;
        Action<string> setter;

        switch (def.Type)
        {
            case SandboxFieldType.Bool:
            {
                var cb = new CheckBox
                {
                    AutoSize = true,
                    Anchor = AnchorStyles.Left,
                    Margin = new Padding(0, 8, 0, 8),
                };
                cb.CheckedChanged += (_, _) => MarkDirty();
                editor = cb;
                getter = () => SandboxLua.FormatBool(cb.Checked);
                setter = v => cb.Checked = SandboxLua.TryParseBool(v, out var b) && b;
                break;
            }
            case SandboxFieldType.IntChoices:
            {
                var combo = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Width = 220,
                    Anchor = AnchorStyles.Left,
                    Margin = new Padding(0, 4, 0, 4),
                };
                if (def.Choices is not null)
                {
                    foreach (var c in def.Choices)
                        combo.Items.Add(new SandboxChoiceItem(c));
                }
                combo.SelectedIndexChanged += (_, _) => MarkDirty();
                editor = combo;
                getter = () =>
                {
                    if (combo.SelectedItem is SandboxChoiceItem item)
                        return item.Choice.Value.ToString(CultureInfo.InvariantCulture);
                    return def.DefaultValue ?? "0";
                };
                setter = v =>
                {
                    if (!int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
                    {
                        combo.SelectedIndex = combo.Items.Count > 0 ? 0 : -1;
                        return;
                    }
                    for (int i = 0; i < combo.Items.Count; i++)
                        if (combo.Items[i] is SandboxChoiceItem ci && ci.Choice.Value == n)
                        { combo.SelectedIndex = i; return; }
                    combo.SelectedIndex = -1;
                };
                break;
            }
            case SandboxFieldType.Int:
            {
                var nud = new NumericUpDown
                {
                    Width = 140,
                    Anchor = AnchorStyles.Left,
                    Margin = new Padding(0, 4, 0, 4),
                    Minimum = def.IntMin ?? int.MinValue,
                    Maximum = def.IntMax ?? int.MaxValue,
                    ThousandsSeparator = false,
                };
                nud.ValueChanged += (_, _) => MarkDirty();
                editor = nud;
                getter = () => ((long)nud.Value).ToString(CultureInfo.InvariantCulture);
                setter = v =>
                {
                    if (long.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
                    {
                        var clamped = Math.Min(Math.Max(n, (long)nud.Minimum), (long)nud.Maximum);
                        nud.Value = clamped;
                    }
                };
                break;
            }
            default: // Float
            {
                var tb = new TextBox
                {
                    Width = 160,
                    Anchor = AnchorStyles.Left,
                    Margin = new Padding(0, 4, 0, 4),
                };
                tb.TextChanged += (_, _) => MarkDirty();
                editor = tb;
                getter = () =>
                {
                    var s = tb.Text.Trim();
                    if (string.IsNullOrEmpty(s)) return def.DefaultValue ?? "0.0";
                    if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
                        return SandboxLua.FormatFloat(d);
                    return s;
                };
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
        _editors[def.Path] = new Editor { Def = def, Get = getter, Set = setter };
    }

    private TabPage BuildRawPage()
    {
        var page = new TabPage("Raw");
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(12) };
        var note = new Label
        {
            Dock = DockStyle.Top,
            Text = "Sandbox keys not in the schema. Edit cautiously; values are written back unchanged.",
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
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

        scroll.Controls.Add(table);
        scroll.Controls.Add(note);
        page.Controls.Add(scroll);
        return page;
    }

    private void PopulateRawTab(SandboxLua lua)
    {
        var rawPage = _subTabs.TabPages[_subTabs.TabPages.Count - 1];
        var table = (TableLayoutPanel)rawPage.Controls[0].Controls.OfType<Control>()
            .First(c => c is TableLayoutPanel t && (t.Tag as string) == "raw-table");
        table.SuspendLayout();
        table.Controls.Clear();
        _rawEditors.Clear();

        foreach (var path in lua.AllPaths())
        {
            if (SandboxSchema.ByPath.ContainsKey(path)) continue;
            lua.TryGet(path, out var v);
            var label = new Label
            {
                Text = path,
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
            _rawEditors.Add((path, tb));
        }
        table.ResumeLayout();
    }

    private void LoadCurrent()
    {
        if (_paths is null) return;
        var path = _paths.SandboxLuaPath(_profile);
        _lua = SandboxLua.Load(path);

        _suppressDirty = true;
        try
        {
            foreach (var (key, ed) in _editors)
            {
                if (_lua.TryGet(key, out var v))
                    ed.Set(v);
                else if (ed.Def.DefaultValue is not null)
                    ed.Set(ed.Def.DefaultValue);
            }
            PopulateRawTab(_lua);
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

    private void OnSave(object? sender, EventArgs e)
    {
        if (_paths is null || _lua is null) return;

        foreach (var (path, ed) in _editors)
            _lua.Set(path, ed.Get());
        foreach (var (path, tb) in _rawEditors)
            _lua.Set(path, tb.Text);

        var file = _paths.SandboxLuaPath(_profile);
        try
        {
            _lua.Save(file);
            _dirty = false;
            UpdateDirtyUi();
            _statusLabel.Text = $"Saved {Path.GetFileName(file)}";
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
                $"Discard unsaved changes to {_profile.Name}_SandboxVars.lua?",
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

    private sealed class SandboxChoiceItem
    {
        public SandboxChoice Choice { get; }
        public SandboxChoiceItem(SandboxChoice choice) { Choice = choice; }
        public override string ToString() => $"{Choice.Value} — {Choice.Label}";
    }
}
