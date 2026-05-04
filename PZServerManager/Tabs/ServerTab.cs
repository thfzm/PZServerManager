using PZServerManager.Models;
using PZServerManager.Services;

namespace PZServerManager.Tabs;

public partial class ServerTab : UserControl
{
    private PzPaths? _paths;
    private ServerProcess? _server;

    public ServerTab()
    {
        InitializeComponent();
        UpdateStatusUi(ServerStatus.Stopped);
    }

    public void Bind(PzPaths paths, ServerProcess server, RconClient rcon, AppConfig config, Action<AppConfig> saveConfig)
    {
        _paths = paths;
        _server = server;
        _server.StatusChanged += OnStatusChanged;

        _consoleSubTab.Bind(server);
        _rconSubTab.Bind(paths, rcon);
        _playersSubTab.Bind(rcon);
        _savesSubTab.Bind(paths, config, saveConfig);
        _logsSubTab.Bind(paths);

        ReloadProfiles();
    }

    /// Lets the scheduler delegate auto-backup to our Saves sub-tab.
    public Task BackupCurrentSaveAsync(string saveName) => _savesSubTab.BackupSaveAsync(saveName);

    public void ReloadProfiles()
    {
        if (_paths is null) return;
        var profiles = _paths.ListProfileNames().ToList();
        if (profiles.Count == 0) profiles.Add(ServerProfile.Default);

        _profileBox.BeginUpdate();
        _profileBox.Items.Clear();
        foreach (var p in profiles) _profileBox.Items.Add(p);
        if (_profileBox.Items.Contains(ServerProfile.Default))
            _profileBox.SelectedItem = ServerProfile.Default;
        else
            _profileBox.SelectedIndex = 0;
        _profileBox.EndUpdate();
    }

    private void OnStatusChanged(ServerStatus status)
    {
        if (IsDisposed) return;
        BeginInvoke(() => UpdateStatusUi(status));
    }

    private void UpdateStatusUi(ServerStatus status)
    {
        _statusLabel.Text = "Status: " + status switch
        {
            ServerStatus.Stopped => "Stopped",
            ServerStatus.Starting => "Starting…",
            ServerStatus.Running => "Running",
            ServerStatus.Stopping => "Stopping…",
            ServerStatus.Crashed => "Crashed",
            _ => status.ToString(),
        };
        _statusLabel.ForeColor = status switch
        {
            ServerStatus.Running => Color.SeaGreen,
            ServerStatus.Crashed => Color.Crimson,
            ServerStatus.Starting or ServerStatus.Stopping => Color.Goldenrod,
            _ => SystemColors.GrayText,
        };

        var running = status is ServerStatus.Running or ServerStatus.Starting;
        _startButton.Enabled = !running && _server is not null && _paths is not null && !string.IsNullOrEmpty(_paths.ServerDir);
        _stopButton.Enabled = running;
        _profileBox.Enabled = !running;
    }

    private void OnStart(object? sender, EventArgs e)
    {
        if (_server is null) return;
        var name = (_profileBox.SelectedItem as string) ?? ServerProfile.Default;
        try
        {
            _server.Start(new ServerProfile { Name = name });
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "서버 시작 실패",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void OnStop(object? sender, EventArgs e)
    {
        if (_server is null || !_server.IsRunning) return;
        _stopButton.Enabled = false;
        try { await _server.StopAsync(TimeSpan.FromSeconds(45)); }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Stop error", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        if (_server is not null) _server.StatusChanged -= OnStatusChanged;
        base.OnHandleDestroyed(e);
    }
}
