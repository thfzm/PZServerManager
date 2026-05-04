using System.Diagnostics;
using System.Text.RegularExpressions;
using PZServerManager.Models;
using PZServerManager.Services;

namespace PZServerManager.Tabs;

public partial class ModsTab : UserControl
{
    private PzPaths? _paths;
    private SteamCmd? _steamCmd;
    private AppConfig? _config;
    private Action<AppConfig>? _saveConfig;
    private ModScanner? _scanner;
    private readonly WorkshopApi _api = new();

    private ServerProfile _profile = new() { Name = ServerProfile.Default };
    private List<LocalMod> _installed = new();
    private CancellationTokenSource? _busyCts;

    public ModsTab()
    {
        InitializeComponent();
    }

    public void Bind(PzPaths paths, SteamCmd steamCmd, AppConfig config, Action<AppConfig> saveConfig)
    {
        _paths = paths;
        _steamCmd = steamCmd;
        _config = config;
        _saveConfig = saveConfig;
        _scanner = new ModScanner(paths);

        ReloadProfiles();
        UpdateApiKeyBanner();
        ReloadInstalled();
    }

    private void ReloadProfiles()
    {
        if (_paths is null) return;
        var profiles = _paths.ListProfileNames().ToList();
        if (profiles.Count == 0) profiles.Add(ServerProfile.Default);

        _profileBox.BeginUpdate();
        _profileBox.Items.Clear();
        foreach (var p in profiles) _profileBox.Items.Add(p);
        if (_profileBox.Items.Contains(_profile.Name))
            _profileBox.SelectedItem = _profile.Name;
        else _profileBox.SelectedIndex = 0;
        _profileBox.EndUpdate();
    }

    private void UpdateApiKeyBanner()
    {
        var has = !string.IsNullOrWhiteSpace(_config?.SteamWebApiKey);
        _apiKeyLabel.Text = has ? "API key: 설정됨" : "API key: 없음";
        _apiKeyLabel.ForeColor = has ? Color.SeaGreen : Color.OrangeRed;
        _searchButton.Enabled = has && _busyCts is null;
        _popularButton.Enabled = has && _busyCts is null;
    }

    // ---------------- installed list ----------------
    private void ReloadInstalled()
    {
        if (_scanner is null || _paths is null) return;
        _installed = _scanner.Scan();

        var ini = IniFile.Load(_paths.IniPath(_profile));
        var enabledOrdered = ModSync.ReadEnabledFromIni(ini, _installed);
        var enabledIds = new HashSet<string>(enabledOrdered.Select(m => m.ModId), StringComparer.OrdinalIgnoreCase);

        // Display order: enabled (in load order) first, disabled (alphabetical) after.
        var display = new List<LocalMod>(enabledOrdered);
        foreach (var m in _installed)
            if (!enabledIds.Contains(m.ModId)) display.Add(m);
        _installed = display;

        _installedList.BeginUpdate();
        _installedList.Items.Clear();
        foreach (var m in _installed)
        {
            var lvi = new ListViewItem(m.DisplayName) { Tag = m, Checked = enabledIds.Contains(m.ModId) };
            lvi.SubItems.Add(m.ModId);
            lvi.SubItems.Add(m.WorkshopId.ToString());
            lvi.SubItems.Add(m.IsMapMod ? string.Join(',', m.MapNames) : "");
            _installedList.Items.Add(lvi);
        }
        _installedList.EndUpdate();
        _installedCountLabel.Text = $"Installed: {_installed.Count}";

        UpdateWarnings();
    }

    private List<LocalMod> CurrentEnabledInOrder()
    {
        var list = new List<LocalMod>();
        foreach (ListViewItem item in _installedList.Items)
            if (item.Checked && item.Tag is LocalMod m) list.Add(m);
        return list;
    }

    private void UpdateWarnings()
    {
        var enabled = CurrentEnabledInOrder();
        var sync = ModSync.OrderByDependencies(enabled);
        _warningsLabel.Text = sync.Warnings.Count == 0
            ? "로드 순서 문제 없음."
            : "⚠ " + string.Join("  •  ", sync.Warnings.Take(3));
        _warningsLabel.ForeColor = sync.Warnings.Count == 0 ? SystemColors.GrayText : Color.OrangeRed;
    }

    private void OnInstalledChecked(object? sender, ItemCheckedEventArgs e) => UpdateWarnings();

    private void MoveSelected(int delta)
    {
        if (_installedList.SelectedIndices.Count == 0) return;
        var idx = _installedList.SelectedIndices[0];
        var newIdx = idx + delta;
        if (newIdx < 0 || newIdx >= _installedList.Items.Count) return;

        var item = _installedList.Items[idx];
        _installedList.Items.RemoveAt(idx);
        _installedList.Items.Insert(newIdx, item);
        item.Selected = true;
        item.EnsureVisible();

        if (item.Tag is LocalMod local)
        {
            _installed.Remove(local);
            _installed.Insert(newIdx, local);
        }
        UpdateWarnings();
    }

    private void OnMoveUp(object? sender, EventArgs e) => MoveSelected(-1);
    private void OnMoveDown(object? sender, EventArgs e) => MoveSelected(+1);

    private void OnOpenFolder(object? sender, EventArgs e)
    {
        if (_installedList.SelectedItems.Count == 0) return;
        if (_installedList.SelectedItems[0].Tag is LocalMod m && Directory.Exists(m.ModFolder))
            Process.Start(new ProcessStartInfo { FileName = m.ModFolder, UseShellExecute = true });
    }

    private async void OnUninstall(object? sender, EventArgs e)
    {
        if (_installedList.SelectedItems.Count == 0) return;
        if (_installedList.SelectedItems[0].Tag is not LocalMod m) return;
        if (m.WorkshopId == 0 || _paths is null) return;

        var r = MessageBox.Show(this,
            $"워크샵 {m.WorkshopId} ('{m.DisplayName}') 폴더를 디스크에서 삭제하고 로드 순서에서도 제거할까요?",
            "Uninstall", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (r != DialogResult.Yes) return;

        try
        {
            var workshopDir = Path.Combine(_paths.WorkshopContentDir, m.WorkshopId.ToString());
            if (Directory.Exists(workshopDir))
            {
                await Task.Run(() => Directory.Delete(workshopDir, recursive: true));
                AppendLog($"Deleted {workshopDir}");
            }
        }
        catch (Exception ex) { AppendLog($"[error] uninstall: {ex.Message}"); }
        ReloadInstalled();
    }

    // ---------------- search ----------------
    private async void OnSearch(object? sender, EventArgs e)
        => await DoSearchAsync(_searchBox.Text.Trim());

    private async void OnPopular(object? sender, EventArgs e)
        => await DoSearchAsync("");

    private async Task DoSearchAsync(string text)
    {
        if (_config is null || string.IsNullOrWhiteSpace(_config.SteamWebApiKey)) return;
        SetBusy(true);
        try
        {
            _busyCts = new CancellationTokenSource();
            var result = await _api.SearchAsync(_config.SteamWebApiKey, text, ct: _busyCts.Token);
            ShowSearchResults(result.Mods);
            _resultsCountLabel.Text = $"Workshop: {result.Mods.Count} / {result.Total}";
        }
        catch (Exception ex)
        {
            AppendLog($"[error] search: {ex.Message}");
        }
        finally
        {
            _busyCts?.Dispose();
            _busyCts = null;
            SetBusy(false);
        }
    }

    private void ShowSearchResults(List<WorkshopMod> mods)
    {
        var installedIds = _installed.Select(m => m.WorkshopId).ToHashSet();
        _resultsList.BeginUpdate();
        _resultsList.Items.Clear();
        foreach (var m in mods)
        {
            var lvi = new ListViewItem(m.Title) { Tag = m };
            lvi.SubItems.Add(m.Subscriptions.ToString("N0"));
            lvi.SubItems.Add($"{m.VoteScore:0.00}");
            lvi.SubItems.Add(m.LastUpdated?.ToLocalTime().ToString("yyyy-MM-dd") ?? "");
            lvi.SubItems.Add(installedIds.Contains(m.PublishedFileId) ? "✓ 설치됨" : "");
            _resultsList.Items.Add(lvi);
        }
        _resultsList.EndUpdate();
    }

    private void OnResultDoubleClick(object? sender, EventArgs e)
    {
        if (_resultsList.SelectedItems.Count == 0) return;
        if (_resultsList.SelectedItems[0].Tag is not WorkshopMod m) return;
        Process.Start(new ProcessStartInfo
        {
            FileName = $"https://steamcommunity.com/sharedfiles/filedetails/?id={m.PublishedFileId}",
            UseShellExecute = true,
        });
    }

    // ---------------- install ----------------
    private async void OnInstallSelected(object? sender, EventArgs e)
    {
        if (_resultsList.SelectedItems.Count == 0) return;
        if (_resultsList.SelectedItems[0].Tag is not WorkshopMod m) return;
        await InstallAsync(m.PublishedFileId, m.Title);
    }

    private async void OnInstallByUrl(object? sender, EventArgs e)
    {
        var raw = _urlBox.Text.Trim();
        if (!TryParseWorkshopId(raw, out var id))
        {
            MessageBox.Show(this, "워크샵 ID를 추출하지 못했습니다 (URL 또는 숫자 ID).",
                "Install", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        _urlBox.Clear();
        await InstallAsync(id, $"id {id}");
    }

    private async Task InstallAsync(long workshopId, string label)
    {
        if (_steamCmd is null || _paths is null) return;
        SetBusy(true);
        AppendLog($"[manager] '{label}' (workshop {workshopId}) 설치 중…");
        try
        {
            _busyCts = new CancellationTokenSource();
            var log = new Progress<string>(line => AppendLog(line));
            var exit = await _steamCmd.DownloadWorkshopItemAsync(_paths.ServerDir, workshopId, log, _busyCts.Token);
            if (exit != 0) AppendLog($"[manager] SteamCMD exit code {exit} (다시 스캔합니다)");
            ReloadInstalled();
            // Auto-enable any mods coming from this workshop item.
            foreach (ListViewItem item in _installedList.Items)
                if (item.Tag is LocalMod lm && lm.WorkshopId == workshopId) item.Checked = true;
        }
        catch (Exception ex) { AppendLog($"[error] install: {ex.Message}"); }
        finally
        {
            _busyCts?.Dispose();
            _busyCts = null;
            SetBusy(false);
        }
    }

    private static bool TryParseWorkshopId(string input, out long id)
    {
        if (long.TryParse(input, out id)) return id > 0;
        var m = Regex.Match(input, @"[?&]id=(\d+)");
        if (m.Success && long.TryParse(m.Groups[1].Value, out id)) return id > 0;
        id = 0;
        return false;
    }

    // ---------------- apply ----------------
    private void OnApply(object? sender, EventArgs e)
    {
        if (_paths is null) return;
        var enabled = CurrentEnabledInOrder();
        var sync = ModSync.OrderByDependencies(enabled);

        if (sync.Warnings.Count > 0)
        {
            var msg = "경고:\n  • " + string.Join("\n  • ", sync.Warnings) + "\n\n그래도 적용할까요?";
            var r = MessageBox.Show(this, msg, "Apply mods", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (r != DialogResult.Yes) return;
        }

        var iniPath = _paths.IniPath(_profile);
        var ini = IniFile.Load(iniPath);
        ModSync.Apply(ini, sync.OrderedEnabled);
        try
        {
            ini.Save(iniPath);
            AppendLog($"[manager] 적용 완료 → {Path.GetFileName(iniPath)}");
            AppendLog($"  Mods = {ini.Get("Mods")}");
            AppendLog($"  WorkshopItems = {ini.Get("WorkshopItems")}");
            AppendLog($"  Map = {ini.Get("Map")}");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "저장 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ---------------- API key ----------------
    private void OnSetApiKey(object? sender, EventArgs e)
    {
        if (_config is null || _saveConfig is null) return;
        using var dlg = new ApiKeyDialog { Key = _config.SteamWebApiKey ?? "" };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        _config.SteamWebApiKey = dlg.Key.Trim();
        _saveConfig(_config);
        UpdateApiKeyBanner();
    }

    // ---------------- selection details ----------------
    private void OnInstalledSelected(object? sender, EventArgs e)
    {
        if (_installedList.SelectedItems.Count == 0) return;
        if (_installedList.SelectedItems[0].Tag is not LocalMod m) return;
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"이름: {m.DisplayName}");
        sb.AppendLine($"Mod ID: {m.ModId}");
        sb.AppendLine($"Workshop ID: {m.WorkshopId}");
        if (m.MapNames.Count > 0) sb.AppendLine($"Maps: {string.Join(", ", m.MapNames)}");
        if (m.Requires.Count > 0) sb.AppendLine($"Requires: {string.Join(", ", m.Requires)}");
        if (!string.IsNullOrEmpty(m.Description)) sb.AppendLine($"\n{m.Description}");
        SetDetail(sb.ToString());
    }

    private void OnResultSelected(object? sender, EventArgs e)
    {
        if (_resultsList.SelectedItems.Count == 0) return;
        if (_resultsList.SelectedItems[0].Tag is not WorkshopMod m) return;
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(m.Title);
        sb.AppendLine($"Workshop ID: {m.PublishedFileId}");
        sb.AppendLine($"Subs: {m.Subscriptions:N0}   Vote: {m.VoteScore:0.00}");
        if (m.LastUpdated is not null) sb.AppendLine($"Updated: {m.LastUpdated.Value.ToLocalTime():yyyy-MM-dd}");
        if (m.Tags.Count > 0) sb.AppendLine($"Tags: {string.Join(", ", m.Tags)}");
        if (!string.IsNullOrEmpty(m.Description)) sb.AppendLine($"\n{m.Description}");
        SetDetail(sb.ToString());
    }

    private void SetDetail(string s) { _detailBox.Clear(); _detailBox.AppendText(s); }

    private void SetBusy(bool busy)
    {
        var hasKey = !string.IsNullOrWhiteSpace(_config?.SteamWebApiKey);
        _searchButton.Enabled = !busy && hasKey;
        _popularButton.Enabled = !busy && hasKey;
        _installSelectedButton.Enabled = !busy;
        _installByUrlButton.Enabled = !busy;
        _applyButton.Enabled = !busy;
        _uninstallButton.Enabled = !busy;
        Cursor = busy ? Cursors.AppStarting : Cursors.Default;
    }

    private void OnProfileChanged(object? sender, EventArgs e)
    {
        var name = _profileBox.SelectedItem as string;
        if (string.IsNullOrEmpty(name) || name == _profile.Name) return;
        _profile = new ServerProfile { Name = name };
        ReloadInstalled();
    }

    private void AppendLog(string line)
    {
        if (string.IsNullOrEmpty(line)) return;
        if (InvokeRequired) { BeginInvoke(() => AppendLog(line)); return; }
        _detailBox.AppendText(line);
        if (!line.EndsWith('\n')) _detailBox.AppendText(Environment.NewLine);
    }
}

/// Tiny popup for entering the Steam Web API key.
internal sealed class ApiKeyDialog : Form
{
    private readonly TextBox _box;

    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public string Key { get => _box.Text; set => _box.Text = value; }

    public ApiKeyDialog()
    {
        Text = "Steam Web API key";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MinimizeBox = false;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(440, 160);
        Font = new Font("Segoe UI", 9.5f);

        var info = new Label
        {
            Text = "https://steamcommunity.com/dev/apikey 에서 키를 발급받아 입력하세요.",
            AutoSize = true,
            Location = new Point(12, 12),
            ForeColor = SystemColors.GrayText,
        };
        _box = new TextBox
        {
            Location = new Point(12, 50),
            Size = new Size(416, 23),
            UseSystemPasswordChar = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
        };
        var ok = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Location = new Point(252, 110),
            Size = new Size(80, 28),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
        };
        var cancel = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new Point(340, 110),
            Size = new Size(88, 28),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
        };

        Controls.Add(info);
        Controls.Add(_box);
        Controls.Add(ok);
        Controls.Add(cancel);
        AcceptButton = ok;
        CancelButton = cancel;
    }
}
