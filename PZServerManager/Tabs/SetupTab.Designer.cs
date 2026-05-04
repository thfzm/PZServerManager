namespace PZServerManager.Tabs;

partial class SetupTab
{
    private System.ComponentModel.IContainer components = null;

    // Step 1
    private GroupBox _step1 = null!;
    private TextBox _pathBox = null!;
    private Button _browseButton = null!;
    private Button _confirmButton = null!;

    // Step 2
    private GroupBox _step2 = null!;
    private ProgressBar _step2Progress = null!;
    private Label _step2Status = null!;
    private Button _step2InstallButton = null!;

    // Step 3
    private GroupBox _step3 = null!;
    private ProgressBar _step3Progress = null!;
    private Label _step3Status = null!;
    private Button _step3InstallButton = null!;

    // Maintenance (visible after bootstrap)
    private GroupBox _maintPanel = null!;
    private Button _updateButton = null!;
    private Button _validateButton = null!;
    private Button _cancelButton = null!;

    private TextBox _logBox = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        _step1 = BuildStep1();
        _step2 = BuildStep2();
        _step3 = BuildStep3();
        _maintPanel = BuildMaintPanel();

        // Outer table to stack the rows. AutoSize so it grows with content.
        var stack = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 1,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(20),
        };
        stack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        stack.Controls.Add(_step1);
        stack.Controls.Add(_step2);
        stack.Controls.Add(_step3);
        stack.Controls.Add(_maintPanel);

        // Each row in the stack should fill the column
        _step1.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _step2.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _step3.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _maintPanel.Anchor = AnchorStyles.Left | AnchorStyles.Right;

        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        scroll.Controls.Add(stack);

        // Bottom log area
        var logHost = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 140,
            Padding = new Padding(20, 4, 20, 12),
        };
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
        logHost.Controls.Add(_logBox);

        Controls.Add(scroll);
        Controls.Add(logHost);
    }

    // ---- step 1: pick path ----
    private GroupBox BuildStep1()
    {
        var box = new GroupBox
        {
            Text = "1단계 — 설치 경로",
            Padding = new Padding(12),
            Margin = new Padding(0, 0, 0, 12),
            Height = 110,
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
        };

        var inner = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 2,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
        };
        inner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        inner.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        inner.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        inner.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        inner.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var desc = new Label
        {
            Text = "SteamCMD와 좀보이드 데디케이티드 서버(~5GB)가 이 폴더 아래에 설치됩니다.",
            ForeColor = SystemColors.GrayText,
            AutoSize = true,
            Margin = new Padding(2, 4, 2, 8),
        };
        inner.Controls.Add(desc, 0, 0);
        inner.SetColumnSpan(desc, 3);

        _pathBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(2, 0, 6, 2),
        };
        _browseButton = new Button
        {
            Text = "찾아보기…",
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 6, 2),
        };
        _browseButton.Click += OnBrowse;
        _confirmButton = new Button
        {
            Text = "확인",
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 2),
        };
        _confirmButton.Click += OnConfirmPath;

        inner.Controls.Add(_pathBox, 0, 1);
        inner.Controls.Add(_browseButton, 1, 1);
        inner.Controls.Add(_confirmButton, 2, 1);

        box.Controls.Add(inner);
        return box;
    }

    // ---- step 2: SteamCMD ----
    private GroupBox BuildStep2()
    {
        var box = new GroupBox
        {
            Text = "2단계 — SteamCMD 설치",
            Padding = new Padding(12),
            Margin = new Padding(0, 0, 0, 12),
            Height = 150,
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
        };

        var inner = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
        };
        inner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        inner.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        inner.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        inner.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        inner.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var desc = new Label
        {
            Text = "zip 다운로드(~2MB) + 자체 업데이트(~75MB)까지 한 번에 끝냅니다.",
            ForeColor = SystemColors.GrayText,
            AutoSize = true,
            Margin = new Padding(2, 4, 2, 8),
        };
        inner.Controls.Add(desc, 0, 0);
        inner.SetColumnSpan(desc, 2);

        _step2Progress = new ProgressBar
        {
            Dock = DockStyle.Fill,
            Style = ProgressBarStyle.Continuous,
            Maximum = 100,
            Height = 16,
            Margin = new Padding(2, 2, 8, 2),
        };
        inner.Controls.Add(_step2Progress, 0, 1);

        _step2InstallButton = new Button
        {
            Text = "SteamCMD 설치",
            Dock = DockStyle.Fill,
            Height = 30,
            Margin = new Padding(0, 2, 0, 2),
        };
        _step2InstallButton.Click += OnInstallSteamCmd;
        inner.Controls.Add(_step2InstallButton, 1, 1);
        inner.SetRowSpan(_step2InstallButton, 2);

        _step2Status = new Label
        {
            Text = "",
            ForeColor = SystemColors.GrayText,
            Font = new Font("Consolas", 8.5f),
            AutoEllipsis = true,
            Dock = DockStyle.Fill,
            Height = 18,
            Margin = new Padding(2, 2, 8, 2),
        };
        inner.Controls.Add(_step2Status, 0, 2);

        box.Controls.Add(inner);
        return box;
    }

    // ---- step 3: PZ server ----
    private GroupBox BuildStep3()
    {
        var box = new GroupBox
        {
            Text = "3단계 — 좀보이드 서버 설치",
            Padding = new Padding(12),
            Margin = new Padding(0, 0, 0, 12),
            Height = 150,
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
        };

        var inner = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
        };
        inner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        inner.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        inner.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        inner.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        inner.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var desc = new Label
        {
            Text = "SteamCMD로 app 380870 익명 다운로드 (~5GB).",
            ForeColor = SystemColors.GrayText,
            AutoSize = true,
            Margin = new Padding(2, 4, 2, 8),
        };
        inner.Controls.Add(desc, 0, 0);
        inner.SetColumnSpan(desc, 2);

        _step3Progress = new ProgressBar
        {
            Dock = DockStyle.Fill,
            Style = ProgressBarStyle.Continuous,
            Maximum = 100,
            Height = 16,
            Margin = new Padding(2, 2, 8, 2),
        };
        inner.Controls.Add(_step3Progress, 0, 1);

        _step3InstallButton = new Button
        {
            Text = "서버 설치",
            Dock = DockStyle.Fill,
            Height = 30,
            Margin = new Padding(0, 2, 0, 2),
        };
        _step3InstallButton.Click += OnInstallServer;
        inner.Controls.Add(_step3InstallButton, 1, 1);
        inner.SetRowSpan(_step3InstallButton, 2);

        _step3Status = new Label
        {
            Text = "",
            ForeColor = SystemColors.GrayText,
            Font = new Font("Consolas", 8.5f),
            AutoEllipsis = true,
            Dock = DockStyle.Fill,
            Height = 18,
            Margin = new Padding(2, 2, 8, 2),
        };
        inner.Controls.Add(_step3Status, 0, 2);

        box.Controls.Add(inner);
        return box;
    }

    // ---- maintenance ----
    private GroupBox BuildMaintPanel()
    {
        var box = new GroupBox
        {
            Text = "유지보수 (재설치 / 검증)",
            Padding = new Padding(12),
            Margin = new Padding(0, 0, 0, 12),
            Height = 80,
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            Visible = false,
        };

        var inner = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(2),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
        };

        _updateButton = new Button { Text = "다시 설치 / 업데이트", Width = 170, Height = 30, Margin = new Padding(0, 2, 8, 2) };
        _updateButton.Click += OnUpdate;
        _validateButton = new Button { Text = "파일 검증", Width = 130, Height = 30, Margin = new Padding(0, 2, 8, 2) };
        _validateButton.Click += OnValidate;
        _cancelButton = new Button { Text = "실행 중지", Width = 130, Height = 30, Margin = new Padding(0, 2, 0, 2), Enabled = false };
        _cancelButton.Click += OnCancel;

        inner.Controls.Add(_updateButton);
        inner.Controls.Add(_validateButton);
        inner.Controls.Add(_cancelButton);

        box.Controls.Add(inner);
        return box;
    }
}
