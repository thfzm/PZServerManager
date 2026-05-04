namespace PZServerManager.Tabs;

partial class SandboxTab
{
    private System.ComponentModel.IContainer components = null;

    private Panel _topPanel = null!;
    private Label _profileLabel = null!;
    private ComboBox _profileBox = null!;
    private Button _reloadButton = null!;
    private Button _saveButton = null!;
    private Label _dirtyLabel = null!;
    private Label _statusLabel = null!;
    private TabControl _subTabs = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        _topPanel = new Panel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(12, 8, 12, 6) };
        var bar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 6,
            RowCount = 1,
            Font = new Font("Segoe UI", 9.5f),
        };
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        bar.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        _profileLabel = new Label { Text = "Profile:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(0, 7, 8, 0) };
        _profileBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 4, 8, 4),
        };
        _profileBox.SelectedIndexChanged += OnProfileChanged;
        _reloadButton = new Button { Text = "Reload", Dock = DockStyle.Fill, Margin = new Padding(0, 2, 4, 2) };
        _reloadButton.Click += OnReload;
        _saveButton = new Button { Text = "Save", Dock = DockStyle.Fill, Margin = new Padding(0, 2, 4, 2) };
        _saveButton.Click += OnSave;
        _dirtyLabel = new Label
        {
            Text = "● modified",
            Anchor = AnchorStyles.Left,
            AutoSize = true,
            Margin = new Padding(8, 7, 0, 0),
            ForeColor = Color.OrangeRed,
            Visible = false,
        };
        _statusLabel = new Label
        {
            Text = "",
            Anchor = AnchorStyles.Left,
            AutoSize = true,
            Margin = new Padding(8, 7, 0, 0),
            ForeColor = SystemColors.GrayText,
        };

        bar.Controls.Add(_profileLabel, 0, 0);
        bar.Controls.Add(_profileBox, 1, 0);
        bar.Controls.Add(_reloadButton, 2, 0);
        bar.Controls.Add(_saveButton, 3, 0);
        bar.Controls.Add(_dirtyLabel, 4, 0);
        bar.Controls.Add(_statusLabel, 5, 0);
        _topPanel.Controls.Add(bar);

        _subTabs = new TabControl { Dock = DockStyle.Fill };
        Controls.Add(_subTabs);
        Controls.Add(_topPanel);
    }
}
