using System.Text.RegularExpressions;
using PZServerManager.Forms.Controls;
using PZServerManager.Models;
using PZServerManager.Services;

namespace PZServerManager.Forms;

public partial class FirstRunForm : Form
{
    private enum Stage { PickPath, DownloadSteamCmd, InstallServer, Done }

    private static readonly Regex PercentRegex = new(@"\[\s*(\d+)\s*%\]", RegexOptions.Compiled);

    private Stage _stage = Stage.PickPath;
    private CancellationTokenSource? _busyCts;

    public AppConfig? Result { get; private set; }

    private string SteamCmdDir => Path.Combine(_pathBox.Text.Trim(), "steamcmd");
    private string ServerDir => Path.Combine(_pathBox.Text.Trim(), "server");

    public FirstRunForm()
    {
        InitializeComponent();
        _pathBox.Text = AppPaths.DefaultInstallRoot;
        UpdateUi();
    }

    private void UpdateUi()
    {
        var card1Active = _stage == Stage.PickPath;
        var card2Active = _stage == Stage.DownloadSteamCmd;
        var card3Active = _stage == Stage.InstallServer;

        _card1.IsActive = card1Active;
        _card2.IsActive = card2Active;
        _card3.IsActive = card3Active;

        _card1.IsDone = _stage > Stage.PickPath;
        _card2.IsDone = _stage > Stage.DownloadSteamCmd;
        _card3.IsDone = _stage > Stage.InstallServer;

        _badge1.State = BadgeFor(Stage.PickPath);
        _badge2.State = BadgeFor(Stage.DownloadSteamCmd);
        _badge3.State = BadgeFor(Stage.InstallServer);

        _pathBox.Enabled = card1Active && _busyCts is null;
        _browseButton.Enabled = card1Active && _busyCts is null;
        _confirmPathButton.Enabled = card1Active && _busyCts is null;
        _downloadButton.Enabled = card2Active && _busyCts is null;
        _installButton.Enabled = card3Active && _busyCts is null;
        var done = _stage == Stage.Done;
        _continueButton.Enabled = done;
        if (done)
        {
            _continueButton.BackColor = Color.FromArgb(74, 122, 250);
            _continueButton.ForeColor = Color.White;
            _continueButton.FlatStyle = FlatStyle.Flat;
            _continueButton.FlatAppearance.BorderSize = 0;
            _continueButton.Font = new Font(_continueButton.Font, FontStyle.Bold);
            _continueButton.Focus();
        }
        else
        {
            _continueButton.UseVisualStyleBackColor = true;
            _continueButton.FlatStyle = FlatStyle.System;
        }
        _cancelButton.Text = _busyCts is null ? "취소" : "중단";
    }

    private StepBadge.BadgeState BadgeFor(Stage stage)
    {
        if (_stage == stage) return StepBadge.BadgeState.Active;
        if (_stage > stage) return StepBadge.BadgeState.Done;
        return StepBadge.BadgeState.Inactive;
    }

    // ---------------------- Step 1 ----------------------
    private void OnBrowse(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog
        {
            Description = "Choose install location for SteamCMD and the PZ dedicated server",
            UseDescriptionForTitle = true,
            InitialDirectory = Directory.Exists(_pathBox.Text)
                ? _pathBox.Text
                : AppPaths.DefaultInstallRoot,
        };
        if (dlg.ShowDialog(this) == DialogResult.OK)
            _pathBox.Text = dlg.SelectedPath;
    }

    private void OnConfirmPath(object? sender, EventArgs e)
    {
        var root = _pathBox.Text.Trim();
        if (string.IsNullOrEmpty(root))
        {
            MessageBox.Show(this, "Install path is required.", "PZ Server Manager",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        try { Directory.CreateDirectory(root); }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "PZ Server Manager",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // SteamCMD already present? Skip step 2.
        if (File.Exists(Path.Combine(SteamCmdDir, "steamcmd.exe")))
        {
            _status2.Text = "SteamCMD가 이미 이 경로에 설치되어 있습니다.";
            _progress2.Value = 100;
            _stage = Stage.InstallServer;
        }
        else
        {
            _stage = Stage.DownloadSteamCmd;
        }
        UpdateUi();
    }

    // ---------------------- Step 2 ----------------------
    private async void OnDownloadSteamCmd(object? sender, EventArgs e)
    {
        if (_stage != Stage.DownloadSteamCmd) return;

        _busyCts = new CancellationTokenSource();
        UpdateUi();

        try
        {
            var steamCmd = new SteamCmd(SteamCmdDir);
            var log = new Progress<string>(line =>
            {
                _status2.Text = Truncate(line);
                AppendFullLog(line);
            });
            var pct = new Progress<int>(p => _progress2.Value = Math.Clamp(p, 0, 100));

            // Phase 1: download zip (reports real percentage via Content-Length)
            ((IProgress<string>)log).Report("[manager] downloading SteamCMD…");
            await steamCmd.DownloadAndExtractAsync(log, pct, _busyCts.Token);

            // Phase 2: pre-warm so the bootstrap exe self-updates now (instead of mid-server-install)
            ((IProgress<string>)log).Report("[manager] running self-update (this is the part that prints exit 7 — handled internally)");
            _progress2.Style = ProgressBarStyle.Marquee;
            await steamCmd.PrewarmAsync(log, _busyCts.Token);
            _progress2.Style = ProgressBarStyle.Continuous;

            _status2.Text = "완료 — SteamCMD 준비됨.";
            _progress2.Value = 100;
            _stage = Stage.InstallServer;
        }
        catch (OperationCanceledException)
        {
            _status2.Text = "중단되었습니다.";
        }
        catch (Exception ex)
        {
            _status2.Text = $"Failed: {ex.Message}";
            AppendFullLog($"[error] {ex.Message}");
            MessageBox.Show(this, ex.Message, "SteamCMD install failed",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _progress2.Style = ProgressBarStyle.Continuous;
            _busyCts?.Dispose();
            _busyCts = null;
            UpdateUi();
        }
    }

    // ---------------------- Step 3 ----------------------
    private async void OnInstallServer(object? sender, EventArgs e)
    {
        if (_stage != Stage.InstallServer) return;

        _busyCts = new CancellationTokenSource();
        _progress3.Value = 0;
        UpdateUi();

        try
        {
            var steamCmd = new SteamCmd(SteamCmdDir);
            var log = new Progress<string>(line =>
            {
                _status3.Text = Truncate(line);
                AppendFullLog(line);
                var m = PercentRegex.Match(line);
                if (m.Success && int.TryParse(m.Groups[1].Value, out var p))
                    _progress3.Value = Math.Clamp(p, 0, 100);
            });
            // First install — skip `validate` for speed; it would re-check every file we just freshly fetched.
            var exit = await steamCmd.InstallOrUpdatePzServerAsync(ServerDir, log, _busyCts.Token, validate: false);
            if (exit != 0)
                throw new InvalidOperationException($"SteamCMD exited with code {exit}.");

            var config = new AppConfig
            {
                SteamCmdDir = SteamCmdDir,
                ServerDir = ServerDir,
            };
            AppConfigStore.Save(config);
            Result = config;

            _progress3.Value = 100;
            _status3.Text = "완료 — '계속' 버튼을 눌러 관리 창으로 이동하세요.";
            _stage = Stage.Done;
        }
        catch (OperationCanceledException)
        {
            _status3.Text = "중단되었습니다.";
        }
        catch (Exception ex)
        {
            _status3.Text = $"Failed: {ex.Message}";
            AppendFullLog($"[error] {ex.Message}");
            MessageBox.Show(this, ex.Message, "Install failed",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _busyCts?.Dispose();
            _busyCts = null;
            UpdateUi();
        }
    }

    private void OnContinue(object? sender, EventArgs e)
    {
        if (_stage != Stage.Done) return;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void OnCancel(object? sender, EventArgs e)
    {
        if (_busyCts is not null)
        {
            _busyCts.Cancel();
            return;
        }
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void OnToggleFullLog(object? sender, EventArgs e)
    {
        _fullLogBox.Visible = _showLogToggle.Checked;
    }

    private void AppendFullLog(string line)
    {
        if (string.IsNullOrEmpty(line)) return;
        _fullLogBox.AppendText(line);
        _fullLogBox.AppendText(Environment.NewLine);
    }

    private static string Truncate(string s)
    {
        const int max = 110;
        return s.Length > max ? s[..max] + "…" : s;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_busyCts is not null)
        {
            e.Cancel = true;
            _busyCts.Cancel();
            return;
        }
        base.OnFormClosing(e);
    }
}
