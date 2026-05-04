using PZServerManager.Forms.Controls;

namespace PZServerManager.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    // ---- outer (category) tabs ----
    private TabControl _categoryTabs = null!;
    private TabPage _setupTab = null!;
    private TabPage _modsTopTab = null!;
    private TabPage _configTopTab = null!;
    private TabPage _serverTopTab = null!;

    // ---- inner (sub) tabs ----
    private TabControl _configSubTabs = null!;
    private TabPage _configTab = null!;
    private TabPage _sandboxTab = null!;

    private TabControl _serverSubTabs = null!;
    private TabPage _consoleTab = null!;
    private TabPage _rconTab = null!;
    private TabPage _playersTab = null!;
    private TabPage _savesTab = null!;
    private TabPage _logsTab = null!;

    // ---- user controls ----
    private SettingsControl _settingsControl = null!;
    private ModsControl _modsControl = null!;
    private ConfigControl _configControl = null!;
    private SandboxControl _sandboxControl = null!;
    private ConsoleControl _consoleControl = null!;
    private RconControl _rconControl = null!;
    private PlayersControl _playersControl = null!;
    private SavesControl _savesControl = null!;
    private LogsControl _logsControl = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        _categoryTabs = new TabControl { Dock = DockStyle.Fill, Padding = new Point(14, 6) };

        // ---- 초기 설정 ----
        _setupTab = new TabPage("초기 설정");
        _settingsControl = new SettingsControl { Dock = DockStyle.Fill };
        _setupTab.Controls.Add(_settingsControl);

        // ---- 모드 관리 ----
        _modsTopTab = new TabPage("모드 관리");
        _modsControl = new ModsControl { Dock = DockStyle.Fill };
        _modsTopTab.Controls.Add(_modsControl);

        // ---- 설정 관리 (Config + Sandbox) ----
        _configTopTab = new TabPage("설정 관리");
        _configSubTabs = new TabControl { Dock = DockStyle.Fill };
        _configTab = new TabPage("Server config");
        _configControl = new ConfigControl { Dock = DockStyle.Fill };
        _configTab.Controls.Add(_configControl);
        _sandboxTab = new TabPage("Sandbox");
        _sandboxControl = new SandboxControl { Dock = DockStyle.Fill };
        _sandboxTab.Controls.Add(_sandboxControl);
        _configSubTabs.TabPages.Add(_configTab);
        _configSubTabs.TabPages.Add(_sandboxTab);
        _configTopTab.Controls.Add(_configSubTabs);

        // ---- 서버 관리 (Console + RCON + Players + Saves + Logs) ----
        _serverTopTab = new TabPage("서버 관리");
        _serverSubTabs = new TabControl { Dock = DockStyle.Fill };

        _consoleTab = new TabPage("Console");
        _consoleControl = new ConsoleControl { Dock = DockStyle.Fill };
        _consoleTab.Controls.Add(_consoleControl);

        _rconTab = new TabPage("RCON");
        _rconControl = new RconControl { Dock = DockStyle.Fill };
        _rconTab.Controls.Add(_rconControl);

        _playersTab = new TabPage("Players");
        _playersControl = new PlayersControl { Dock = DockStyle.Fill };
        _playersTab.Controls.Add(_playersControl);

        _savesTab = new TabPage("Saves");
        _savesControl = new SavesControl { Dock = DockStyle.Fill };
        _savesTab.Controls.Add(_savesControl);

        _logsTab = new TabPage("Logs");
        _logsControl = new LogsControl { Dock = DockStyle.Fill };
        _logsTab.Controls.Add(_logsControl);

        _serverSubTabs.TabPages.Add(_consoleTab);
        _serverSubTabs.TabPages.Add(_rconTab);
        _serverSubTabs.TabPages.Add(_playersTab);
        _serverSubTabs.TabPages.Add(_savesTab);
        _serverSubTabs.TabPages.Add(_logsTab);
        _serverTopTab.Controls.Add(_serverSubTabs);

        _categoryTabs.TabPages.Add(_setupTab);
        _categoryTabs.TabPages.Add(_modsTopTab);
        _categoryTabs.TabPages.Add(_configTopTab);
        _categoryTabs.TabPages.Add(_serverTopTab);

        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1100, 700);
        Controls.Add(_categoryTabs);
        MinimumSize = new Size(900, 580);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "PZ Server Manager";
    }
}
