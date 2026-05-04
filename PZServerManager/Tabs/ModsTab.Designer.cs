namespace PZServerManager.Tabs;

partial class ModsTab
{
    private System.ComponentModel.IContainer components = null;

    // top toolbar
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

    // installed (left)
    private Label _installedCountLabel = null!;
    private ListView _installedList = null!;
    private Button _moveUpButton = null!;
    private Button _moveDownButton = null!;
    private Button _openFolderButton = null!;
    private Button _uninstallButton = null!;

    // search (right)
    private Label _resultsCountLabel = null!;
    private ListView _resultsList = null!;
    private Button _installSelectedButton = null!;

    // bottom apply
    private Panel _applyPanel = null!;
    private Label _warningsLabel = null!;
    private Button _applyButton = null!;

    private TextBox _detailBox = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        // ---- top toolbar (two rows) ----
        _topPanel = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(12, 8, 12, 6) };
        var topGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 9,
            RowCount = 2,
            Font = new Font("Segoe UI", 9.5f),
        };
        topGrid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));         // "Profile:" label
        topGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));    // profile combo
        topGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));    // API key label
        topGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));     // Set key button
        topGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));    // search box
        topGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));     // search button
        topGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));    // popular button
        topGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 0));      // (unused col 7)
        topGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 0));      // (unused col 8)
        topGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
        topGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

        _profileLabel = new Label { Text = "Profile:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(0, 7, 6, 0) };
        _profileBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill, Margin = new Padding(0, 4, 8, 4) };
        _profileBox.SelectedIndexChanged += OnProfileChanged;
        _apiKeyLabel = new Label { Text = "API key: ?", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(0, 7, 6, 0) };
        _setApiKeyButton = new Button { Text = "Set key…", Dock = DockStyle.Fill, Margin = new Padding(0, 2, 8, 2) };
        _setApiKeyButton.Click += OnSetApiKey;
        _searchBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 4, 8, 4),
            PlaceholderText = "Search workshop…",
        };
        _searchBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; OnSearch(s, EventArgs.Empty); } };
        _searchButton = new Button { Text = "Search", Dock = DockStyle.Fill, Margin = new Padding(0, 2, 4, 2) };
        _searchButton.Click += OnSearch;
        _popularButton = new Button { Text = "Most popular", Dock = DockStyle.Fill, Margin = new Padding(0, 2, 0, 2) };
        _popularButton.Click += OnPopular;

        topGrid.Controls.Add(_profileLabel, 0, 0);
        topGrid.Controls.Add(_profileBox, 1, 0);
        topGrid.Controls.Add(_apiKeyLabel, 2, 0);
        topGrid.Controls.Add(_setApiKeyButton, 3, 0);
        topGrid.Controls.Add(_searchBox, 4, 0);
        topGrid.Controls.Add(_searchButton, 5, 0);
        topGrid.Controls.Add(_popularButton, 6, 0);

        _urlLabel = new Label { Text = "Install via URL/ID:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(0, 7, 6, 0) };
        _urlBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 4, 8, 4),
            PlaceholderText = "https://steamcommunity.com/.../?id=… or just the ID",
        };
        _installByUrlButton = new Button { Text = "Install", Dock = DockStyle.Fill, Margin = new Padding(0, 2, 0, 2) };
        _installByUrlButton.Click += OnInstallByUrl;

        topGrid.Controls.Add(_urlLabel, 0, 1);
        topGrid.SetColumnSpan(_urlLabel, 1);
        topGrid.Controls.Add(_urlBox, 1, 1);
        topGrid.SetColumnSpan(_urlBox, 5);
        topGrid.Controls.Add(_installByUrlButton, 6, 1);

        _topPanel.Controls.Add(topGrid);

        // ---- bottom apply panel ----
        _applyPanel = new Panel { Dock = DockStyle.Bottom, Height = 44, Padding = new Padding(12, 6, 12, 12) };
        _warningsLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = "",
            ForeColor = SystemColors.GrayText,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true,
        };
        _applyButton = new Button { Text = "Apply to ini", Dock = DockStyle.Right, Width = 140, Height = 28 };
        _applyButton.Click += OnApply;
        _applyPanel.Controls.Add(_warningsLabel);
        _applyPanel.Controls.Add(_applyButton);

        // ---- detail box (above apply panel) ----
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
            WordWrap = false,
        };

        // ---- center split: installed | search results ----
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 540,
            Panel1MinSize = 320,
            Panel2MinSize = 320,
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
        _installedList.Columns.Add("Name", 220);
        _installedList.Columns.Add("Mod id", 130);
        _installedList.Columns.Add("Workshop id", 110);
        _installedList.Columns.Add("Maps", 80);
        _installedList.SelectedIndexChanged += OnInstalledSelected;
        _installedList.ItemChecked += OnInstalledChecked;

        var leftButtons = new Panel { Dock = DockStyle.Bottom, Height = 36, Padding = new Padding(8, 4, 8, 4) };
        _moveUpButton = new Button { Text = "↑", Location = new Point(0, 0), Size = new Size(40, 26) };
        _moveUpButton.Click += OnMoveUp;
        _moveDownButton = new Button { Text = "↓", Location = new Point(46, 0), Size = new Size(40, 26) };
        _moveDownButton.Click += OnMoveDown;
        _openFolderButton = new Button { Text = "Open folder", Location = new Point(96, 0), Size = new Size(110, 26) };
        _openFolderButton.Click += OnOpenFolder;
        _uninstallButton = new Button { Text = "Uninstall…", Location = new Point(212, 0), Size = new Size(100, 26) };
        _uninstallButton.Click += OnUninstall;
        leftButtons.Controls.Add(_moveUpButton);
        leftButtons.Controls.Add(_moveDownButton);
        leftButtons.Controls.Add(_openFolderButton);
        leftButtons.Controls.Add(_uninstallButton);

        leftRoot.Controls.Add(_installedList);
        leftRoot.Controls.Add(leftButtons);
        leftRoot.Controls.Add(_installedCountLabel);
        split.Panel1.Controls.Add(leftRoot);

        // -- right (search results) --
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
        _resultsList.Columns.Add("Title", 240);
        _resultsList.Columns.Add("Subs", 80);
        _resultsList.Columns.Add("Score", 60);
        _resultsList.Columns.Add("Updated", 90);
        _resultsList.Columns.Add("Status", 90);
        _resultsList.SelectedIndexChanged += OnResultSelected;
        _resultsList.DoubleClick += OnResultDoubleClick;

        var rightButtons = new Panel { Dock = DockStyle.Bottom, Height = 36, Padding = new Padding(8, 4, 8, 4) };
        _installSelectedButton = new Button { Text = "Install / Update", Location = new Point(0, 0), Size = new Size(140, 26) };
        _installSelectedButton.Click += OnInstallSelected;
        rightButtons.Controls.Add(_installSelectedButton);

        rightRoot.Controls.Add(_resultsList);
        rightRoot.Controls.Add(rightButtons);
        rightRoot.Controls.Add(_resultsCountLabel);
        split.Panel2.Controls.Add(rightRoot);

        // Order matters for Dock — last added gets first crack at the layout area.
        Controls.Add(split);          // Fill — what's left after others
        Controls.Add(_applyPanel);    // Bottom 44 (added first → last processed)
        Controls.Add(_detailBox);     // Bottom 140
        Controls.Add(_topPanel);      // Top 80
    }
}
