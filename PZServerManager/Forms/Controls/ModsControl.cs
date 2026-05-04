using System.Diagnostics;
using System.Net.Http;
using System.Text.RegularExpressions;
using PZServerManager.Models;
using PZServerManager.Services;

namespace PZServerManager.Forms.Controls;

public partial class ModsControl : UserControl
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

    public ModsControl()
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

    public void ReloadProfiles()
    {
        if (_paths is null) return;
        var profiles = _paths.ListProfileNames().ToList();
        if (profiles.Count == 0) profiles.Add(ServerProfile.Default);

        _profileBox.BeginUpdate();
        _profileBox.Items.Clear();
        foreach (var p in profiles) _profileBox.Items.Add(p);
        if (_profileBox.Items.Contains(_profile.Name)) _profileBox.SelectedItem = _profile.Name;
        else _profileBox.SelectedIndex = 0;
        _profileBox.EndUpdate();
    }

    private void UpdateApiKeyBanner()
    {
        var hasKey = !string.IsNullOrWhiteSpace(_config?.SteamWebApiKey);
        _apiKeyLabel.Text = hasKey ? "API key: set" : "API key: missing";
        _apiKeyLabel.ForeColor = hasKey ? Color.SeaGreen : Color.OrangeRed;
        _searchButton.Enabled = hasKey;
        _popularButton.Enabled = hasKey;
    }

    private void OnSetApiKey(object? sender, EventArgs e)
    {
        if (_config is null || _saveConfig is null) return;
        var key = PromptDialog.Ask(this, "Steam Web API key",
            "Get one at https://steamcommunity.com/dev/apikey",
            _config.SteamWebApiKey ?? "", isPassword: true);
        if (key is null) return;
        _config.SteamWebApiKey = key.Trim();
        _saveConfig(_config);
        UpdateApiKeyBanner();
    }

    private void OnProfileChanged(object? sender, EventArgs e)
    {
        var name = _profileBox.SelectedItem as string;
        if (string.IsNullOrEmpty(name) || name == _profile.Name) return;
        _profile = new ServerProfile { Name = name };
        ReloadInstalled();
    }

    private void ReloadInstalled()
    {
        if (_scanner is null || _paths is null) return;
        _installed = _scanner.Scan();

        var ini = IniFile.Load(_paths.IniPath(_profile));
        var enabledOrdered = ModSync.ReadEnabledFromIni(ini, _installed);
        var enabledSet = new HashSet<string>(enabledOrdered.Select(m => m.ModId), StringComparer.OrdinalIgnoreCase);

        // Display order: enabled (in load order) first, disabled (alphabetical) after.
        var displayOrder = new List<LocalMod>();
        displayOrder.AddRange(enabledOrdered);
        foreach (var m in _installed)
            if (!enabledSet.Contains(m.ModId)) displayOrder.Add(m);
        _installed = displayOrder;

        _installedList.BeginUpdate();
        _installedList.Items.Clear();
        foreach (var m in _installed)
        {
            var lvi = new ListViewItem(m.DisplayName) { Tag = m, Checked = enabledSet.Contains(m.ModId) };
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
            ? "No load-order issues."
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

        var local = (LocalMod)item.Tag!;
        _installed.Remove(local);
        _installed.Insert(newIdx, local);
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
        if (m.WorkshopId == 0) return;
        var r = MessageBox.Show(this,
            $"Delete workshop item {m.WorkshopId} ('{m.DisplayName}') from disk? It will be removed from the load order too.",
            "Uninstall", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (r != DialogResult.Yes) return;

        try
        {
            var workshopDir = Path.Combine(_paths!.WorkshopContentDir, m.WorkshopId.ToString());
            if (Directory.Exists(workshopDir))
            {
                await Task.Run(() => Directory.Delete(workshopDir, recursive: true));
                AppendLog($"Deleted {workshopDir}");
            }
        }
        catch (Exception ex)
        {
            AppendLog($"[error] uninstall failed: {ex.Message}");
        }
        ReloadInstalled();
    }

    private async void OnSearch(object? sender, EventArgs e)
        => await DoSearchAsync(_searchBox.Text.Trim(), browseMode: false);

    private async void OnPopular(object? sender, EventArgs e)
        => await DoSearchAsync("", browseMode: true);

    private async Task DoSearchAsync(string text, bool browseMode)
    {
        if (_config is null || string.IsNullOrWhiteSpace(_config.SteamWebApiKey)) return;
        SetBusy(true);
        try
        {
            _busyCts = new CancellationTokenSource();
            var result = await _api.SearchAsync(
                _config.SteamWebApiKey,
                searchText: browseMode ? null : text,
                queryType: browseMode ? 9 : 21,
                ct: _busyCts.Token);
            ShowResults(result.Mods);
            _resultsCountLabel.Text = $"Workshop: {result.Mods.Count} of {result.Total}";
        }
        catch (Exception ex)
        {
            AppendLog($"[error] search failed: {ex.Message}");
        }
        finally
        {
            _busyCts?.Dispose();
            _busyCts = null;
            SetBusy(false);
        }
    }

    private void ShowResults(List<WorkshopMod> mods)
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
            lvi.SubItems.Add(installedIds.Contains(m.PublishedFileId) ? "✓ installed" : "");
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

    private async void OnInstallSelected(object? sender, EventArgs e)
    {
        if (_resultsList.SelectedItems.Count == 0) return;
        if (_resultsList.SelectedItems[0].Tag is not WorkshopMod m) return;
        await InstallAsync(m.PublishedFileId, m.Title);
    }

    private async void OnInstallByUrl(object? sender, EventArgs e)
    {
        var raw = _urlBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(raw)) return;
        if (!TryParseWorkshopId(raw, out var id))
        {
            MessageBox.Show(this, "Could not parse a workshop ID from that input.",
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
        AppendLog($"[manager] installing '{label}' (workshop {workshopId})…");
        try
        {
            _busyCts = new CancellationTokenSource();
            var log = new Progress<string>(line => AppendLog(line));
            var exit = await _steamCmd.DownloadWorkshopItemAsync(_paths.ServerDir, workshopId, log, _busyCts.Token);
            if (exit != 0)
                AppendLog($"[manager] SteamCMD exited with code {exit} (often non-fatal — re-scanning)");
            ReloadInstalled();
            // Auto-enable the newly installed mod(s).
            foreach (ListViewItem item in _installedList.Items)
                if (item.Tag is LocalMod lm && lm.WorkshopId == workshopId) item.Checked = true;
        }
        catch (Exception ex)
        {
            AppendLog($"[error] install failed: {ex.Message}");
        }
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

    private async void OnApply(object? sender, EventArgs e)
    {
        if (_paths is null) return;
        var enabled = CurrentEnabledInOrder();
        var sync = ModSync.OrderByDependencies(enabled);
        if (sync.Warnings.Count > 0)
        {
            var msg = "Warnings:\n  • " + string.Join("\n  • ", sync.Warnings) + "\n\nApply anyway?";
            var r = MessageBox.Show(this, msg, "Apply mods",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (r != DialogResult.Yes) return;
        }

        var iniPath = _paths.IniPath(_profile);
        var ini = IniFile.Load(iniPath);
        ModSync.Apply(ini, sync.OrderedEnabled);
        try
        {
            ini.Save(iniPath);
            AppendLog($"[manager] applied {sync.OrderedEnabled.Count} mods to {Path.GetFileName(iniPath)}");
            AppendLog($"  Mods = {ini.Get("Mods")}");
            AppendLog($"  WorkshopItems = {ini.Get("WorkshopItems")}");
            AppendLog($"  Map = {ini.Get("Map")}");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Save failed",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnInstalledSelected(object? sender, EventArgs e)
    {
        if (_installedList.SelectedItems.Count == 0) return;
        if (_installedList.SelectedItems[0].Tag is not LocalMod m) return;
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Name: {m.DisplayName}");
        sb.AppendLine($"Mod id: {m.ModId}");
        sb.AppendLine($"Workshop id: {m.WorkshopId}");
        if (m.MapNames.Count > 0) sb.AppendLine($"Maps: {string.Join(", ", m.MapNames)}");
        if (m.Requires.Count > 0) sb.AppendLine($"Requires: {string.Join(", ", m.Requires)}");
        if (!string.IsNullOrEmpty(m.VersionMin)) sb.AppendLine($"Min version: {m.VersionMin}");
        if (!string.IsNullOrEmpty(m.Description)) sb.AppendLine($"\n{m.Description}");
        _detailBox.Text = sb.ToString();
    }

    private void OnResultSelected(object? sender, EventArgs e)
    {
        if (_resultsList.SelectedItems.Count == 0) return;
        if (_resultsList.SelectedItems[0].Tag is not WorkshopMod m) return;
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"{m.Title}");
        sb.AppendLine($"Workshop id: {m.PublishedFileId}");
        sb.AppendLine($"Subscribers: {m.Subscriptions:N0}   Vote score: {m.VoteScore:0.00}");
        if (m.LastUpdated is not null) sb.AppendLine($"Updated: {m.LastUpdated.Value.ToLocalTime():yyyy-MM-dd}");
        if (m.Tags.Count > 0) sb.AppendLine($"Tags: {string.Join(", ", m.Tags)}");
        if (!string.IsNullOrEmpty(m.Description)) sb.AppendLine($"\n{m.Description}");
        _detailBox.Text = sb.ToString();
    }

    private void SetBusy(bool busy)
    {
        _searchButton.Enabled = !busy && !string.IsNullOrEmpty(_config?.SteamWebApiKey);
        _popularButton.Enabled = _searchButton.Enabled;
        _installSelectedButton.Enabled = !busy;
        _installByUrlButton.Enabled = !busy;
        _applyButton.Enabled = !busy;
        Cursor = busy ? Cursors.AppStarting : Cursors.Default;
    }

    private void AppendLog(string line)
    {
        if (string.IsNullOrEmpty(line)) return;
        if (InvokeRequired) { BeginInvoke(() => AppendLog(line)); return; }
        _detailBox.AppendText(line);
        if (!line.EndsWith("\n")) _detailBox.AppendText(Environment.NewLine);
    }
}
