namespace PZServerManager.Forms.Controls;

partial class RconControl
{
    private System.ComponentModel.IContainer components = null;

    private Panel _topPanel = null!;
    private Label _profileLabel = null!;
    private ComboBox _profileBox = null!;
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
    private Panel _quickPanel = null!;
    private Button _quickPlayers = null!;
    private Button _quickSave = null!;
    private Button _quickQuit = null!;
    private Button _quickBroadcast = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        _topPanel = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(8, 8, 8, 4) };

        _profileLabel = new Label { Text = "Profile:", AutoSize = true, Location = new Point(8, 13) };
        _profileBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(60, 10),
            Size = new Size(160, 23),
        };
        _profileBox.SelectedIndexChanged += OnProfileChanged;

        _hostLabel = new Label { Text = "Host:", AutoSize = true, Location = new Point(232, 13) };
        _hostBox = new TextBox { Location = new Point(274, 10), Size = new Size(140, 23), Text = "127.0.0.1" };
        _portLabel = new Label { Text = "Port:", AutoSize = true, Location = new Point(424, 13) };
        _portBox = new NumericUpDown
        {
            Location = new Point(462, 10),
            Size = new Size(80, 23),
            Minimum = 0,
            Maximum = 65535,
            Value = 27015,
            ThousandsSeparator = false,
        };
        _passwordLabel = new Label { Text = "Password:", AutoSize = true, Location = new Point(8, 47) };
        _passwordBox = new TextBox
        {
            Location = new Point(80, 44),
            Size = new Size(220, 23),
            UseSystemPasswordChar = true,
        };
        _connectButton = new Button { Text = "Connect", Location = new Point(310, 42), Size = new Size(110, 28) };
        _connectButton.Click += OnConnect;
        _statusLabel = new Label
        {
            Text = "Disconnected",
            AutoSize = true,
            Location = new Point(430, 47),
            ForeColor = SystemColors.GrayText,
        };

        _topPanel.Controls.Add(_profileLabel);
        _topPanel.Controls.Add(_profileBox);
        _topPanel.Controls.Add(_hostLabel);
        _topPanel.Controls.Add(_hostBox);
        _topPanel.Controls.Add(_portLabel);
        _topPanel.Controls.Add(_portBox);
        _topPanel.Controls.Add(_passwordLabel);
        _topPanel.Controls.Add(_passwordBox);
        _topPanel.Controls.Add(_connectButton);
        _topPanel.Controls.Add(_statusLabel);

        _bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 80, Padding = new Padding(8, 4, 8, 8) };

        _quickPanel = new Panel { Dock = DockStyle.Top, Height = 36 };
        _quickPlayers = new Button { Text = "Players", Location = new Point(0, 4), Size = new Size(80, 26) };
        _quickPlayers.Click += OnQuickPlayers;
        _quickSave = new Button { Text = "Save", Location = new Point(86, 4), Size = new Size(80, 26) };
        _quickSave.Click += OnQuickSave;
        _quickQuit = new Button { Text = "Quit", Location = new Point(172, 4), Size = new Size(80, 26) };
        _quickQuit.Click += OnQuickQuit;
        _quickBroadcast = new Button { Text = "Broadcast…", Location = new Point(258, 4), Size = new Size(100, 26) };
        _quickBroadcast.Click += OnQuickBroadcast;
        _quickPanel.Controls.Add(_quickPlayers);
        _quickPanel.Controls.Add(_quickSave);
        _quickPanel.Controls.Add(_quickQuit);
        _quickPanel.Controls.Add(_quickBroadcast);

        _sendButton = new Button { Text = "Send", Dock = DockStyle.Right, Width = 80 };
        _sendButton.Click += OnSend;
        _commandBox = new TextBox { Dock = DockStyle.Fill, Font = new Font("Consolas", 10f) };
        _commandBox.KeyDown += OnCommandKey;

        var inputPanel = new Panel { Dock = DockStyle.Fill };
        inputPanel.Controls.Add(_commandBox);
        inputPanel.Controls.Add(_sendButton);

        _bottomPanel.Controls.Add(inputPanel);
        _bottomPanel.Controls.Add(_quickPanel);

        _logBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor = Color.FromArgb(20, 20, 20),
            ForeColor = Color.Gainsboro,
            Font = new Font("Consolas", 9.5f),
            WordWrap = false,
            BorderStyle = BorderStyle.None,
        };

        Controls.Add(_logBox);
        Controls.Add(_bottomPanel);
        Controls.Add(_topPanel);
    }
}
