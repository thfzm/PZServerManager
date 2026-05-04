namespace PZServerManager.Tabs;

partial class ConsoleControl
{
    private System.ComponentModel.IContainer components = null;

    private TextBox _logBox = null!;
    private Panel _bottomPanel = null!;
    private TextBox _stdinBox = null!;
    private Button _sendButton = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

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

        _bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 40, Padding = new Padding(8, 6, 8, 8) };
        var row = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Font = new Font("Consolas", 10f),
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        row.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        _stdinBox = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 6, 0), Font = new Font("Consolas", 10f) };
        _stdinBox.KeyDown += OnStdinKey;
        _sendButton = new Button { Text = "Send", Dock = DockStyle.Fill };
        _sendButton.Click += OnSend;

        row.Controls.Add(_stdinBox, 0, 0);
        row.Controls.Add(_sendButton, 1, 0);
        _bottomPanel.Controls.Add(row);

        Controls.Add(_logBox);
        Controls.Add(_bottomPanel);
    }
}
