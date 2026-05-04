using System.Diagnostics;
using System.Text.RegularExpressions;
using PZServerManager.Models;
using PZServerManager.Services;

namespace PZServerManager.Forms.Controls;

public partial class SettingsControl : UserControl
{
    private enum WizardStage { PickPath, DownloadSteamCmd, InstallServer, Done }

    private static readonly Regex PercentRegex = new(@"\[\s*(\d+)\s*%\]", RegexOptions.Compiled);

    private AppConfig? _config;
    private Action<AppConfig>? _saveConfig;
    private SteamCmd? _steamCmd;
    private CancellationTokenSource? _busyCts;

    private WizardStage _stage = WizardStage.PickPath;

    public SettingsControl()
    {
        InitializeComponent();
    }

    public void Bind(AppConfig config, Action<AppConfig> saveConfig, SteamCmd steamCmd)
    {
        _config = config;
        _saveConfig = saveConfig;
        _steamCmd = steamCmd;

        _stage = config.IsBootstrapped ? WizardStage.Done : WizardStage.PickPath;
        _wizardPathBox.Text = string.IsNullOrWhiteSpace(config.ServerDir)
            ? AppPaths.DefaultInstallRoot
            : Path.GetDirectoryName(config.ServerDir.TrimEnd('\\', '/')) ?? AppPaths.DefaultInstallRoot;

        LoadToUi();
        UpdateWizardUi();
    }

    // ---------------- existing settings sections ----------------
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
        if (string.IsNullOrWhiteSpace(_config.ServerDir))
        {
            MessageBox.Show(this, "초기 설치를 먼저 완료해주세요.", "Update",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

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
            AppendLog(exit == 0 ? "[update] success" : $"[update] SteamCMD exited with code {exit}");
        }
        catch (Exception ex) { AppendLog($"[error] update failed: {ex.Message}"); }
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
        if (string.IsNullOrWhiteSpace(_config.ServerDir))
        {
            MessageBox.Show(this, "초기 설치를 먼저 완료해주세요.", "Validate",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _validateButton.Enabled = false;
        _busyCts = new CancellationTokenSource();
        try
        {
            var args = $"+force_install_dir \"{_config.ServerDir}\" +login anonymous +app_info_update 1 +app_update 380870 validate +quit";
            var log = new Progress<string>(line => AppendLog(line));
            var exit = await _steamCmd.RunAsync(args, log, _busyCts.Token);
            AppendLog(exit == 0 ? "[validate] success" : $"[validate] SteamCMD exited with code {exit}");
        }
        catch (Exception ex) { AppendLog($"[error] validate failed: {ex.Message}"); }
        finally
        {
            _busyCts?.Dispose();
            _busyCts = null;
            _validateButton.Enabled = true;
        }
    }

    private void OnCancel(object? sender, EventArgs e) => _busyCts?.Cancel();

    // ---------------- wizard ----------------
    private string WizardSteamCmdDir => Path.Combine(_wizardPathBox.Text.Trim(), "steamcmd");
    private string WizardServerDir => Path.Combine(_wizardPathBox.Text.Trim(), "server");

    private void UpdateWizardUi()
    {
        var bootstrapped = _stage == WizardStage.Done;
        _wizardBox.Visible = !bootstrapped;
        _doneBanner.Visible = bootstrapped;
        _pathsBox.Visible = bootstrapped;
        _serverBox.Visible = bootstrapped;
        _networkBox.Visible = bootstrapped;
        _apiBox.Visible = true;
        _aboutBox.Visible = true;

        if (bootstrapped) return;

        var s1 = _stage == WizardStage.PickPath;
        var s2 = _stage == WizardStage.DownloadSteamCmd;
        var s3 = _stage == WizardStage.InstallServer;

        _card1.IsActive = s1;
        _card2.IsActive = s2;
        _card3.IsActive = s3;
        _card1.IsDone = _stage > WizardStage.PickPath;
        _card2.IsDone = _stage > WizardStage.DownloadSteamCmd;
        _card3.IsDone = false;

        _badge1.State = BadgeFor(WizardStage.PickPath);
        _badge2.State = BadgeFor(WizardStage.DownloadSteamCmd);
        _badge3.State = BadgeFor(WizardStage.InstallServer);

        _wizardPathBox.Enabled = s1 && _busyCts is null;
        _wizardBrowseButton.Enabled = s1 && _busyCts is null;
        _wizardConfirmButton.Enabled = s1 && _busyCts is null;
        _wizardDownloadButton.Enabled = s2 && _busyCts is null;
        _wizardInstallButton.Enabled = s3 && _busyCts is null;
    }

    private StepBadge.BadgeState BadgeFor(WizardStage stage)
    {
        if (_stage == stage) return StepBadge.BadgeState.Active;
        if (_stage > stage) return StepBadge.BadgeState.Done;
        return StepBadge.BadgeState.Inactive;
    }

    private void OnWizardBrowse(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog
        {
            Description = "Choose install location for SteamCMD and the PZ dedicated server",
            UseDescriptionForTitle = true,
            InitialDirectory = Directory.Exists(_wizardPathBox.Text)
                ? _wizardPathBox.Text
                : AppPaths.DefaultInstallRoot,
        };
        if (dlg.ShowDialog(this) == DialogResult.OK)
            _wizardPathBox.Text = dlg.SelectedPath;
    }

    private void OnWizardConfirmPath(object? sender, EventArgs e)
    {
        var root = _wizardPathBox.Text.Trim();
        if (string.IsNullOrEmpty(root))
        {
            MessageBox.Show(this, "설치 경로를 입력해주세요.", "Setup",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        try { Directory.CreateDirectory(root); }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Setup", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (File.Exists(Path.Combine(WizardSteamCmdDir, "steamcmd.exe")))
        {
            _wizardStatus2.Text = "SteamCMD가 이미 설치되어 있습니다.";
            _wizardProgress2.Value = 100;
            _stage = WizardStage.InstallServer;
        }
        else
        {
            _stage = WizardStage.DownloadSteamCmd;
        }
        UpdateWizardUi();
    }

    private async void OnWizardDownloadSteamCmd(object? sender, EventArgs e)
    {
        if (_stage != WizardStage.DownloadSteamCmd) return;
        _busyCts = new CancellationTokenSource();
        UpdateWizardUi();

        try
        {
            var steamCmd = new SteamCmd(WizardSteamCmdDir);
            var log = new Progress<string>(line =>
            {
                _wizardStatus2.Text = Truncate(line);
                AppendLog(line);
            });
            var pct = new Progress<int>(p => _wizardProgress2.Value = Math.Clamp(p, 0, 100));

            ((IProgress<string>)log).Report("[manager] downloading SteamCMD…");
            await steamCmd.DownloadAndExtractAsync(log, pct, _busyCts.Token);

            ((IProgress<string>)log).Report("[manager] running self-update…");
            _wizardProgress2.Style = ProgressBarStyle.Marquee;
            await steamCmd.PrewarmAsync(log, _busyCts.Token);
            _wizardProgress2.Style = ProgressBarStyle.Continuous;

            _wizardStatus2.Text = "완료 — SteamCMD 준비됨.";
            _wizardProgress2.Value = 100;
            _stage = WizardStage.InstallServer;
        }
        catch (OperationCanceledException) { _wizardStatus2.Text = "중단되었습니다."; }
        catch (Exception ex)
        {
            _wizardStatus2.Text = $"실패: {ex.Message}";
            AppendLog($"[error] {ex.Message}");
            MessageBox.Show(this, ex.Message, "SteamCMD install failed",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _wizardProgress2.Style = ProgressBarStyle.Continuous;
            _busyCts?.Dispose();
            _busyCts = null;
            UpdateWizardUi();
        }
    }

    private async void OnWizardInstallServer(object? sender, EventArgs e)
    {
        if (_stage != WizardStage.InstallServer) return;
        _busyCts = new CancellationTokenSource();
        _wizardProgress3.Value = 0;
        UpdateWizardUi();

        try
        {
            var steamCmd = new SteamCmd(WizardSteamCmdDir);
            var log = new Progress<string>(line =>
            {
                _wizardStatus3.Text = Truncate(line);
                AppendLog(line);
                var m = PercentRegex.Match(line);
                if (m.Success && int.TryParse(m.Groups[1].Value, out var p))
                    _wizardProgress3.Value = Math.Clamp(p, 0, 100);
            });
            var exit = await steamCmd.InstallOrUpdatePzServerAsync(WizardServerDir, log, _busyCts.Token, validate: false);
            if (exit != 0)
                throw new InvalidOperationException($"SteamCMD exited with code {exit}.");

            if (_config is null || _saveConfig is null) return;
            _config.SteamCmdDir = WizardSteamCmdDir;
            _config.ServerDir = WizardServerDir;
            _saveConfig(_config);

            _wizardProgress3.Value = 100;
            _wizardStatus3.Text = "완료.";
            _stage = WizardStage.Done;

            MessageBox.Show(this,
                "초기 설치가 완료되었습니다. 다른 탭을 사용하기 위해 앱을 재시작합니다.",
                "Setup complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Application.Restart();
        }
        catch (OperationCanceledException) { _wizardStatus3.Text = "중단되었습니다."; }
        catch (Exception ex)
        {
            _wizardStatus3.Text = $"실패: {ex.Message}";
            AppendLog($"[error] {ex.Message}");
            MessageBox.Show(this, ex.Message, "Install failed",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _busyCts?.Dispose();
            _busyCts = null;
            UpdateWizardUi();
        }
    }

    private static string Truncate(string s)
    {
        const int max = 110;
        return s.Length > max ? s[..max] + "…" : s;
    }

    private void AppendLog(string line)
    {
        if (string.IsNullOrEmpty(line)) return;
        if (InvokeRequired) { BeginInvoke(() => AppendLog(line)); return; }
        _logBox.AppendText(line);
        if (!line.EndsWith("\n")) _logBox.AppendText(Environment.NewLine);
    }
}
