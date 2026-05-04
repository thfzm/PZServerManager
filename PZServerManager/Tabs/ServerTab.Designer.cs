namespace PZServerManager.Tabs;

partial class ServerTab
{
    private System.ComponentModel.IContainer components = null;

    private Panel _topPanel = null!;
    private Label _profileLabel = null!;
    private ComboBox _profileBox = null!;
    private Button _startButton = null!;
    private Button _stopButton = null!;
    private Label _statusLabel = null!;

    private TabControl _subTabs = null!;
    private TabPage _consoleTab = null!;
    private ConsoleControl _consoleSubTab = null!;
    private TabPage _rconTab = null!;
    private RconControl _rconSubTab = null!;
    private TabPage _playersTab = null!;
    private PlayersControl _playersSubTab = null!;
    private TabPage _savesTab = null!;
    private SavesControl _savesSubTab = null!;
    private TabPage _logsTab = null!;
    private LogsControl _logsSubTab = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        // ---- top toolbar (Profile + Start/Stop + Status) ----
        _topPanel = new Panel { Dock = DockStyle.Top, Height = 44, Padding = new Padding(12, 8, 12, 6) };
        var bar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 5,
            RowCount = 1,
            Font = new Font("Segoe UI", 9.5f),
        };
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        bar.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        _profileLabel = new Label { Text = "Profile:", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(0, 7, 8, 0) };
        _profileBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 4, 8, 4),
        };
        _startButton = new Button { Text = "Start", Dock = DockStyle.Fill, Margin = new Padding(0, 2, 4, 2) };
        _startButton.Click += OnStart;
        _stopButton = new Button { Text = "Stop", Dock = DockStyle.Fill, Margin = new Padding(0, 2, 4, 2) };
        _stopButton.Click += OnStop;
        _statusLabel = new Label { Text = "Status: Stopped", Anchor = AnchorStyles.Left, AutoSize = true, Margin = new Padding(8, 7, 0, 0), ForeColor = SystemColors.GrayText };

        bar.Controls.Add(_profileLabel, 0, 0);
        bar.Controls.Add(_profileBox, 1, 0);
        bar.Controls.Add(_startButton, 2, 0);
        bar.Controls.Add(_stopButton, 3, 0);
        bar.Controls.Add(_statusLabel, 4, 0);
        _topPanel.Controls.Add(bar);

        // ---- inner sub-tabs ----
        _subTabs = new TabControl { Dock = DockStyle.Fill };

        _consoleTab = new TabPage("Console");
        _consoleSubTab = new ConsoleControl { Dock = DockStyle.Fill };
        _consoleTab.Controls.Add(_consoleSubTab);

        _rconTab = new TabPage("RCON");
        _rconSubTab = new RconControl { Dock = DockStyle.Fill };
        _rconTab.Controls.Add(_rconSubTab);

        _playersTab = new TabPage("Players");
        _playersSubTab = new PlayersControl { Dock = DockStyle.Fill };
        _playersTab.Controls.Add(_playersSubTab);

        _savesTab = new TabPage("Saves");
        _savesSubTab = new SavesControl { Dock = DockStyle.Fill };
        _savesTab.Controls.Add(_savesSubTab);

        _logsTab = new TabPage("Logs");
        _logsSubTab = new LogsControl { Dock = DockStyle.Fill };
        _logsTab.Controls.Add(_logsSubTab);

        _subTabs.TabPages.Add(_consoleTab);
        _subTabs.TabPages.Add(_rconTab);
        _subTabs.TabPages.Add(_playersTab);
        _subTabs.TabPages.Add(_savesTab);
        _subTabs.TabPages.Add(_logsTab);

        Controls.Add(_subTabs);
        Controls.Add(_topPanel);
    }
}
