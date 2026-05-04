using PZServerManager.Forms.Controls;

namespace PZServerManager.Forms;

partial class FirstRunForm
{
    private System.ComponentModel.IContainer components = null;

    private static readonly Color BgColor = Color.FromArgb(247, 247, 250);
    private static readonly Color TextPrimary = Color.FromArgb(28, 28, 32);
    private static readonly Color TextSecondary = Color.FromArgb(110, 112, 120);
    private static readonly Color Accent = Color.FromArgb(74, 122, 250);

    private Label _titleLabel = null!;
    private Label _subtitleLabel = null!;

    // Step 1: path
    private StepCard _card1 = null!;
    private StepBadge _badge1 = null!;
    private Label _step1Title = null!;
    private Label _step1Description = null!;
    private TextBox _pathBox = null!;
    private Button _browseButton = null!;
    private Button _confirmPathButton = null!;

    // Step 2: SteamCMD
    private StepCard _card2 = null!;
    private StepBadge _badge2 = null!;
    private Label _step2Title = null!;
    private Label _step2Description = null!;
    private ProgressBar _progress2 = null!;
    private Label _status2 = null!;
    private Button _downloadButton = null!;

    // Step 3: PZ server
    private StepCard _card3 = null!;
    private StepBadge _badge3 = null!;
    private Label _step3Title = null!;
    private Label _step3Description = null!;
    private ProgressBar _progress3 = null!;
    private Label _status3 = null!;
    private Button _installButton = null!;

    // footer
    private Panel _footer = null!;
    private CheckBox _showLogToggle = null!;
    private Button _cancelButton = null!;
    private Button _continueButton = null!;
    private TextBox _fullLogBox = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        Font = new Font("Segoe UI", 9.5f);
        BackColor = BgColor;
        ForeColor = TextPrimary;
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(720, 720);
        MinimumSize = new Size(640, 600);
        Text = "PZ Server Manager — Setup";
        StartPosition = FormStartPosition.CenterScreen;

        // ---- title ----
        _titleLabel = new Label
        {
            Text = "초기 설정",
            Font = new Font("Segoe UI Semibold", 18f),
            ForeColor = TextPrimary,
            AutoSize = true,
            Location = new Point(28, 22),
        };
        _subtitleLabel = new Label
        {
            Text = "세 단계로 설치합니다 — 각 단계가 끝나야 다음 버튼이 활성화됩니다.",
            ForeColor = TextSecondary,
            AutoSize = true,
            Location = new Point(30, 60),
        };

        // ---- step 1 ----
        _card1 = NewCard(new Point(28, 96), 664, 116);
        _badge1 = new StepBadge { Number = 1, Location = new Point(20, 22) };
        _step1Title = new Label
        {
            Text = "설치 경로 선택",
            Font = new Font("Segoe UI Semibold", 11f),
            ForeColor = TextPrimary,
            AutoSize = true,
            Location = new Point(70, 18),
        };
        _step1Description = new Label
        {
            Text = "SteamCMD와 좀보이드 데디케이티드 서버(~5GB)가 이 폴더 아래에 설치됩니다.",
            ForeColor = TextSecondary,
            AutoSize = true,
            Location = new Point(70, 42),
        };
        _pathBox = new TextBox
        {
            Location = new Point(70, 70),
            Size = new Size(380, 23),
        };
        _browseButton = new Button
        {
            Text = "찾아보기…",
            Location = new Point(456, 69),
            Size = new Size(80, 26),
            FlatStyle = FlatStyle.System,
        };
        _browseButton.Click += OnBrowse;
        _confirmPathButton = new Button
        {
            Text = "확인",
            Location = new Point(548, 69),
            Size = new Size(96, 26),
            FlatStyle = FlatStyle.System,
        };
        _confirmPathButton.Click += OnConfirmPath;
        _card1.Controls.Add(_badge1);
        _card1.Controls.Add(_step1Title);
        _card1.Controls.Add(_step1Description);
        _card1.Controls.Add(_pathBox);
        _card1.Controls.Add(_browseButton);
        _card1.Controls.Add(_confirmPathButton);

        // ---- step 2 ----
        _card2 = NewCard(new Point(28, 224), 664, 130);
        _badge2 = new StepBadge { Number = 2, Location = new Point(20, 22) };
        _step2Title = new Label
        {
            Text = "SteamCMD 설치",
            Font = new Font("Segoe UI Semibold", 11f),
            ForeColor = TextPrimary,
            AutoSize = true,
            Location = new Point(70, 18),
        };
        _step2Description = new Label
        {
            Text = "zip 다운로드(~2MB) + 자체 업데이트(~75MB)까지 한 번에 끝냅니다.",
            ForeColor = TextSecondary,
            AutoSize = true,
            Location = new Point(70, 42),
        };
        _progress2 = new ProgressBar
        {
            Location = new Point(70, 72),
            Size = new Size(466, 16),
            Style = ProgressBarStyle.Continuous,
            Maximum = 100,
        };
        _status2 = new Label
        {
            Text = "",
            ForeColor = TextSecondary,
            AutoSize = false,
            Size = new Size(466, 18),
            Location = new Point(70, 94),
            Font = new Font("Consolas", 8.5f),
            AutoEllipsis = true,
        };
        _downloadButton = new Button
        {
            Text = "SteamCMD 설치",
            Location = new Point(528, 70),
            Size = new Size(116, 28),
            FlatStyle = FlatStyle.System,
        };
        _downloadButton.Click += OnDownloadSteamCmd;
        _card2.Controls.Add(_badge2);
        _card2.Controls.Add(_step2Title);
        _card2.Controls.Add(_step2Description);
        _card2.Controls.Add(_progress2);
        _card2.Controls.Add(_status2);
        _card2.Controls.Add(_downloadButton);

        // ---- step 3 ----
        _card3 = NewCard(new Point(28, 366), 664, 150);
        _badge3 = new StepBadge { Number = 3, Location = new Point(20, 22) };
        _step3Title = new Label
        {
            Text = "좀보이드 데디케이티드 서버 설치",
            Font = new Font("Segoe UI Semibold", 11f),
            ForeColor = TextPrimary,
            AutoSize = true,
            Location = new Point(70, 18),
        };
        _step3Description = new Label
        {
            Text = "SteamCMD로 app 380870 익명 다운로드 (~5GB).",
            ForeColor = TextSecondary,
            AutoSize = true,
            Location = new Point(70, 42),
        };
        _progress3 = new ProgressBar
        {
            Location = new Point(70, 72),
            Size = new Size(466, 16),
            Style = ProgressBarStyle.Continuous,
            Maximum = 100,
        };
        _status3 = new Label
        {
            Text = "",
            ForeColor = TextSecondary,
            AutoSize = false,
            Size = new Size(574, 36),
            Location = new Point(70, 94),
            Font = new Font("Consolas", 8.5f),
            AutoEllipsis = true,
        };
        _installButton = new Button
        {
            Text = "서버 설치",
            Location = new Point(528, 70),
            Size = new Size(116, 28),
            FlatStyle = FlatStyle.System,
        };
        _installButton.Click += OnInstallServer;
        _card3.Controls.Add(_badge3);
        _card3.Controls.Add(_step3Title);
        _card3.Controls.Add(_step3Description);
        _card3.Controls.Add(_progress3);
        _card3.Controls.Add(_status3);
        _card3.Controls.Add(_installButton);

        // ---- footer ----
        _footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 56,
            BackColor = BgColor,
            Padding = new Padding(28, 12, 28, 12),
        };
        _showLogToggle = new CheckBox
        {
            Text = "전체 로그 보기",
            Anchor = AnchorStyles.Left,
            Location = new Point(0, 6),
            AutoSize = true,
            ForeColor = TextSecondary,
        };
        _showLogToggle.CheckedChanged += OnToggleFullLog;

        _cancelButton = new Button
        {
            Text = "취소",
            Anchor = AnchorStyles.Right,
            Location = new Point(_footer.ClientSize.Width - 230, 4),
            Size = new Size(100, 30),
            FlatStyle = FlatStyle.System,
        };
        _cancelButton.Click += OnCancel;
        _continueButton = new Button
        {
            Text = "계속",
            Anchor = AnchorStyles.Right,
            Location = new Point(_footer.ClientSize.Width - 120, 4),
            Size = new Size(100, 30),
            FlatStyle = FlatStyle.System,
        };
        _continueButton.Click += OnContinue;
        _footer.Controls.Add(_showLogToggle);
        _footer.Controls.Add(_cancelButton);
        _footer.Controls.Add(_continueButton);

        _fullLogBox = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor = Color.FromArgb(20, 20, 20),
            ForeColor = Color.Gainsboro,
            Font = new Font("Consolas", 9f),
            BorderStyle = BorderStyle.None,
            Visible = false,
            Dock = DockStyle.Bottom,
            Height = 180,
        };

        Controls.Add(_titleLabel);
        Controls.Add(_subtitleLabel);
        Controls.Add(_card1);
        Controls.Add(_card2);
        Controls.Add(_card3);
        Controls.Add(_fullLogBox);
        Controls.Add(_footer);

        AcceptButton = _continueButton;
    }

    private static StepCard NewCard(Point location, int width, int height)
        => new StepCard { Location = location, Size = new Size(width, height) };
}
