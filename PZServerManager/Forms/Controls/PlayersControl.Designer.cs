namespace PZServerManager.Forms.Controls;

partial class PlayersControl
{
    private System.ComponentModel.IContainer components = null;

    private Label _bannerLabel = null!;

    // online list
    private Panel _leftPanel = null!;
    private Label _onlineCountLabel = null!;
    private Button _refreshButton = null!;
    private CheckBox _autoRefreshCheck = null!;
    private ListBox _playersList = null!;

    // right side
    private TabControl _rightTabs = null!;

    // online actions
    private Button _kickButton = null!;
    private Button _banSelectedButton = null!;
    private Button _msgSelectedButton = null!;
    private Button _grantAdminSelectedButton = null!;
    private ComboBox _accessLevelBox = null!;
    private Button _setAccessButton = null!;
    private Button _giveItemButton = null!;
    private Button _teleportButton = null!;

    // whitelist
    private TextBox _whitelistInput = null!;
    private Button _whitelistAddButton = null!;
    private Button _whitelistRemoveButton = null!;

    // bans
    private TextBox _banInput = null!;
    private CheckBox _banBySteamId = null!;
    private Button _banAddButton = null!;
    private Button _banRemoveButton = null!;

    // admins
    private TextBox _adminInput = null!;
    private Button _grantAdminButton = null!;
    private Button _removeAdminButton = null!;
    private ComboBox _adminLevelBox = null!;
    private Button _setAccessApplyButton = null!;

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

        _bannerLabel = new Label
        {
            Dock = DockStyle.Top,
            Height = 24,
            Padding = new Padding(8, 4, 8, 0),
            Text = "Not connected",
            ForeColor = Color.OrangeRed,
        };

        // ---- left panel ----
        _leftPanel = new Panel { Dock = DockStyle.Left, Width = 280, Padding = new Padding(8) };
        var leftHeader = new Panel { Dock = DockStyle.Top, Height = 60 };
        _onlineCountLabel = new Label { Text = "Online: 0", AutoSize = true, Location = new Point(0, 6) };
        _refreshButton = new Button { Text = "Refresh", Location = new Point(0, 28), Size = new Size(90, 26) };
        _refreshButton.Click += OnRefresh;
        _autoRefreshCheck = new CheckBox { Text = "Auto", Location = new Point(96, 31), AutoSize = true };
        _autoRefreshCheck.CheckedChanged += OnAutoRefreshChanged;
        leftHeader.Controls.Add(_onlineCountLabel);
        leftHeader.Controls.Add(_refreshButton);
        leftHeader.Controls.Add(_autoRefreshCheck);
        _playersList = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false };
        _leftPanel.Controls.Add(_playersList);
        _leftPanel.Controls.Add(leftHeader);

        // ---- right tabs ----
        _rightTabs = new TabControl { Dock = DockStyle.Fill };

        var onlineTab = new TabPage("Selected actions");
        var onlinePanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), AutoScroll = true };
        _kickButton = new Button { Text = "Kick…", Location = new Point(0, 0), Size = new Size(120, 28) };
        _kickButton.Click += OnKick;
        _banSelectedButton = new Button { Text = "Ban", Location = new Point(130, 0), Size = new Size(120, 28) };
        _banSelectedButton.Click += OnBanSelected;
        _msgSelectedButton = new Button { Text = "Whisper…", Location = new Point(260, 0), Size = new Size(120, 28) };
        _msgSelectedButton.Click += OnMessageSelected;
        _grantAdminSelectedButton = new Button { Text = "Grant admin", Location = new Point(0, 36), Size = new Size(120, 28) };
        _grantAdminSelectedButton.Click += OnGrantAdminSelected;
        _accessLevelBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(130, 38),
            Size = new Size(140, 24),
        };
        _setAccessButton = new Button { Text = "Set access", Location = new Point(280, 36), Size = new Size(100, 28) };
        _setAccessButton.Click += OnSetAccess;
        _giveItemButton = new Button { Text = "Give item…", Location = new Point(0, 72), Size = new Size(120, 28) };
        _giveItemButton.Click += OnGiveItem;
        _teleportButton = new Button { Text = "Teleport…", Location = new Point(130, 72), Size = new Size(120, 28) };
        _teleportButton.Click += OnTeleport;
        onlinePanel.Controls.Add(_kickButton);
        onlinePanel.Controls.Add(_banSelectedButton);
        onlinePanel.Controls.Add(_msgSelectedButton);
        onlinePanel.Controls.Add(_grantAdminSelectedButton);
        onlinePanel.Controls.Add(_accessLevelBox);
        onlinePanel.Controls.Add(_setAccessButton);
        onlinePanel.Controls.Add(_giveItemButton);
        onlinePanel.Controls.Add(_teleportButton);
        onlineTab.Controls.Add(onlinePanel);

        var whitelistTab = new TabPage("Whitelist");
        var wlPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
        var wlLabel = new Label { Text = "Username:", AutoSize = true, Location = new Point(0, 4) };
        _whitelistInput = new TextBox { Location = new Point(80, 0), Size = new Size(220, 23) };
        _whitelistAddButton = new Button { Text = "Add", Location = new Point(0, 36), Size = new Size(120, 28) };
        _whitelistAddButton.Click += OnWhitelistAdd;
        _whitelistRemoveButton = new Button { Text = "Remove", Location = new Point(130, 36), Size = new Size(120, 28) };
        _whitelistRemoveButton.Click += OnWhitelistRemove;
        wlPanel.Controls.Add(wlLabel);
        wlPanel.Controls.Add(_whitelistInput);
        wlPanel.Controls.Add(_whitelistAddButton);
        wlPanel.Controls.Add(_whitelistRemoveButton);
        whitelistTab.Controls.Add(wlPanel);

        var bansTab = new TabPage("Bans");
        var banPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
        var banLabel = new Label { Text = "User / SteamID:", AutoSize = true, Location = new Point(0, 4) };
        _banInput = new TextBox { Location = new Point(110, 0), Size = new Size(220, 23) };
        _banBySteamId = new CheckBox { Text = "by SteamID", Location = new Point(340, 2), AutoSize = true };
        _banAddButton = new Button { Text = "Ban", Location = new Point(0, 36), Size = new Size(120, 28) };
        _banAddButton.Click += OnBanAdd;
        _banRemoveButton = new Button { Text = "Unban", Location = new Point(130, 36), Size = new Size(120, 28) };
        _banRemoveButton.Click += OnBanRemove;
        banPanel.Controls.Add(banLabel);
        banPanel.Controls.Add(_banInput);
        banPanel.Controls.Add(_banBySteamId);
        banPanel.Controls.Add(_banAddButton);
        banPanel.Controls.Add(_banRemoveButton);
        bansTab.Controls.Add(banPanel);

        var adminsTab = new TabPage("Admins");
        var adminPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
        var adminLabel = new Label { Text = "Username:", AutoSize = true, Location = new Point(0, 4) };
        _adminInput = new TextBox { Location = new Point(80, 0), Size = new Size(220, 23) };
        _grantAdminButton = new Button { Text = "Grant admin", Location = new Point(0, 36), Size = new Size(120, 28) };
        _grantAdminButton.Click += OnGrantAdmin;
        _removeAdminButton = new Button { Text = "Remove admin", Location = new Point(130, 36), Size = new Size(140, 28) };
        _removeAdminButton.Click += OnRemoveAdmin;
        var levelLabel = new Label { Text = "Access level:", AutoSize = true, Location = new Point(0, 78) };
        _adminLevelBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(96, 75),
            Size = new Size(140, 24),
        };
        _adminLevelBox.Items.AddRange(new object[]
        {
            "none", "admin", "moderator", "overseer", "gm", "observer",
        });
        _adminLevelBox.SelectedIndex = 0;
        _setAccessApplyButton = new Button { Text = "Apply", Location = new Point(244, 73), Size = new Size(100, 28) };
        _setAccessApplyButton.Click += OnSetAccessApply;
        adminPanel.Controls.Add(adminLabel);
        adminPanel.Controls.Add(_adminInput);
        adminPanel.Controls.Add(_grantAdminButton);
        adminPanel.Controls.Add(_removeAdminButton);
        adminPanel.Controls.Add(levelLabel);
        adminPanel.Controls.Add(_adminLevelBox);
        adminPanel.Controls.Add(_setAccessApplyButton);
        adminsTab.Controls.Add(adminPanel);

        _rightTabs.TabPages.Add(onlineTab);
        _rightTabs.TabPages.Add(whitelistTab);
        _rightTabs.TabPages.Add(bansTab);
        _rightTabs.TabPages.Add(adminsTab);

        // ---- log at bottom ----
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
            WordWrap = false,
            BorderStyle = BorderStyle.None,
        };

        Controls.Add(_rightTabs);
        Controls.Add(_logBox);
        Controls.Add(_leftPanel);
        Controls.Add(_bannerLabel);
    }
}
