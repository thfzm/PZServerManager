namespace PZServerManager.Forms.Controls;

partial class ConsoleControl
{
    private System.ComponentModel.IContainer components = null;

    private Panel _topPanel = null!;
    private Label _profileLabel = null!;
    private ComboBox _profileBox = null!;
    private Button _startButton = null!;
    private Button _stopButton = null!;
    private Label _statusLabel = null!;

    private TextBox _logBox = null!;

    private Panel _bottomPanel = null!;
    private TextBox _inputBox = null!;
    private Button _sendButton = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        _topPanel = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(8, 6, 8, 6) };
        _profileLabel = new Label { Text = "Profile:", AutoSize = true, Location = new Point(8, 11) };
        _profileBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(60, 8),
            Size = new Size(180, 23),
        };
        _startButton = new Button { Text = "Start", Location = new Point(252, 7), Size = new Size(80, 26) };
        _startButton.Click += OnStart;
        _stopButton = new Button { Text = "Stop", Location = new Point(340, 7), Size = new Size(80, 26) };
        _stopButton.Click += OnStop;
        _statusLabel = new Label
        {
            Text = "Status: Stopped",
            AutoSize = true,
            Location = new Point(436, 11),
            ForeColor = SystemColors.ControlText,
        };
        _topPanel.Controls.Add(_profileLabel);
        _topPanel.Controls.Add(_profileBox);
        _topPanel.Controls.Add(_startButton);
        _topPanel.Controls.Add(_stopButton);
        _topPanel.Controls.Add(_statusLabel);

        _bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 40, Padding = new Padding(8, 6, 8, 6) };
        _sendButton = new Button { Text = "Send", Dock = DockStyle.Right, Width = 80 };
        _sendButton.Click += OnSend;
        _inputBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 10f),
        };
        _inputBox.KeyDown += OnInputKey;
        _bottomPanel.Controls.Add(_inputBox);
        _bottomPanel.Controls.Add(_sendButton);

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
