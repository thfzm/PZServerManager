namespace PZServerManager.Forms.Controls;

partial class ModsControl
{
    private System.ComponentModel.IContainer components = null;

    private Panel _topPanel = null!;
    private Label _profileLabel = null!;
    private ComboBox _profileBox = null!;
    private Label _apiKeyLabel = null!;
    private Button _setApiKeyButton = null!;
    private TextBox _searchBox = null!;
    private Button _searchButton = null!;
    private Button _popularButton = null!;
    private Label _urlLabel = null!;
    private TextBox _urlBox = null!;
    private Button _installByUrlButton = null!;

    private SplitContainer _split = null!;

    // left: installed
    private Label _installedCountLabel = null!;
    private ListView _installedList = null!;
    private Panel _installedButtons = null!;
    private Button _moveUpButton = null!;
    private Button _moveDownButton = null!;
    private Button _openFolderButton = null!;
    private Button _uninstallButton = null!;

    // right: search results
    private Label _resultsCountLabel = null!;
    private ListView _resultsList = null!;
    private Panel _resultsButtons = null!;
    private Button _installSelectedButton = null!;

    // bottom
    private TextBox _detailBox = null!;

    private Panel _bottomPanel = null!;
    private Label _warningsLabel = null!;
    private Button _applyButton = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        // ---- top panel ----
        _topPanel = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(8, 6, 8, 4) };

        _profileLabel = new Label { Text = "Profile:", AutoSize = true, Location = new Point(8, 11) };
        _profileBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(60, 8),
            Size = new Size(160, 23),
        };
        _profileBox.SelectedIndexChanged += OnProfileChanged;

        _apiKeyLabel = new Label
        {
            Text = "API key: ?",
            AutoSize = true,
            Location = new Point(232, 11),
        };
        _setApiKeyButton = new Button { Text = "Set key…", Location = new Point(326, 7), Size = new Size(80, 26) };
        _setApiKeyButton.Click += OnSetApiKey;

        _searchBox = new TextBox
        {
            Location = new Point(416, 8),
            Size = new Size(320, 23),
            PlaceholderText = "Search workshop…",
        };
        _searchBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; OnSearch(s, EventArgs.Empty); } };
        _searchButton = new Button { Text = "Search", Location = new Point(742, 7), Size = new Size(80, 26) };
        _searchButton.Click += OnSearch;
        _popularButton = new Button { Text = "Most popular", Location = new Point(828, 7), Size = new Size(110, 26) };
        _popularButton.Click += OnPopular;

        _urlLabel = new Label { Text = "Install via URL/ID:", AutoSize = true, Location = new Point(8, 47) };
        _urlBox = new TextBox
        {
            Location = new Point(120, 44),
            Size = new Size(640, 23),
            PlaceholderText = "https://steamcommunity.com/sharedfiles/filedetails/?id=… or just the ID",
        };
        _installByUrlButton = new Button { Text = "Install", Location = new Point(766, 43), Size = new Size(80, 26) };
        _installByUrlButton.Click += OnInstallByUrl;

        _topPanel.Controls.Add(_profileLabel);
        _topPanel.Controls.Add(_profileBox);
        _topPanel.Controls.Add(_apiKeyLabel);
        _topPanel.Controls.Add(_setApiKeyButton);
        _topPanel.Controls.Add(_searchBox);
        _topPanel.Controls.Add(_searchButton);
        _topPanel.Controls.Add(_popularButton);
        _topPanel.Controls.Add(_urlLabel);
        _topPanel.Controls.Add(_urlBox);
        _topPanel.Controls.Add(_installByUrlButton);

        // ---- bottom panel ----
        _bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 44, Padding = new Padding(8, 6, 8, 6) };
        _warningsLabel = new Label
        {
            Text = "",
            Dock = DockStyle.Fill,
            ForeColor = SystemColors.GrayText,
            TextAlign = ContentAlignment.MiddleLeft,
        };
        _applyButton = new Button { Text = "Apply to ini", Dock = DockStyle.Right, Width = 140 };
        _applyButton.Click += OnApply;
        _bottomPanel.Controls.Add(_warningsLabel);
        _bottomPanel.Controls.Add(_applyButton);

        // ---- detail box ----
        _detailBox = new TextBox
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

        // ---- split ----
        _split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 480,
            Panel1MinSize = 250,
            Panel2MinSize = 250,
        };

        // -- left (installed) --
        var leftRoot = new Panel { Dock = DockStyle.Fill };
        _installedCountLabel = new Label
        {
            Text = "Installed: 0",
            Dock = DockStyle.Top,
            Height = 22,
            Padding = new Padding(8, 4, 8, 0),
        };
        _installedList = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            CheckBoxes = true,
            FullRowSelect = true,
            HideSelection = false,
            GridLines = false,
        };
        _installedList.Columns.Add("Name", 200);
        _installedList.Columns.Add("Mod id", 130);
        _installedList.Columns.Add("Workshop id", 110);
        _installedList.Columns.Add("Maps", 100);
        _installedList.SelectedIndexChanged += OnInstalledSelected;
        _installedList.ItemChecked += OnInstalledChecked;

        _installedButtons = new Panel { Dock = DockStyle.Bottom, Height = 36, Padding = new Padding(8, 4, 8, 4) };
        _moveUpButton = new Button { Text = "↑", Location = new Point(0, 0), Size = new Size(40, 26) };
        _moveUpButton.Click += OnMoveUp;
        _moveDownButton = new Button { Text = "↓", Location = new Point(46, 0), Size = new Size(40, 26) };
        _moveDownButton.Click += OnMoveDown;
        _openFolderButton = new Button { Text = "Open folder", Location = new Point(96, 0), Size = new Size(110, 26) };
        _openFolderButton.Click += OnOpenFolder;
        _uninstallButton = new Button { Text = "Uninstall…", Location = new Point(212, 0), Size = new Size(100, 26) };
        _uninstallButton.Click += OnUninstall;
        _installedButtons.Controls.Add(_moveUpButton);
        _installedButtons.Controls.Add(_moveDownButton);
        _installedButtons.Controls.Add(_openFolderButton);
        _installedButtons.Controls.Add(_uninstallButton);

        leftRoot.Controls.Add(_installedList);
        leftRoot.Controls.Add(_installedButtons);
        leftRoot.Controls.Add(_installedCountLabel);
        _split.Panel1.Controls.Add(leftRoot);

        // -- right (search) --
        var rightRoot = new Panel { Dock = DockStyle.Fill };
        _resultsCountLabel = new Label
        {
            Text = "Workshop:",
            Dock = DockStyle.Top,
            Height = 22,
            Padding = new Padding(8, 4, 8, 0),
        };
        _resultsList = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            HideSelection = false,
            GridLines = false,
        };
        _resultsList.Columns.Add("Title", 260);
        _resultsList.Columns.Add("Subs", 80);
        _resultsList.Columns.Add("Score", 60);
        _resultsList.Columns.Add("Updated", 90);
        _resultsList.Columns.Add("Status", 90);
        _resultsList.SelectedIndexChanged += OnResultSelected;
        _resultsList.DoubleClick += OnResultDoubleClick;

        _resultsButtons = new Panel { Dock = DockStyle.Bottom, Height = 36, Padding = new Padding(8, 4, 8, 4) };
        _installSelectedButton = new Button { Text = "Install / Update", Location = new Point(0, 0), Size = new Size(140, 26) };
        _installSelectedButton.Click += OnInstallSelected;
        _resultsButtons.Controls.Add(_installSelectedButton);

        rightRoot.Controls.Add(_resultsList);
        rightRoot.Controls.Add(_resultsButtons);
        rightRoot.Controls.Add(_resultsCountLabel);
        _split.Panel2.Controls.Add(rightRoot);

        Controls.Add(_split);
        Controls.Add(_detailBox);
        Controls.Add(_bottomPanel);
        Controls.Add(_topPanel);
    }
}
