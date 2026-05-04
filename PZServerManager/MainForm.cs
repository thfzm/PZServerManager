using PZServerManager.Models;
using PZServerManager.Services;

namespace PZServerManager;

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

        _setupContent.Bind(_config, AppConfigStore.Save);
        _serverContent.Bind(_paths, _server, _rcon, _config, AppConfigStore.Save);
        _configContent.Bind(_paths);
        _sandboxContent.Bind(_paths);
        _modsContent.Bind(_paths, _steamCmd, _config, AppConfigStore.Save);

        _server.Exited += OnServerExited;
        _server.StatusChanged += OnServerStatusChanged;
        _scheduler.BackupDue += () => BeginInvoke(HandleBackupDue);
        _scheduler.RestartDue += () => BeginInvoke(HandleRestartDue);
        _scheduler.Start();

        UpdateTrayState(_server.Status);

        if (!_config.IsBootstrapped)
            _tabs.SelectedIndex = 0; // park on setup tab on first launch
    }

    // ---------------- tray ----------------
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

        _tray = new NotifyIcon { Visible = true, Text = "PZ Server Manager", ContextMenuStrip = menu };
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
            _tray.ShowBalloonTip(5000, "PZ 서버 크래시",
                _config.AutoRestartOnCrash
                    ? "자동 재시작을 시도합니다."
                    : "자동 재시작 꺼져있음 — 수동으로 시작하세요.",
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
        if (WindowState == FormWindowState.Minimized) Hide();
    }

    // ---------------- scheduler ----------------
    private async void HandleBackupDue()
    {
        var profile = _server.CurrentProfile?.Name ?? ServerProfile.Default;
        try
        {
            _tray.ShowBalloonTip(3000, "Auto backup", $"백업 중: '{profile}'…", ToolTipIcon.Info);
            await _serverContent.BackupCurrentSaveAsync(profile);
            _scheduler.MarkBackupDone();
        }
        catch { /* SavesControl logs internally */ }
    }

    private async void HandleRestartDue()
    {
        if (!_server.IsRunning) { _scheduler.MarkRestartDone(); return; }
        var profile = _server.CurrentProfile ?? new ServerProfile { Name = ServerProfile.Default };
        try
        {
            _tray.ShowBalloonTip(3000, "Auto restart", $"재시작 중: '{profile.Name}'…", ToolTipIcon.Info);
            await _server.StopAsync(TimeSpan.FromSeconds(60));
            await Task.Delay(TimeSpan.FromSeconds(5));
            _server.Start(profile);
        }
        catch { }
        finally { _scheduler.MarkRestartDone(); }
    }

    // ---------------- crash auto-restart ----------------
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
                $"{CrashWindowMinutes}분 동안 {_recentCrashes.Count}번 크래시 — 자동 재시작 일시 중지.",
                ToolTipIcon.Warning));
            return;
        }

        BeginInvoke(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(15));
            try { _server.Start(profile); } catch { }
        });
    }

    // ---------------- close handling ----------------
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Closing via X minimizes to tray. Use tray "Exit" to actually quit.
        if (!_allowClose && e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
            return;
        }

        if (_server.IsRunning)
        {
            var r = MessageBox.Show(this,
                "서버가 실행 중입니다. 종료 전에 정지할까요?",
                "PZ Server Manager",
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
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
