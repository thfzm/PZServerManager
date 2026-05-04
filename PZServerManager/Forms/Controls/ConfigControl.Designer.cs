namespace PZServerManager.Forms.Controls;

partial class ConfigControl
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
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        _topPanel = new Panel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(8, 8, 8, 8) };
        _profileLabel = new Label { Text = "Profile:", AutoSize = true, Location = new Point(8, 13) };
        _profileBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(60, 10),
            Size = new Size(200, 23),
        };
        _profileBox.SelectedIndexChanged += OnProfileChanged;
        _reloadButton = new Button
        {
            Text = "Reload", Location = new Point(272, 9), Size = new Size(80, 26),
        };
        _reloadButton.Click += OnReload;
        _saveButton = new Button
        {
            Text = "Save", Location = new Point(360, 9), Size = new Size(80, 26),
        };
        _saveButton.Click += OnSave;
        _dirtyLabel = new Label
        {
            Text = "● modified",
            AutoSize = true,
            Location = new Point(450, 13),
            ForeColor = Color.OrangeRed,
            Visible = false,
        };
        _statusLabel = new Label
        {
            Text = "",
            AutoSize = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(560, 13),
            ForeColor = SystemColors.GrayText,
        };
        _topPanel.Controls.Add(_profileLabel);
        _topPanel.Controls.Add(_profileBox);
        _topPanel.Controls.Add(_reloadButton);
        _topPanel.Controls.Add(_saveButton);
        _topPanel.Controls.Add(_dirtyLabel);
        _topPanel.Controls.Add(_statusLabel);

        _subTabs = new TabControl { Dock = DockStyle.Fill };

        Controls.Add(_subTabs);
        Controls.Add(_topPanel);
    }
}
