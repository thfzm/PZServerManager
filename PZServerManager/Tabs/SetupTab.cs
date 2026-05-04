using System.Text.RegularExpressions;
using PZServerManager.Models;
using PZServerManager.Services;

namespace PZServerManager.Tabs;

public partial class SetupTab : UserControl
{
    private enum Stage { PickPath, DownloadSteamCmd, InstallServer, Done }

    private static readonly Regex PercentRegex = new(@"\[\s*(\d+)\s*%\]", RegexOptions.Compiled);

    private AppConfig? _config;
    private Action<AppConfig>? _saveConfig;
    private Stage _stage = Stage.PickPath;
    private CancellationTokenSource? _busyCts;

    private string SteamCmdDir => Path.Combine(_pathBox.Text.Trim(), "steamcmd");
    private string ServerDir => Path.Combine(_pathBox.Text.Trim(), "server");

    public SetupTab()
    {
        InitializeComponent();
    }

    public void Bind(AppConfig config, Action<AppConfig> saveConfig)
    {
        _config = config;
        _saveConfig = saveConfig;

        if (config.IsBootstrapped)
        {
            // Configured already — show paths derived from config, lock everything.
            var root = Path.GetDirectoryName(config.ServerDir.TrimEnd('\\', '/')) ?? AppPaths.DefaultInstallRoot;
            _pathBox.Text = root;
            _stage = Stage.Done;
        }
        else
        {
            _pathBox.Text = AppPaths.DefaultInstallRoot;
            _stage = Stage.PickPath;
        }
        RefreshUi();
    }

    // ---------------- step 1: pick path ----------------
    private void OnBrowse(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog
        {
            Description = "Choose install location for SteamCMD and the PZ dedicated server",
            UseDescriptionForTitle = true,
            InitialDirectory = Directory.Exists(_pathBox.Text) ? _pathBox.Text : AppPaths.DefaultInstallRoot,
        };
        if (dlg.ShowDialog(this) == DialogResult.OK) _pathBox.Text = dlg.SelectedPath;
    }

    private void OnConfirmPath(object? sender, EventArgs e)
    {
        var root = _pathBox.Text.Trim();
        if (string.IsNullOrEmpty(root))
        {
            Warn("설치 경로를 입력하거나 선택해주세요.");
            return;
        }
        try { Directory.CreateDirectory(root); }
        catch (Exception ex) { Warn(ex.Message); return; }

        // Skip step 2 if SteamCMD already at this location.
        if (File.Exists(Path.Combine(SteamCmdDir, "steamcmd.exe")))
        {
            _step2Status.Text = "SteamCMD가 이미 존재합니다 — 건너뜁니다.";
            _step2Progress.Value = 100;
            _stage = Stage.InstallServer;
        }
        else
        {
            _stage = Stage.DownloadSteamCmd;
        }
        RefreshUi();
    }

    // ---------------- step 2: install SteamCMD ----------------
    private async void OnInstallSteamCmd(object? sender, EventArgs e)
    {
        if (_stage != Stage.DownloadSteamCmd) return;
        _busyCts = new CancellationTokenSource();
        RefreshUi();

        try
        {
            var sc = new SteamCmd(SteamCmdDir);
            var log = new Progress<string>(line =>
            {
                _step2Status.Text = Truncate(line);
                AppendLog(line);
            });
            var pct = new Progress<int>(p => _step2Progress.Value = Math.Clamp(p, 0, 100));

            ((IProgress<string>)log).Report("[manager] downloading SteamCMD…");
            await sc.DownloadAndExtractAsync(log, pct, _busyCts.Token);

            ((IProgress<string>)log).Report("[manager] running self-update (prewarm)…");
            _step2Progress.Style = ProgressBarStyle.Marquee;
            await sc.PrewarmAsync(log, _busyCts.Token);
            _step2Progress.Style = ProgressBarStyle.Continuous;
            _step2Progress.Value = 100;

            _step2Status.Text = "완료 — SteamCMD 준비됨.";
            _stage = Stage.InstallServer;
        }
        catch (OperationCanceledException) { _step2Status.Text = "중단되었습니다."; }
        catch (Exception ex)
        {
            _step2Status.Text = $"실패: {ex.Message}";
            AppendLog($"[error] {ex.Message}");
            ShowError("SteamCMD 설치 실패", ex);
        }
        finally
        {
            _step2Progress.Style = ProgressBarStyle.Continuous;
            _busyCts?.Dispose();
            _busyCts = null;
            RefreshUi();
        }
    }

    // ---------------- step 3: install PZ server ----------------
    private async void OnInstallServer(object? sender, EventArgs e)
    {
        if (_stage != Stage.InstallServer) return;
        _busyCts = new CancellationTokenSource();
        _step3Progress.Value = 0;
        RefreshUi();

        try
        {
            var sc = new SteamCmd(SteamCmdDir);
            var log = new Progress<string>(line =>
            {
                _step3Status.Text = Truncate(line);
                AppendLog(line);
                var m = PercentRegex.Match(line);
                if (m.Success && int.TryParse(m.Groups[1].Value, out var p))
                    _step3Progress.Value = Math.Clamp(p, 0, 100);
            });

            // Fresh install — skip `validate` for speed.
            var exit = await sc.InstallOrUpdatePzServerAsync(ServerDir, log, _busyCts.Token, validate: false);
            if (exit != 0) throw new InvalidOperationException($"SteamCMD exited with code {exit}.");

            if (_config is null || _saveConfig is null) return;
            _config.SteamCmdDir = SteamCmdDir;
            _config.ServerDir = ServerDir;
            _saveConfig(_config);

            _step3Progress.Value = 100;
            _step3Status.Text = "완료 — 곧 앱이 재시작됩니다.";
            _stage = Stage.Done;

            MessageBox.Show(this,
                "초기 설치가 완료되었습니다. 다른 탭들을 활성화하기 위해 앱을 재시작합니다.",
                "Setup complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Application.Restart();
        }
        catch (OperationCanceledException) { _step3Status.Text = "중단되었습니다."; }
        catch (Exception ex)
        {
            _step3Status.Text = $"실패: {ex.Message}";
            AppendLog($"[error] {ex.Message}");
            ShowError("서버 설치 실패", ex);
        }
        finally
        {
            _busyCts?.Dispose();
            _busyCts = null;
            RefreshUi();
        }
    }

    // ---------------- maintenance (post-bootstrap only) ----------------
    private async void OnUpdate(object? sender, EventArgs e)
    {
        if (_config is null) return;
        if (_busyCts is not null) return;
        var r = MessageBox.Show(this,
            "서버 파일을 SteamCMD로 다시 받습니다 (서버는 미리 정지하세요).",
            "Update", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
        if (r != DialogResult.OK) return;

        _busyCts = new CancellationTokenSource();
        RefreshUi();
        try
        {
            var sc = new SteamCmd(_config.SteamCmdDir);
            var log = new Progress<string>(AppendLog);
            var exit = await sc.InstallOrUpdatePzServerAsync(_config.ServerDir, log, _busyCts.Token);
            AppendLog(exit == 0 ? "[update] 완료" : $"[update] SteamCMD exit {exit}");
        }
        catch (Exception ex) { AppendLog($"[error] {ex.Message}"); }
        finally
        {
            _busyCts?.Dispose();
            _busyCts = null;
            RefreshUi();
        }
    }

    private async void OnValidate(object? sender, EventArgs e)
    {
        if (_config is null) return;
        if (_busyCts is not null) return;
        _busyCts = new CancellationTokenSource();
        RefreshUi();
        try
        {
            var sc = new SteamCmd(_config.SteamCmdDir);
            var log = new Progress<string>(AppendLog);
            var exit = await sc.InstallOrUpdatePzServerAsync(_config.ServerDir, log, _busyCts.Token, validate: true);
            AppendLog(exit == 0 ? "[validate] 완료" : $"[validate] SteamCMD exit {exit}");
        }
        catch (Exception ex) { AppendLog($"[error] {ex.Message}"); }
        finally
        {
            _busyCts?.Dispose();
            _busyCts = null;
            RefreshUi();
        }
    }

    private void OnCancel(object? sender, EventArgs e) => _busyCts?.Cancel();

    // ---------------- UI state ----------------
    private void RefreshUi()
    {
        var bootstrapped = _stage == Stage.Done && _config?.IsBootstrapped == true;
        var busy = _busyCts is not null;

        // Title decoration per step
        DecorateStep(_step1, "1단계 — 설치 경로", _stage > Stage.PickPath || bootstrapped);
        DecorateStep(_step2, "2단계 — SteamCMD 설치", _stage > Stage.DownloadSteamCmd || bootstrapped);
        DecorateStep(_step3, "3단계 — 좀보이드 서버 설치", _stage > Stage.InstallServer || bootstrapped);

        // Per-step controls
        var s1Active = _stage == Stage.PickPath;
        _pathBox.Enabled = s1Active && !busy;
        _browseButton.Enabled = s1Active && !busy;
        _confirmButton.Enabled = s1Active && !busy;

        var s2Active = _stage == Stage.DownloadSteamCmd;
        _step2InstallButton.Enabled = s2Active && !busy;

        var s3Active = _stage == Stage.InstallServer;
        _step3InstallButton.Enabled = s3Active && !busy;

        // Maintenance panel only after bootstrap
        _maintPanel.Visible = bootstrapped;
        _updateButton.Enabled = bootstrapped && !busy;
        _validateButton.Enabled = bootstrapped && !busy;
        _cancelButton.Enabled = busy;
    }

    private static void DecorateStep(GroupBox box, string baseText, bool done)
    {
        box.Text = done ? "✓ " + baseText : baseText;
        box.ForeColor = done ? Color.SeaGreen : SystemColors.ControlText;
    }

    private void AppendLog(string line)
    {
        if (string.IsNullOrEmpty(line)) return;
        if (InvokeRequired) { BeginInvoke(() => AppendLog(line)); return; }
        _logBox.AppendText(line);
        if (!line.EndsWith('\n')) _logBox.AppendText(Environment.NewLine);
    }

    private static string Truncate(string s) => s.Length > 110 ? s[..110] + "…" : s;

    private void Warn(string msg)
        => MessageBox.Show(this, msg, "Setup", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    private void ShowError(string title, Exception ex)
        => MessageBox.Show(this, ex.Message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
}
