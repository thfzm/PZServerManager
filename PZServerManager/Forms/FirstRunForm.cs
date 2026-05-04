using PZServerManager.Models;
using PZServerManager.Services;

namespace PZServerManager.Forms;

public partial class FirstRunForm : Form
{
    private CancellationTokenSource? _cts;
    private bool _installing;

    public AppConfig? Result { get; private set; }

    public FirstRunForm()
    {
        InitializeComponent();
        _installRootBox.Text = AppPaths.DefaultInstallRoot;
    }

    private void OnBrowse(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog
        {
            Description = "Choose install location for SteamCMD and the PZ dedicated server",
            UseDescriptionForTitle = true,
            InitialDirectory = Directory.Exists(_installRootBox.Text)
                ? _installRootBox.Text
                : AppPaths.DefaultInstallRoot,
        };
        if (dlg.ShowDialog(this) == DialogResult.OK)
            _installRootBox.Text = dlg.SelectedPath;
    }

    private async void OnInstall(object? sender, EventArgs e)
    {
        if (_installing) return;

        var root = _installRootBox.Text.Trim();
        if (string.IsNullOrEmpty(root))
        {
            MessageBox.Show(this, "Install path is required.", "PZ Server Manager",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var steamCmdDir = Path.Combine(root, "steamcmd");
        var serverDir = Path.Combine(root, "server");

        _installing = true;
        _cts = new CancellationTokenSource();
        SetUiInstalling(true);
        _logBox.Clear();

        var log = new Progress<string>(line =>
        {
            _logBox.AppendText(line);
            _logBox.AppendText(Environment.NewLine);
        });

        try
        {
            var steamCmd = new SteamCmd(steamCmdDir);
            if (!steamCmd.IsInstalled)
                await steamCmd.DownloadAndExtractAsync(log, _cts.Token);
            else
                ((IProgress<string>)log).Report("SteamCMD already present, skipping download.");

            var exit = await steamCmd.InstallOrUpdatePzServerAsync(serverDir, log, _cts.Token);
            if (exit != 0)
                throw new InvalidOperationException($"SteamCMD exited with code {exit}.");

            var config = new AppConfig
            {
                SteamCmdDir = steamCmdDir,
                ServerDir = serverDir,
            };
            AppConfigStore.Save(config);
            Result = config;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (OperationCanceledException)
        {
            ((IProgress<string>)log).Report("Cancelled.");
        }
        catch (Exception ex)
        {
            ((IProgress<string>)log).Report($"ERROR: {ex.Message}");
            MessageBox.Show(this, ex.Message, "Install failed",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _installing = false;
            _cts?.Dispose();
            _cts = null;
            SetUiInstalling(false);
        }
    }

    private void OnCancel(object? sender, EventArgs e)
    {
        if (_installing)
        {
            _cts?.Cancel();
            return;
        }
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void SetUiInstalling(bool installing)
    {
        _installRootBox.Enabled = !installing;
        _browseButton.Enabled = !installing;
        _installButton.Enabled = !installing;
        _cancelButton.Text = installing ? "Stop" : "Cancel";
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_installing)
        {
            e.Cancel = true;
            _cts?.Cancel();
            return;
        }
        base.OnFormClosing(e);
    }
}
