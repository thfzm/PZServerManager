namespace PZServerManager.Forms.Controls;

partial class SettingsControl
{
    private System.ComponentModel.IContainer components = null;

    private static readonly Color TextSecondary = Color.FromArgb(110, 112, 120);

    // ---- wizard (shown when not bootstrapped) ----
    private GroupBox _wizardBox = null!;
    private StepCard _card1 = null!;
    private StepBadge _badge1 = null!;
    private TextBox _wizardPathBox = null!;
    private Button _wizardBrowseButton = null!;
    private Button _wizardConfirmButton = null!;
    private StepCard _card2 = null!;
    private StepBadge _badge2 = null!;
    private ProgressBar _wizardProgress2 = null!;
    private Label _wizardStatus2 = null!;
    private Button _wizardDownloadButton = null!;
    private StepCard _card3 = null!;
    private StepBadge _badge3 = null!;
    private ProgressBar _wizardProgress3 = null!;
    private Label _wizardStatus3 = null!;
    private Button _wizardInstallButton = null!;

    // ---- post-bootstrap "complete" banner ----
    private Label _doneBanner = null!;

    // ---- existing settings sections ----
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

        // ---- wizard ----
        _wizardBox = new GroupBox { Text = "초기 설치", Width = 900, Height = 480, Padding = new Padding(12) };

        // Step 1
        _card1 = new StepCard { Location = new Point(12, 28), Size = new Size(860, 110) };
        _badge1 = new StepBadge { Number = 1, Location = new Point(20, 22) };
        var step1Title = new Label
        {
            Text = "설치 경로 선택",
            Font = new Font("Segoe UI Semibold", 11f),
            AutoSize = true,
            Location = new Point(70, 18),
        };
        var step1Desc = new Label
        {
            Text = "SteamCMD와 좀보이드 데디케이티드 서버(~5GB)가 이 폴더 아래에 설치됩니다.",
            ForeColor = TextSecondary,
            AutoSize = true,
            Location = new Point(70, 42),
        };
        _wizardPathBox = new TextBox { Location = new Point(70, 70), Size = new Size(480, 23) };
        _wizardBrowseButton = new Button { Text = "찾아보기…", Location = new Point(556, 69), Size = new Size(96, 26) };
        _wizardBrowseButton.Click += OnWizardBrowse;
        _wizardConfirmButton = new Button { Text = "확인", Location = new Point(660, 69), Size = new Size(180, 26) };
        _wizardConfirmButton.Click += OnWizardConfirmPath;
        _card1.Controls.Add(_badge1);
        _card1.Controls.Add(step1Title);
        _card1.Controls.Add(step1Desc);
        _card1.Controls.Add(_wizardPathBox);
        _card1.Controls.Add(_wizardBrowseButton);
        _card1.Controls.Add(_wizardConfirmButton);

        // Step 2
        _card2 = new StepCard { Location = new Point(12, 148), Size = new Size(860, 130) };
        _badge2 = new StepBadge { Number = 2, Location = new Point(20, 22) };
        var step2Title = new Label
        {
            Text = "SteamCMD 설치",
            Font = new Font("Segoe UI Semibold", 11f),
            AutoSize = true,
            Location = new Point(70, 18),
        };
        var step2Desc = new Label
        {
            Text = "zip 다운로드(~2MB) + 자체 업데이트(~75MB)까지 한 번에 끝냅니다.",
            ForeColor = TextSecondary,
            AutoSize = true,
            Location = new Point(70, 42),
        };
        _wizardProgress2 = new ProgressBar
        {
            Location = new Point(70, 72),
            Size = new Size(580, 16),
            Style = ProgressBarStyle.Continuous,
            Maximum = 100,
        };
        _wizardStatus2 = new Label
        {
            ForeColor = TextSecondary,
            AutoSize = false,
            Size = new Size(580, 18),
            Location = new Point(70, 94),
            Font = new Font("Consolas", 8.5f),
            AutoEllipsis = true,
        };
        _wizardDownloadButton = new Button
        {
            Text = "SteamCMD 설치",
            Location = new Point(660, 70),
            Size = new Size(180, 28),
            FlatStyle = FlatStyle.System,
        };
        _wizardDownloadButton.Click += OnWizardDownloadSteamCmd;
        _card2.Controls.Add(_badge2);
        _card2.Controls.Add(step2Title);
        _card2.Controls.Add(step2Desc);
        _card2.Controls.Add(_wizardProgress2);
        _card2.Controls.Add(_wizardStatus2);
        _card2.Controls.Add(_wizardDownloadButton);

        // Step 3
        _card3 = new StepCard { Location = new Point(12, 288), Size = new Size(860, 150) };
        _badge3 = new StepBadge { Number = 3, Location = new Point(20, 22) };
        var step3Title = new Label
        {
            Text = "좀보이드 데디케이티드 서버 설치",
            Font = new Font("Segoe UI Semibold", 11f),
            AutoSize = true,
            Location = new Point(70, 18),
        };
        var step3Desc = new Label
        {
            Text = "SteamCMD로 app 380870 익명 다운로드 (~5GB).",
            ForeColor = TextSecondary,
            AutoSize = true,
            Location = new Point(70, 42),
        };
        _wizardProgress3 = new ProgressBar
        {
            Location = new Point(70, 72),
            Size = new Size(580, 16),
            Style = ProgressBarStyle.Continuous,
            Maximum = 100,
        };
        _wizardStatus3 = new Label
        {
            ForeColor = TextSecondary,
            AutoSize = false,
            Size = new Size(770, 36),
            Location = new Point(70, 94),
            Font = new Font("Consolas", 8.5f),
            AutoEllipsis = true,
        };
        _wizardInstallButton = new Button
        {
            Text = "서버 설치",
            Location = new Point(660, 70),
            Size = new Size(180, 28),
            FlatStyle = FlatStyle.System,
        };
        _wizardInstallButton.Click += OnWizardInstallServer;
        _card3.Controls.Add(_badge3);
        _card3.Controls.Add(step3Title);
        _card3.Controls.Add(step3Desc);
        _card3.Controls.Add(_wizardProgress3);
        _card3.Controls.Add(_wizardStatus3);
        _card3.Controls.Add(_wizardInstallButton);

        _wizardBox.Controls.Add(_card1);
        _wizardBox.Controls.Add(_card2);
        _wizardBox.Controls.Add(_card3);

        // ---- done banner ----
        _doneBanner = new Label
        {
            Text = "✓ 초기 설치 완료 — 다른 탭에서 서버를 관리하세요.",
            Font = new Font("Segoe UI Semibold", 10.5f),
            ForeColor = Color.FromArgb(46, 160, 67),
            AutoSize = false,
            Width = 900,
            Height = 36,
            Padding = new Padding(8, 8, 0, 0),
            Visible = false,
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
            ForeColor = TextSecondary,
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
            ForeColor = TextSecondary,
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
        _versionLabel = new Label { Text = "", AutoSize = true, Location = new Point(8, 28), ForeColor = TextSecondary };
        _aboutBox.Controls.Add(_versionLabel);

        stack.Controls.Add(_wizardBox);
        stack.Controls.Add(_doneBanner);
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
            Font = new Font("Consolas", 9f),
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
