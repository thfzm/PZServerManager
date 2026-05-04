using System.Diagnostics;
using PZServerManager.Models;
using PZServerManager.Services;

namespace PZServerManager.Forms.Controls;

public partial class SettingsControl : UserControl
{
    private AppConfig? _config;
    private Action<AppConfig>? _saveConfig;
    private SteamCmd? _steamCmd;
    private CancellationTokenSource? _busyCts;

    public SettingsControl()
    {
        InitializeComponent();
    }

    public void Bind(AppConfig config, Action<AppConfig> saveConfig, SteamCmd steamCmd)
    {
        _config = config;
        _saveConfig = saveConfig;
        _steamCmd = steamCmd;
        LoadToUi();
    }

    private void LoadToUi()
    {
        if (_config is null) return;
        _steamCmdDirBox.Text = _config.SteamCmdDir;
        _serverDirBox.Text = _config.ServerDir;
        _backupDirBox.Text = _config.BackupDir;
        _apiKeyBox.Text = _config.SteamWebApiKey ?? "";
        _appConfigPathBox.Text = AppPaths.ConfigFile;
        _zomboidDirBox.Text = AppPaths.ZomboidUserDir;
        _versionLabel.Text = $"PZServerManager — .NET {Environment.Version} — {typeof(SettingsControl).Assembly.GetName().Version}";
    }

    private void OnOpenSteamCmd(object? sender, EventArgs e) => OpenIfExists(_config?.SteamCmdDir);
    private void OnOpenServer(object? sender, EventArgs e) => OpenIfExists(_config?.ServerDir);
    private void OnOpenBackupDir(object? sender, EventArgs e) => OpenIfExists(_config?.BackupDir);
    private void OnOpenZomboid(object? sender, EventArgs e) => OpenIfExists(AppPaths.ZomboidUserDir);
    private void OnOpenAppData(object? sender, EventArgs e) => OpenIfExists(AppPaths.AppDataDir);

    private static void OpenIfExists(string? dir)
    {
        if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return;
        Process.Start(new ProcessStartInfo { FileName = dir, UseShellExecute = true });
    }

    private void OnSaveApiKey(object? sender, EventArgs e)
    {
        if (_config is null || _saveConfig is null) return;
        _config.SteamWebApiKey = _apiKeyBox.Text.Trim();
        _saveConfig(_config);
        AppendLog("[settings] Steam Web API key saved");
    }

    private async void OnRefreshIp(object? sender, EventArgs e)
    {
        _ipBox.Text = "fetching…";
        try
        {
            var ip = await PublicIp.FetchAsync();
            _ipBox.Text = ip;
        }
        catch (Exception ex)
        {
            _ipBox.Text = $"(failed: {ex.Message})";
        }
    }

    private async void OnUpdateServer(object? sender, EventArgs e)
    {
        if (_steamCmd is null || _config is null) return;
        if (_busyCts is not null) return;

        var r = MessageBox.Show(this,
            "Run SteamCMD app_update 380870 against the configured server directory?",
            "Update server", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (r != DialogResult.Yes) return;

        _updateButton.Enabled = false;
        _busyCts = new CancellationTokenSource();
        try
        {
            var log = new Progress<string>(line => AppendLog(line));
            var exit = await _steamCmd.InstallOrUpdatePzServerAsync(_config.ServerDir, log, _busyCts.Token);
            AppendLog(exit == 0
                ? "[update] success"
                : $"[update] SteamCMD exited with code {exit}");
        }
        catch (Exception ex)
        {
            AppendLog($"[error] update failed: {ex.Message}");
        }
        finally
        {
            _busyCts?.Dispose();
            _busyCts = null;
            _updateButton.Enabled = true;
        }
    }

    private async void OnValidateServer(object? sender, EventArgs e)
    {
        if (_steamCmd is null || _config is null) return;
        if (_busyCts is not null) return;

        _validateButton.Enabled = false;
        _busyCts = new CancellationTokenSource();
        try
        {
            var args = $"+force_install_dir \"{_config.ServerDir}\" +login anonymous +app_update 380870 validate +quit";
            var log = new Progress<string>(line => AppendLog(line));
            var exit = await _steamCmd.RunAsync(args, log, _busyCts.Token);
            AppendLog(exit == 0
                ? "[validate] success"
                : $"[validate] SteamCMD exited with code {exit}");
        }
        catch (Exception ex)
        {
            AppendLog($"[error] validate failed: {ex.Message}");
        }
        finally
        {
            _busyCts?.Dispose();
            _busyCts = null;
            _validateButton.Enabled = true;
        }
    }

    private void OnCancel(object? sender, EventArgs e) => _busyCts?.Cancel();

    private void AppendLog(string line)
    {
        if (string.IsNullOrEmpty(line)) return;
        if (InvokeRequired) { BeginInvoke(() => AppendLog(line)); return; }
        _logBox.AppendText(line);
        if (!line.EndsWith("\n")) _logBox.AppendText(Environment.NewLine);
    }
}
