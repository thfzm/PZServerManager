namespace PZServerManager.Forms;

partial class FirstRunForm
{
    private System.ComponentModel.IContainer components = null;

    private Label _titleLabel = null!;
    private Label _pathLabel = null!;
    private TextBox _installRootBox = null!;
    private Button _browseButton = null!;
    private TextBox _logBox = null!;
    private Button _installButton = null!;
    private Button _cancelButton = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        _titleLabel = new Label
        {
            Text = "First-time setup — install SteamCMD and the Project Zomboid dedicated server.",
            AutoSize = true,
            Location = new Point(12, 12),
        };

        _pathLabel = new Label
        {
            Text = "Install location:",
            AutoSize = true,
            Location = new Point(12, 48),
        };

        _installRootBox = new TextBox
        {
            Location = new Point(118, 45),
            Size = new Size(540, 23),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
        };

        _browseButton = new Button
        {
            Text = "Browse...",
            Location = new Point(664, 44),
            Size = new Size(96, 26),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
        };
        _browseButton.Click += OnBrowse;

        _logBox = new TextBox
        {
            Location = new Point(12, 84),
            Size = new Size(748, 380),
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Consolas", 9f),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            BackColor = SystemColors.Window,
        };

        _cancelButton = new Button
        {
            Text = "Cancel",
            Location = new Point(568, 478),
            Size = new Size(96, 28),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
        };
        _cancelButton.Click += OnCancel;

        _installButton = new Button
        {
            Text = "Install",
            Location = new Point(672, 478),
            Size = new Size(96, 28),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
        };
        _installButton.Click += OnInstall;

        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(776, 520);
        Controls.Add(_titleLabel);
        Controls.Add(_pathLabel);
        Controls.Add(_installRootBox);
        Controls.Add(_browseButton);
        Controls.Add(_logBox);
        Controls.Add(_cancelButton);
        Controls.Add(_installButton);
        MinimumSize = new Size(640, 420);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "PZ Server Manager — Setup";
        AcceptButton = _installButton;
    }
}
