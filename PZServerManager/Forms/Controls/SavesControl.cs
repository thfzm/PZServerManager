using System.Diagnostics;
using PZServerManager.Models;
using PZServerManager.Services;

namespace PZServerManager.Forms.Controls;

public partial class SavesControl : UserControl
{
    private PzPaths? _paths;
    private SaveBackup? _backup;
    private AppConfig? _config;
    private Action<AppConfig>? _saveConfig;
    private Scheduler? _scheduler;

    public SavesControl()
    {
        InitializeComponent();
    }

    public void Bind(PzPaths paths, AppConfig config, Action<AppConfig> saveConfig, Scheduler scheduler)
    {
        _paths = paths;
        _config = config;
        _saveConfig = saveConfig;
        _scheduler = scheduler;
        _backup = new SaveBackup(paths, () => _config?.BackupDir ?? "");

        if (string.IsNullOrWhiteSpace(_config.BackupDir))
            _config.BackupDir = Path.Combine(AppPaths.AppDataDir, "backups");

        LoadSettingsToUi();
        Refresh();
    }

    public new void Refresh()
    {
        if (_backup is null || _config is null) return;
        _backupDirBox.Text = _config.BackupDir;
        _savesList.BeginUpdate();
        _savesList.Items.Clear();
        try
        {
            var saves = _backup.ListSaves();
            foreach (var s in saves)
            {
                var lvi = new ListViewItem(s.Name) { Tag = s };
                lvi.SubItems.Add(s.SizeDisplay);
                lvi.SubItems.Add(s.LastModified.ToString("yyyy-MM-dd HH:mm"));
                lvi.SubItems.Add(s.BackupCount.ToString());
                _savesList.Items.Add(lvi);
            }
            _statusLabel.Text = $"{saves.Count} saves at {_paths!.MultiplayerSavesDir}";
        }
        catch (Exception ex)
        {
            _statusLabel.Text = $"Error: {ex.Message}";
        }
        finally
        {
            _savesList.EndUpdate();
        }
    }

    private SaveInfo? Selected()
        => _savesList.SelectedItems.Count > 0 ? _savesList.SelectedItems[0].Tag as SaveInfo : null;

    private void OnRefresh(object? sender, EventArgs e) => Refresh();

    private void OnBrowseDir(object? sender, EventArgs e)
    {
        if (_config is null) return;
        using var dlg = new FolderBrowserDialog
        {
            Description = "Backup destination",
            UseDescriptionForTitle = true,
            InitialDirectory = Directory.Exists(_config.BackupDir) ? _config.BackupDir : AppPaths.AppDataDir,
        };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        _config.BackupDir = dlg.SelectedPath;
        _saveConfig?.Invoke(_config);
        _backupDirBox.Text = _config.BackupDir;
        Refresh();
    }

    private void OnOpenBackupDir(object? sender, EventArgs e)
    {
        if (_config is null) return;
        Directory.CreateDirectory(_config.BackupDir);
        Process.Start(new ProcessStartInfo { FileName = _config.BackupDir, UseShellExecute = true });
    }

    private void OnOpenSavesDir(object? sender, EventArgs e)
    {
        if (_paths is null) return;
        Directory.CreateDirectory(_paths.MultiplayerSavesDir);
        Process.Start(new ProcessStartInfo { FileName = _paths.MultiplayerSavesDir, UseShellExecute = true });
    }

    private async void OnBackup(object? sender, EventArgs e)
    {
        var s = Selected();
        if (s is null) { Toast("Select a save first."); return; }
        await BackupSaveAsync(s.Name);
    }

    public async Task BackupSaveAsync(string saveName)
    {
        if (_backup is null || _config is null) return;
        SetBusy(true);
        try
        {
            var log = new Progress<string>(line => AppendLog(line));
            var path = await _backup.BackupAsync(saveName, log, CancellationToken.None);
            var deleted = _backup.Rotate(saveName, _config.BackupRetention);
            if (deleted > 0) AppendLog($"[backup] rotated out {deleted} old backup(s)");
            AppendLog($"[backup] saved to {path}");
        }
        catch (Exception ex)
        {
            AppendLog($"[error] backup failed: {ex.Message}");
        }
        finally
        {
            SetBusy(false);
            Refresh();
        }
    }

    private async void OnRestore(object? sender, EventArgs e)
    {
        var s = Selected();
        if (s is null || _backup is null) { Toast("Select a save first."); return; }
        var backups = _backup.ListBackups(s.Name);
        if (backups.Count == 0) { Toast("No backups exist for this save."); return; }

        var pick = PickBackupDialog.Pick(this, backups);
        if (pick is null) return;

        var r = MessageBox.Show(this,
            $"Restore '{s.Name}' from\n  {Path.GetFileName(pick)}\n\nThis WILL OVERWRITE the current save folder.",
            "Restore", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (r != DialogResult.Yes) return;

        SetBusy(true);
        try
        {
            var log = new Progress<string>(line => AppendLog(line));
            await _backup.RestoreAsync(s.Name, pick, log, CancellationToken.None);
        }
        catch (Exception ex)
        {
            AppendLog($"[error] restore failed: {ex.Message}");
        }
        finally
        {
            SetBusy(false);
            Refresh();
        }
    }

    private void OnDelete(object? sender, EventArgs e)
    {
        var s = Selected();
        if (s is null || _backup is null) { Toast("Select a save first."); return; }
        var r = MessageBox.Show(this,
            $"Delete save '{s.Name}' permanently from disk? Backups (if any) are kept.",
            "Delete save", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (r != DialogResult.Yes) return;
        try { _backup.Delete(s.Name); }
        catch (Exception ex) { AppendLog($"[error] delete failed: {ex.Message}"); }
        Refresh();
    }

    // ---- schedule UI ----
    private void LoadSettingsToUi()
    {
        if (_config is null) return;
        _autoBackupCheck.Checked = _config.AutoBackupEnabled;
        _autoBackupHours.Value = (decimal)Math.Clamp(_config.AutoBackupIntervalHours, 0.25, 720);
        _autoRestartCheck.Checked = _config.AutoRestartEnabled;
        _autoRestartHours.Value = (decimal)Math.Clamp(_config.AutoRestartIntervalHours, 0.25, 720);
        _crashRestartCheck.Checked = _config.AutoRestartOnCrash;
        _retentionBox.Value = Math.Clamp(_config.BackupRetention, 0, 100);
    }

    private void OnSaveSchedule(object? sender, EventArgs e)
    {
        if (_config is null || _scheduler is null) return;
        _config.AutoBackupEnabled = _autoBackupCheck.Checked;
        _config.AutoBackupIntervalHours = (double)_autoBackupHours.Value;
        _config.AutoRestartEnabled = _autoRestartCheck.Checked;
        _config.AutoRestartIntervalHours = (double)_autoRestartHours.Value;
        _config.AutoRestartOnCrash = _crashRestartCheck.Checked;
        _config.BackupRetention = (int)_retentionBox.Value;
        _saveConfig?.Invoke(_config);
        _scheduler.NotifyConfigChanged();
        AppendLog("[schedule] saved");
    }

    private void SetBusy(bool busy)
    {
        _backupButton.Enabled = !busy;
        _restoreButton.Enabled = !busy;
        _deleteButton.Enabled = !busy;
        _refreshButton.Enabled = !busy;
        Cursor = busy ? Cursors.AppStarting : Cursors.Default;
    }

    private void AppendLog(string line)
    {
        if (string.IsNullOrEmpty(line)) return;
        if (InvokeRequired) { BeginInvoke(() => AppendLog(line)); return; }
        _logBox.AppendText(line);
        if (!line.EndsWith("\n")) _logBox.AppendText(Environment.NewLine);
    }

    private void Toast(string msg)
        => MessageBox.Show(this, msg, "Saves", MessageBoxButtons.OK, MessageBoxIcon.Information);
}
