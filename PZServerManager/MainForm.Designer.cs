namespace PZServerManager;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    private TabControl _tabs = null!;
    private TabPage _setupTab = null!;
    private TabPage _serverTab = null!;
    private TabPage _configTab = null!;
    private TabPage _modsTab = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        _tabs = new TabControl { Dock = DockStyle.Fill, Padding = new Point(14, 6) };

        _setupTab = MakePlaceholderTab("초기 설정", "Phase 2에서 SteamCMD + 좀보이드 서버 설치 위저드를 여기에 붙입니다.");
        _serverTab = MakePlaceholderTab("서버 관리", "Phase 3에서 콘솔/RCON/플레이어/세이브 sub-tabs 들어옵니다.");
        _configTab = MakePlaceholderTab("설정 관리", "Phase 4에서 servertest.ini / SandboxVars.lua 편집기 들어옵니다.");
        _modsTab = MakePlaceholderTab("모드 관리", "Phase 5에서 워크샵 검색/설치 들어옵니다.");

        _tabs.TabPages.Add(_setupTab);
        _tabs.TabPages.Add(_serverTab);
        _tabs.TabPages.Add(_configTab);
        _tabs.TabPages.Add(_modsTab);

        AutoScaleMode = AutoScaleMode.Font;
        Font = new Font("Segoe UI", 9.5f);
        ClientSize = new Size(1100, 700);
        MinimumSize = new Size(900, 600);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "PZ Server Manager";
        Controls.Add(_tabs);
    }

    private static TabPage MakePlaceholderTab(string title, string body)
    {
        var page = new TabPage(title) { Padding = new Padding(20) };
        var label = new Label
        {
            Text = body,
            Dock = DockStyle.Top,
            AutoSize = true,
            Font = new Font("Segoe UI", 11f),
            ForeColor = SystemColors.GrayText,
            Padding = new Padding(0, 8, 0, 0),
        };
        page.Controls.Add(label);
        return page;
    }
}
