using PZServerManager.Forms.Controls;
using PZServerManager.Models;
using PZServerManager.Services;

namespace PZServerManager.Forms;

public partial class MainForm : Form
{
    private const int CrashWindowMinutes = 5;
    private const int CrashMaxRetries = 3;

    private readonly AppConfig _config;
    private readonly PzPaths _paths;
    private readonly ServerProcess _server;
    private readonly RconClient _rcon;
    private readonly SteamCmd _steamCmd;
    private readonly Scheduler _scheduler;
    private readonly Queue<DateTime> _recentCrashes = new();

    private NotifyIcon _tray = null!;
    private ToolStripMenuItem _trayShow = null!;
    private ToolStripMenuItem _trayStartStop = null!;
    private ToolStripMenuItem _trayExit = null!;
    private bool _allowClose;
    private Icon? _currentTrayIcon;

    public MainForm(AppConfig config)
    {
        _config = config;
        _paths = new PzPaths(config.ServerDir);
        _server = new ServerProcess(_paths);
        _rcon = new RconClient();
        _steamCmd = new SteamCmd(config.SteamCmdDir);
        _scheduler = new Scheduler(_config, AppConfigStore.Save);

        InitializeComponent();
        InitializeTray();

        _consoleControl.Bind(_paths, _server);
        _configControl.Bind(_paths);
        _sandboxControl.Bind(_paths);
        _rconControl.Bind(_paths, _rcon);
        _playersControl.Bind(_rcon);
        _modsControl.Bind(_paths, _steamCmd, _config, AppConfigStore.Save);
        _savesControl.Bind(_paths, _config, AppConfigStore.Save, _scheduler);
        _logsControl.Bind(_paths);
        _settingsControl.Bind(_config, AppConfigStore.Save, _steamCmd);

        _server.Exited += OnServerExited;
        _server.StatusChanged += OnServerStatusChanged;
        _scheduler.BackupDue += () => BeginInvoke(HandleBackupDue);
        _scheduler.RestartDue += () => BeginInvoke(HandleRestartDue);
        _scheduler.Start();

        UpdateTrayState(_server.Status);
    }

    private void InitializeTray()
    {
        var menu = new ContextMenuStrip();
        _trayShow = new ToolStripMenuItem("Show", null, (_, _) => ShowFromTray());
        _trayStartStop = new ToolStripMenuItem("Start server", null, async (_, _) => await ToggleServerFromTray());
        _trayExit = new ToolStripMenuItem("Exit", null, (_, _) => { _allowClose = true; Close(); });
        menu.Items.Add(_trayShow);
        menu.Items.Add(_trayStartStop);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(_trayExit);

        _tray = new NotifyIcon
        {
            Visible = true,
            Text = "PZ Server Manager",
            ContextMenuStrip = menu,
        };
        _tray.DoubleClick += (_, _) => ShowFromTray();
        SetTrayIcon(ServerStatus.Stopped);
    }

    private void ShowFromTray()
    {
        Show();
        WindowState = FormWindowState.Normal;
        Activate();
    }

    private async Task ToggleServerFromTray()
    {
        try
        {
            if (_server.IsRunning)
                await _server.StopAsync(TimeSpan.FromSeconds(45));
            else
                _server.Start(_server.CurrentProfile ?? new ServerProfile { Name = ServerProfile.Default });
        }
        catch (Exception ex)
        {
            _tray.ShowBalloonTip(4000, "PZ Server Manager", ex.Message, ToolTipIcon.Error);
        }
    }

    private void OnServerStatusChanged(ServerStatus status)
    {
        if (IsDisposed) return;
        BeginInvoke(() => UpdateTrayState(status));
    }

    private void UpdateTrayState(ServerStatus status)
    {
        SetTrayIcon(status);
        _tray.Text = $"PZ Server Manager — {status}";
        _trayStartStop.Text = status is ServerStatus.Running or ServerStatus.Starting
            ? "Stop server" : "Start server";

        if (status == ServerStatus.Crashed)
            _tray.ShowBalloonTip(5000, "PZ server crashed",
                _config.AutoRestartOnCrash
                    ? "Auto-restart will attempt to recover."
                    : "Auto-restart is off — start manually.",
                ToolTipIcon.Warning);
    }

    private void SetTrayIcon(ServerStatus status)
    {
        var newIcon = StatusIcons.Create(status);
        _tray.Icon = newIcon;
        Icon = newIcon;
        _currentTrayIcon?.Dispose();
        _currentTrayIcon = newIcon;
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (WindowState == FormWindowState.Minimized)
            Hide();
    }

    private async void HandleBackupDue()
    {
        var profile = _server.CurrentProfile?.Name ?? ServerProfile.Default;
        try
        {
            _tray.ShowBalloonTip(3000, "Auto backup", $"Backing up '{profile}'…", ToolTipIcon.Info);
            await _savesControl.BackupSaveAsync(profile);
            _scheduler.MarkBackupDone();
        }
        catch
        {
            // logged inside SavesControl
        }
    }

    private async void HandleRestartDue()
    {
        if (!_server.IsRunning) { _scheduler.MarkRestartDone(); return; }
        var profile = _server.CurrentProfile ?? new ServerProfile { Name = ServerProfile.Default };
        try
        {
            _tray.ShowBalloonTip(3000, "Auto restart", $"Restarting '{profile.Name}'…", ToolTipIcon.Info);
            await _server.StopAsync(TimeSpan.FromSeconds(60));
            await Task.Delay(TimeSpan.FromSeconds(5));
            _server.Start(profile);
        }
        catch
        {
            // ignore — next tick will retry if still due
        }
        finally
        {
            _scheduler.MarkRestartDone();
        }
    }

    private void OnServerExited(int code)
    {
        if (code == 0) return;
        if (!_config.AutoRestartOnCrash) return;
        if (_server.CurrentProfile is null) return;

        var profile = _server.CurrentProfile;
        var now = DateTime.UtcNow;
        _recentCrashes.Enqueue(now);
        while (_recentCrashes.Count > 0 &&
               (now - _recentCrashes.Peek()) > TimeSpan.FromMinutes(CrashWindowMinutes))
            _recentCrashes.Dequeue();

        if (_recentCrashes.Count > CrashMaxRetries)
        {
            BeginInvoke(() => _tray.ShowBalloonTip(6000, "Crash loop",
                $"Server has crashed {_recentCrashes.Count} times in the last {CrashWindowMinutes} minutes — auto-restart paused.",
                ToolTipIcon.Warning));
            return;
        }

        BeginInvoke(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(15));
            try { _server.Start(profile); } catch { }
        });
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Closing via the X button minimizes to tray instead of exiting.
        // Use the tray "Exit" menu (or the Yes/No prompt below) to actually quit.
        if (!_allowClose && e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        if (_server.IsRunning)
        {
            var r = MessageBox.Show(this,
                "The server is running. Stop it before exiting?",
                "PZ Server Manager",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning);
            if (r == DialogResult.Cancel) { e.Cancel = true; return; }
            if (r == DialogResult.Yes)
            {
                e.Cancel = true;
                StopThenClose();
                return;
            }
            _server.Kill();
        }

        _scheduler.Dispose();
        _server.Dispose();
        _rcon.Dispose();
        _tray.Visible = false;
        _tray.Dispose();
        _currentTrayIcon?.Dispose();
        base.OnFormClosing(e);
    }

    private async void StopThenClose()
    {
        try { await _server.StopAsync(TimeSpan.FromSeconds(45)); }
        catch { }
        _allowClose = true;
        _scheduler.Dispose();
        _server.Dispose();
        _rcon.Dispose();
        _tray.Visible = false;
        _tray.Dispose();
        _currentTrayIcon?.Dispose();
        Close();
    }
}
