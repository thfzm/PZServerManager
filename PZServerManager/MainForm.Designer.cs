using PZServerManager.Tabs;

namespace PZServerManager;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    private TabControl _tabs = null!;
    private TabPage _setupTab = null!;
    private SetupTab _setupContent = null!;
    private TabPage _serverTab = null!;
    private ServerTab _serverContent = null!;
    private TabPage _configTopTab = null!;
    private TabControl _configSubTabs = null!;
    private TabPage _configSubTabPage = null!;
    private ConfigTab _configContent = null!;
    private TabPage _sandboxSubTabPage = null!;
    private SandboxTab _sandboxContent = null!;
    private TabPage _modsTab = null!;
    private ModsTab _modsContent = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        _tabs = new TabControl { Dock = DockStyle.Fill, Padding = new Point(14, 6) };

        _setupTab = new TabPage("초기 설정");
        _setupContent = new SetupTab { Dock = DockStyle.Fill };
        _setupTab.Controls.Add(_setupContent);

        _serverTab = new TabPage("서버 관리");
        _serverContent = new ServerTab { Dock = DockStyle.Fill };
        _serverTab.Controls.Add(_serverContent);

        _configTopTab = new TabPage("설정 관리");
        _configSubTabs = new TabControl { Dock = DockStyle.Fill };
        _configSubTabPage = new TabPage("Server config");
        _configContent = new ConfigTab { Dock = DockStyle.Fill };
        _configSubTabPage.Controls.Add(_configContent);
        _sandboxSubTabPage = new TabPage("Sandbox");
        _sandboxContent = new SandboxTab { Dock = DockStyle.Fill };
        _sandboxSubTabPage.Controls.Add(_sandboxContent);
        _configSubTabs.TabPages.Add(_configSubTabPage);
        _configSubTabs.TabPages.Add(_sandboxSubTabPage);
        _configTopTab.Controls.Add(_configSubTabs);

        _modsTab = new TabPage("모드 관리");
        _modsContent = new ModsTab { Dock = DockStyle.Fill };
        _modsTab.Controls.Add(_modsContent);

        _tabs.TabPages.Add(_setupTab);
        _tabs.TabPages.Add(_serverTab);
        _tabs.TabPages.Add(_configTopTab);
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
