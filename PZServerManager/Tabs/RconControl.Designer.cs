namespace PZServerManager.Tabs;

partial class RconControl
{
    private System.ComponentModel.IContainer components = null;

    private Panel _topPanel = null!;
    private Label _profileLabel = null!;
    private ComboBox _profileBox = null!;
    private Button _reloadButton = null!;
    private Label _hostLabel = null!;
    private TextBox _hostBox = null!;
    private Label _portLabel = null!;
    private NumericUpDown _portBox = null!;
    private Label _passwordLabel = null!;
    private TextBox _passwordBox = null!;
    private Button _connectButton = null!;
    private Label _statusLabel = null!;

    private TextBox _logBox = null!;

    private Panel _bottomPanel = null!;
    private TextBox _commandBox = null!;
    private Button _sendButton = null!;
    private Button _quickPlayers = null!;
    private Button _quickSave = null!;
    private Button _quickQuit = null!;
    private Button _quickBroadcast = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        // ---- top toolbar ----
        _topPanel = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(12, 8, 12, 6) };
        var top = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 8,
            RowCount = 2,
            Font = new Font("Segoe UI", 9.5f),
        };
        top.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        top.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        top.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

        // Row 0: Profile + Reload + Status
        _profileLabel = new Label { Text = "Profile:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(0, 7, 6, 0) };
        _profileBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill, Margin = new Padding(0, 4, 8, 4) };
        _profileBox.SelectedIndexChanged += OnProfileChanged;
        _reloadButton = new Button { Text = "Reload from ini", Dock = DockStyle.Fill, Margin = new Padding(0, 2, 12, 2) };
        _reloadButton.Click += OnReloadDefaults;
        _statusLabel = new Label { Text = "Disconnected", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(8, 7, 0, 0), ForeColor = SystemColors.GrayText };

        top.Controls.Add(_profileLabel, 0, 0);
        top.Controls.Add(_profileBox, 1, 0);
        top.Controls.Add(_reloadButton, 2, 0);
        top.Controls.Add(_statusLabel, 3, 0);
        top.SetColumnSpan(_statusLabel, 5);

        // Row 1: Host + Port + Password + Connect
        _hostLabel = new Label { Text = "Host:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(0, 7, 6, 0) };
        _hostBox = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 4, 8, 4), Text = "127.0.0.1" };
        _portLabel = new Label { Text = "Port:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(0, 7, 6, 0) };
        _portBox = new NumericUpDown { Dock = DockStyle.Fill, Margin = new Padding(0, 4, 12, 4), Minimum = 0, Maximum = 65535, Value = 27015, ThousandsSeparator = false };
        _passwordLabel = new Label { Text = "Password:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(0, 7, 6, 0) };
        _passwordBox = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 4, 8, 4), UseSystemPasswordChar = true };
        _connectButton = new Button { Text = "Connect", Dock = DockStyle.Fill, Margin = new Padding(0, 2, 0, 2) };
        _connectButton.Click += OnConnect;

        top.Controls.Add(_hostLabel, 0, 1);
        top.Controls.Add(_hostBox, 1, 1);
        top.Controls.Add(_portLabel, 2, 1);
        top.Controls.Add(_portBox, 3, 1);
        top.Controls.Add(_passwordLabel, 4, 1);
        top.Controls.Add(_passwordBox, 5, 1);
        top.Controls.Add(_connectButton, 6, 1);
        top.SetColumnSpan(_connectButton, 2);

        _topPanel.Controls.Add(top);

        // ---- bottom: quick + command row ----
        _bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 80, Padding = new Padding(12, 6, 12, 8) };
        var bottom = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 5,
            RowCount = 2,
            Font = new Font("Segoe UI", 9.5f),
        };
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        bottom.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        bottom.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

        _quickPlayers = new Button { Text = "Players", Dock = DockStyle.Fill, Margin = new Padding(0, 2, 4, 2) };
        _quickPlayers.Click += OnQuickPlayers;
        _quickSave = new Button { Text = "Save", Dock = DockStyle.Fill, Margin = new Padding(0, 2, 4, 2) };
        _quickSave.Click += OnQuickSave;
        _quickQuit = new Button { Text = "Quit", Dock = DockStyle.Fill, Margin = new Padding(0, 2, 4, 2) };
        _quickQuit.Click += OnQuickQuit;
        _quickBroadcast = new Button { Text = "Broadcast…", Dock = DockStyle.Fill, Margin = new Padding(0, 2, 0, 2) };
        _quickBroadcast.Click += OnQuickBroadcast;
        bottom.Controls.Add(_quickPlayers, 0, 0);
        bottom.Controls.Add(_quickSave, 1, 0);
        bottom.Controls.Add(_quickQuit, 2, 0);
        bottom.Controls.Add(_quickBroadcast, 3, 0);

        _commandBox = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 4, 8, 4), Font = new Font("Consolas", 10f) };
        _commandBox.KeyDown += OnCommandKey;
        _sendButton = new Button { Text = "Send", Dock = DockStyle.Fill, Margin = new Padding(0, 2, 0, 2) };
        _sendButton.Click += OnSend;
        bottom.Controls.Add(_commandBox, 0, 1);
        bottom.SetColumnSpan(_commandBox, 4);
        bottom.Controls.Add(_sendButton, 4, 1);

        _bottomPanel.Controls.Add(bottom);

        // ---- center: log ----
        _logBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor = Color.FromArgb(20, 20, 20),
            ForeColor = Color.Gainsboro,
            Font = new Font("Consolas", 9.5f),
            BorderStyle = BorderStyle.None,
            WordWrap = false,
        };

        Controls.Add(_logBox);
        Controls.Add(_bottomPanel);
        Controls.Add(_topPanel);
    }
}
