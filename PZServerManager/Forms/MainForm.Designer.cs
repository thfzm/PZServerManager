using PZServerManager.Forms.Controls;

namespace PZServerManager.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    private TabControl _tabs = null!;
    private TabPage _consoleTab = null!;
    private ConsoleControl _consoleControl = null!;
    private TabPage _configTab = null!;
    private ConfigControl _configControl = null!;
    private TabPage _sandboxTab = null!;
    private SandboxControl _sandboxControl = null!;
    private TabPage _modsTab = null!;
    private ModsControl _modsControl = null!;
    private TabPage _playersTab = null!;
    private PlayersControl _playersControl = null!;
    private TabPage _savesTab = null!;
    private SavesControl _savesControl = null!;
    private TabPage _rconTab = null!;
    private RconControl _rconControl = null!;
    private TabPage _logsTab = null!;
    private LogsControl _logsControl = null!;
    private TabPage _settingsTab = null!;
    private SettingsControl _settingsControl = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        _tabs = new TabControl { Dock = DockStyle.Fill };

        _consoleTab = new TabPage("Console");
        _consoleControl = new ConsoleControl { Dock = DockStyle.Fill };
        _consoleTab.Controls.Add(_consoleControl);

        _configTab = new TabPage("Config");
        _configControl = new ConfigControl { Dock = DockStyle.Fill };
        _configTab.Controls.Add(_configControl);

        _sandboxTab = new TabPage("Sandbox");
        _sandboxControl = new SandboxControl { Dock = DockStyle.Fill };
        _sandboxTab.Controls.Add(_sandboxControl);

        _modsTab = new TabPage("Mods");
        _modsControl = new ModsControl { Dock = DockStyle.Fill };
        _modsTab.Controls.Add(_modsControl);

        _playersTab = new TabPage("Players");
        _playersControl = new PlayersControl { Dock = DockStyle.Fill };
        _playersTab.Controls.Add(_playersControl);

        _savesTab = new TabPage("Saves");
        _savesControl = new SavesControl { Dock = DockStyle.Fill };
        _savesTab.Controls.Add(_savesControl);

        _rconTab = new TabPage("RCON");
        _rconControl = new RconControl { Dock = DockStyle.Fill };
        _rconTab.Controls.Add(_rconControl);

        _logsTab = new TabPage("Logs");
        _logsControl = new LogsControl { Dock = DockStyle.Fill };
        _logsTab.Controls.Add(_logsControl);

        _settingsTab = new TabPage("Settings");
        _settingsControl = new SettingsControl { Dock = DockStyle.Fill };
        _settingsTab.Controls.Add(_settingsControl);

        _tabs.TabPages.Add(_consoleTab);
        _tabs.TabPages.Add(_configTab);
        _tabs.TabPages.Add(_sandboxTab);
        _tabs.TabPages.Add(_modsTab);
        _tabs.TabPages.Add(_playersTab);
        _tabs.TabPages.Add(_savesTab);
        _tabs.TabPages.Add(_rconTab);
        _tabs.TabPages.Add(_logsTab);
        _tabs.TabPages.Add(_settingsTab);

        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1100, 680);
        Controls.Add(_tabs);
        MinimumSize = new Size(900, 560);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "PZ Server Manager";
    }
}
