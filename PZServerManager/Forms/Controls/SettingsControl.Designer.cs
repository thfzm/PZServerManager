namespace PZServerManager.Forms.Controls;

partial class SettingsControl
{
    private System.ComponentModel.IContainer components = null;

    private GroupBox _pathsBox = null!;
    private TextBox _steamCmdDirBox = null!;
    private Button _openSteamCmdButton = null!;
    private TextBox _serverDirBox = null!;
    private Button _openServerButton = null!;
    private TextBox _backupDirBox = null!;
    private Button _openBackupButton = null!;
    private TextBox _zomboidDirBox = null!;
    private Button _openZomboidButton = null!;
    private TextBox _appConfigPathBox = null!;
    private Button _openAppDataButton = null!;

    private GroupBox _serverBox = null!;
    private Button _updateButton = null!;
    private Button _validateButton = null!;
    private Button _cancelButton = null!;

    private GroupBox _networkBox = null!;
    private TextBox _ipBox = null!;
    private Button _refreshIpButton = null!;

    private GroupBox _apiBox = null!;
    private TextBox _apiKeyBox = null!;
    private Button _saveApiButton = null!;
    private LinkLabel _apiLink = null!;

    private GroupBox _aboutBox = null!;
    private Label _versionLabel = null!;

    private TextBox _logBox = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(12) };
        var stack = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 1,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
        };

        // ---- Paths ----
        _pathsBox = new GroupBox { Text = "Paths", Width = 900, Height = 220, Padding = new Padding(8) };
        AddPathRow(_pathsBox, "SteamCMD dir:", 24, out _steamCmdDirBox, out _openSteamCmdButton, OnOpenSteamCmd);
        AddPathRow(_pathsBox, "Server dir:", 60, out _serverDirBox, out _openServerButton, OnOpenServer);
        AddPathRow(_pathsBox, "Backup dir:", 96, out _backupDirBox, out _openBackupButton, OnOpenBackupDir);
        AddPathRow(_pathsBox, "Zomboid user dir:", 132, out _zomboidDirBox, out _openZomboidButton, OnOpenZomboid);
        AddPathRow(_pathsBox, "App config:", 168, out _appConfigPathBox, out _openAppDataButton, OnOpenAppData);

        // ---- Server (update / validate) ----
        _serverBox = new GroupBox { Text = "Server install", Width = 900, Height = 130, Padding = new Padding(8) };
        var serverNote = new Label
        {
            Text = "Re-runs SteamCMD against the same install dir. Stop the server first.",
            AutoSize = true,
            Location = new Point(8, 28),
            ForeColor = SystemColors.GrayText,
        };
        _updateButton = new Button { Text = "Update / install", Location = new Point(8, 56), Size = new Size(140, 28) };
        _updateButton.Click += OnUpdateServer;
        _validateButton = new Button { Text = "Validate files", Location = new Point(156, 56), Size = new Size(140, 28) };
        _validateButton.Click += OnValidateServer;
        _cancelButton = new Button { Text = "Cancel running", Location = new Point(304, 56), Size = new Size(140, 28) };
        _cancelButton.Click += OnCancel;
        _serverBox.Controls.Add(serverNote);
        _serverBox.Controls.Add(_updateButton);
        _serverBox.Controls.Add(_validateButton);
        _serverBox.Controls.Add(_cancelButton);

        // ---- Network ----
        _networkBox = new GroupBox { Text = "Network", Width = 900, Height = 90, Padding = new Padding(8) };
        var ipLabel = new Label { Text = "Public IP:", AutoSize = true, Location = new Point(8, 32) };
        _ipBox = new TextBox
        {
            Location = new Point(120, 28),
            Size = new Size(220, 23),
            ReadOnly = true,
            BackColor = SystemColors.Window,
        };
        _refreshIpButton = new Button { Text = "Refresh", Location = new Point(348, 27), Size = new Size(80, 26) };
        _refreshIpButton.Click += OnRefreshIp;
        var ipNote = new Label
        {
            Text = "(UDP port reachability isn't reliably testable from outside; verify by joining from another machine.)",
            AutoSize = true,
            Location = new Point(440, 32),
            ForeColor = SystemColors.GrayText,
        };
        _networkBox.Controls.Add(ipLabel);
        _networkBox.Controls.Add(_ipBox);
        _networkBox.Controls.Add(_refreshIpButton);
        _networkBox.Controls.Add(ipNote);

        // ---- API key ----
        _apiBox = new GroupBox { Text = "Steam Web API key (used by Mods tab)", Width = 900, Height = 110, Padding = new Padding(8) };
        var apiLabel = new Label { Text = "Key:", AutoSize = true, Location = new Point(8, 32) };
        _apiKeyBox = new TextBox
        {
            Location = new Point(60, 28),
            Size = new Size(420, 23),
            UseSystemPasswordChar = true,
        };
        _saveApiButton = new Button { Text = "Save", Location = new Point(488, 27), Size = new Size(80, 26) };
        _saveApiButton.Click += OnSaveApiKey;
        _apiLink = new LinkLabel
        {
            Text = "Get an API key at https://steamcommunity.com/dev/apikey",
            Location = new Point(8, 64),
            AutoSize = true,
        };
        _apiLink.LinkClicked += (_, _) =>
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://steamcommunity.com/dev/apikey",
                UseShellExecute = true,
            });
        _apiBox.Controls.Add(apiLabel);
        _apiBox.Controls.Add(_apiKeyBox);
        _apiBox.Controls.Add(_saveApiButton);
        _apiBox.Controls.Add(_apiLink);

        // ---- About ----
        _aboutBox = new GroupBox { Text = "About", Width = 900, Height = 70, Padding = new Padding(8) };
        _versionLabel = new Label { Text = "", AutoSize = true, Location = new Point(8, 28), ForeColor = SystemColors.GrayText };
        _aboutBox.Controls.Add(_versionLabel);

        stack.Controls.Add(_pathsBox);
        stack.Controls.Add(_serverBox);
        stack.Controls.Add(_networkBox);
        stack.Controls.Add(_apiBox);
        stack.Controls.Add(_aboutBox);

        scroll.Controls.Add(stack);

        _logBox = new TextBox
        {
            Dock = DockStyle.Bottom,
            Height = 140,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor = Color.FromArgb(20, 20, 20),
            ForeColor = Color.Gainsboro,
            Font = new Font("Consolas", 9.5f),
            BorderStyle = BorderStyle.None,
        };

        Controls.Add(scroll);
        Controls.Add(_logBox);
    }

    private static void AddPathRow(GroupBox host, string label, int y, out TextBox box, out Button btn, EventHandler onOpen)
    {
        var lbl = new Label { Text = label, AutoSize = true, Location = new Point(8, y + 4) };
        box = new TextBox
        {
            Location = new Point(150, y),
            Size = new Size(620, 23),
            ReadOnly = true,
            BackColor = SystemColors.Window,
        };
        btn = new Button { Text = "Open", Location = new Point(778, y - 1), Size = new Size(80, 26) };
        btn.Click += onOpen;
        host.Controls.Add(lbl);
        host.Controls.Add(box);
        host.Controls.Add(btn);
    }
}
