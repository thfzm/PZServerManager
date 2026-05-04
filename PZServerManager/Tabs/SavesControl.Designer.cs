namespace PZServerManager.Tabs;

partial class SavesControl
{
    private System.ComponentModel.IContainer components = null;

    private Panel _topPanel = null!;
    private Label _backupDirLabel = null!;
    private TextBox _backupDirBox = null!;
    private Button _browseDirButton = null!;
    private Button _openBackupDirButton = null!;
    private Button _openSavesDirButton = null!;

    private ListView _savesList = null!;

    private Panel _actionPanel = null!;
    private Button _refreshButton = null!;
    private Button _backupButton = null!;
    private Button _restoreButton = null!;
    private Button _deleteButton = null!;

    private GroupBox _scheduleBox = null!;
    private CheckBox _autoBackupCheck = null!;
    private NumericUpDown _autoBackupHours = null!;
    private CheckBox _autoRestartCheck = null!;
    private NumericUpDown _autoRestartHours = null!;
    private CheckBox _crashRestartCheck = null!;
    private Label _retentionLabel = null!;
    private NumericUpDown _retentionBox = null!;
    private Button _saveScheduleButton = null!;

    private Panel _statusPanel = null!;
    private Label _statusLabel = null!;
    private TextBox _logBox = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        _topPanel = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(8, 8, 8, 4) };
        _backupDirLabel = new Label { Text = "백업 폴더:", AutoSize = true, Location = new Point(0, 7) };
        _backupDirBox = new TextBox
        {
            Location = new Point(86, 4),
            Size = new Size(420, 23),
            ReadOnly = true,
            BackColor = SystemColors.Window,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            Width = 420,
        };
        _browseDirButton = new Button { Text = "변경…", Location = new Point(514, 3), Size = new Size(80, 26) };
        _browseDirButton.Click += OnBrowseDir;
        _openBackupDirButton = new Button { Text = "백업 폴더 열기", Location = new Point(600, 3), Size = new Size(110, 26) };
        _openBackupDirButton.Click += OnOpenBackupDir;
        _openSavesDirButton = new Button { Text = "세이브 폴더 열기", Location = new Point(716, 3), Size = new Size(120, 26) };
        _openSavesDirButton.Click += OnOpenSavesDir;
        _topPanel.Controls.Add(_backupDirLabel);
        _topPanel.Controls.Add(_backupDirBox);
        _topPanel.Controls.Add(_browseDirButton);
        _topPanel.Controls.Add(_openBackupDirButton);
        _topPanel.Controls.Add(_openSavesDirButton);

        _savesList = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            HideSelection = false,
            GridLines = false,
        };
        _savesList.Columns.Add("Name", 220);
        _savesList.Columns.Add("Size", 100);
        _savesList.Columns.Add("Last modified", 140);
        _savesList.Columns.Add("Backups", 70);

        _actionPanel = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(8, 8, 8, 4) };
        _refreshButton = new Button { Text = "Refresh", Location = new Point(0, 0), Size = new Size(80, 26) };
        _refreshButton.Click += OnRefresh;
        _backupButton = new Button { Text = "지금 백업", Location = new Point(86, 0), Size = new Size(110, 26) };
        _backupButton.Click += OnBackup;
        _restoreButton = new Button { Text = "복원…", Location = new Point(202, 0), Size = new Size(100, 26) };
        _restoreButton.Click += OnRestore;
        _deleteButton = new Button { Text = "삭제…", Location = new Point(308, 0), Size = new Size(100, 26) };
        _deleteButton.Click += OnDelete;
        _actionPanel.Controls.Add(_refreshButton);
        _actionPanel.Controls.Add(_backupButton);
        _actionPanel.Controls.Add(_restoreButton);
        _actionPanel.Controls.Add(_deleteButton);

        _scheduleBox = new GroupBox
        {
            Text = "스케줄",
            Dock = DockStyle.Top,
            Height = 130,
            Padding = new Padding(8),
        };
        _autoBackupCheck = new CheckBox { Text = "Auto backup every", Location = new Point(12, 24), AutoSize = true };
        _autoBackupHours = new NumericUpDown
        {
            Location = new Point(160, 22),
            Size = new Size(70, 23),
            Minimum = 0.25m,
            Maximum = 720m,
            DecimalPlaces = 2,
            Increment = 0.5m,
            Value = 4m,
        };
        var lbl1 = new Label { Text = "hours", AutoSize = true, Location = new Point(238, 26) };
        _autoRestartCheck = new CheckBox { Text = "Auto restart every", Location = new Point(12, 54), AutoSize = true };
        _autoRestartHours = new NumericUpDown
        {
            Location = new Point(160, 52),
            Size = new Size(70, 23),
            Minimum = 0.25m,
            Maximum = 720m,
            DecimalPlaces = 2,
            Increment = 1m,
            Value = 24m,
        };
        var lbl2 = new Label { Text = "hours", AutoSize = true, Location = new Point(238, 56) };
        _crashRestartCheck = new CheckBox { Text = "Auto restart on crash", Location = new Point(12, 84), AutoSize = true };
        _retentionLabel = new Label { Text = "Keep last N backups:", AutoSize = true, Location = new Point(360, 26) };
        _retentionBox = new NumericUpDown
        {
            Location = new Point(490, 22),
            Size = new Size(70, 23),
            Minimum = 0,
            Maximum = 100,
            Value = 5,
        };
        _saveScheduleButton = new Button { Text = "스케줄 저장", Location = new Point(360, 84), Size = new Size(140, 28) };
        _saveScheduleButton.Click += OnSaveSchedule;
        _scheduleBox.Controls.Add(_autoBackupCheck);
        _scheduleBox.Controls.Add(_autoBackupHours);
        _scheduleBox.Controls.Add(lbl1);
        _scheduleBox.Controls.Add(_autoRestartCheck);
        _scheduleBox.Controls.Add(_autoRestartHours);
        _scheduleBox.Controls.Add(lbl2);
        _scheduleBox.Controls.Add(_crashRestartCheck);
        _scheduleBox.Controls.Add(_retentionLabel);
        _scheduleBox.Controls.Add(_retentionBox);
        _scheduleBox.Controls.Add(_saveScheduleButton);

        _statusPanel = new Panel { Dock = DockStyle.Bottom, Height = 24, Padding = new Padding(8, 4, 8, 4) };
        _statusLabel = new Label
        {
            Text = "",
            Dock = DockStyle.Fill,
            ForeColor = SystemColors.GrayText,
            TextAlign = ContentAlignment.MiddleLeft,
        };
        _statusPanel.Controls.Add(_statusLabel);

        _logBox = new TextBox
        {
            Dock = DockStyle.Bottom,
            Height = 110,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor = Color.FromArgb(20, 20, 20),
            ForeColor = Color.Gainsboro,
            Font = new Font("Consolas", 9.5f),
            BorderStyle = BorderStyle.None,
        };

        Controls.Add(_savesList);
        Controls.Add(_actionPanel);
        Controls.Add(_scheduleBox);
        Controls.Add(_statusPanel);
        Controls.Add(_logBox);
        Controls.Add(_topPanel);
    }
}
