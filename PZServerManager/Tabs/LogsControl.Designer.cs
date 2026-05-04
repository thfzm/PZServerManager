namespace PZServerManager.Tabs;

partial class LogsControl
{
    private System.ComponentModel.IContainer components = null;

    private SplitContainer _split = null!;
    private Panel _topPanel = null!;
    private Button _refreshButton = null!;
    private Button _openFolderButton = null!;
    private CheckBox _tailCheck = null!;
    private Label _statusLabel = null!;

    private ListView _filesList = null!;
    private TextBox _logBox = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        _topPanel = new Panel { Dock = DockStyle.Top, Height = 36, Padding = new Padding(8, 6, 8, 4) };
        _refreshButton = new Button { Text = "Refresh", Location = new Point(0, 0), Size = new Size(80, 26) };
        _refreshButton.Click += OnRefresh;
        _openFolderButton = new Button { Text = "Open folder", Location = new Point(86, 0), Size = new Size(110, 26) };
        _openFolderButton.Click += OnOpenFolder;
        _tailCheck = new CheckBox { Text = "Tail (auto-refresh)", Location = new Point(206, 4), AutoSize = true };
        _tailCheck.CheckedChanged += OnTailToggle;
        _statusLabel = new Label
        {
            Text = "",
            AutoSize = true,
            Location = new Point(360, 6),
            ForeColor = SystemColors.GrayText,
        };
        _topPanel.Controls.Add(_refreshButton);
        _topPanel.Controls.Add(_openFolderButton);
        _topPanel.Controls.Add(_tailCheck);
        _topPanel.Controls.Add(_statusLabel);

        _split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 360,
            Panel1MinSize = 250,
            Panel2MinSize = 300,
        };

        _filesList = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            HideSelection = false,
            GridLines = false,
        };
        _filesList.Columns.Add("File", 200);
        _filesList.Columns.Add("Size", 80);
        _filesList.Columns.Add("Modified", 130);
        _filesList.SelectedIndexChanged += OnFileSelected;
        _split.Panel1.Controls.Add(_filesList);

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
        _split.Panel2.Controls.Add(_logBox);

        Controls.Add(_split);
        Controls.Add(_topPanel);
    }
}
