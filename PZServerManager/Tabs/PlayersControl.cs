using System.Text.RegularExpressions;
using PZServerManager.Services;

namespace PZServerManager.Tabs;

public partial class PlayersControl : UserControl
{
    private RconClient? _rcon;
    private readonly System.Windows.Forms.Timer _autoRefreshTimer = new() { Interval = 10_000 };

    public PlayersControl()
    {
        InitializeComponent();
        _autoRefreshTimer.Tick += async (_, _) => await RefreshPlayersAsync();
        var levels = new object[] { "none", "admin", "moderator", "overseer", "gm", "observer" };
        _accessLevelBox.Items.AddRange(levels);
        _accessLevelBox.SelectedIndex = 0;
        _adminLevelBox.Items.AddRange(levels);
        _adminLevelBox.SelectedIndex = 0;
        UpdateConnectedUi(false);
    }

    public void Bind(RconClient rcon)
    {
        _rcon = rcon;
        _rcon.ConnectionChanged += OnConnectionChanged;
        UpdateConnectedUi(_rcon.IsConnected);
    }

    private void OnConnectionChanged(bool connected)
    {
        if (IsDisposed) return;
        BeginInvoke(() => UpdateConnectedUi(connected));
    }

    private void UpdateConnectedUi(bool connected)
    {
        _bannerLabel.Text = connected
            ? "RCON 연결됨 — 실행 가능"
            : "RCON 연결 안 됨 — 'RCON' 탭에서 연결하세요";
        _bannerLabel.ForeColor = connected ? Color.SeaGreen : Color.OrangeRed;

        var ctls = new Control[]
        {
            _refreshButton, _autoRefreshCheck,
            _kickButton, _banSelectedButton, _msgSelectedButton, _grantAdminSelectedButton,
            _setAccessButton, _giveItemButton, _teleportButton,
            _whitelistAddButton, _whitelistRemoveButton,
            _banAddButton, _banRemoveButton,
            _grantAdminButton, _removeAdminButton, _setAccessApplyButton,
        };
        foreach (var c in ctls) c.Enabled = connected;

        if (!connected)
        {
            _autoRefreshTimer.Stop();
            _autoRefreshCheck.Checked = false;
        }
    }

    private async Task<string> ExecAsync(string command)
    {
        if (_rcon is null || !_rcon.IsConnected) return "";
        try { return await _rcon.ExecuteAsync(command, CancellationToken.None); }
        catch (Exception ex) { AppendLog($"[error] {ex.Message}"); return ""; }
    }

    private async void OnRefresh(object? sender, EventArgs e) => await RefreshPlayersAsync();

    private async Task RefreshPlayersAsync()
    {
        var resp = await ExecAsync("players");
        var names = ParsePlayers(resp);
        _playersList.BeginUpdate();
        _playersList.Items.Clear();
        foreach (var n in names) _playersList.Items.Add(n);
        _playersList.EndUpdate();
        _onlineCountLabel.Text = $"Online: {names.Count}";
    }

    private static List<string> ParsePlayers(string raw)
    {
        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(raw)) return result;
        foreach (var lineRaw in raw.Replace("\r\n", "\n").Split('\n'))
        {
            var line = lineRaw.Trim();
            if (line.Length == 0) continue;
            if (Regex.IsMatch(line, @"^\s*[Pp]layers\s+connected")) continue;
            if (line.StartsWith("-")) line = line[1..].Trim();
            if (line.Contains(','))
            {
                foreach (var part in line.Split(','))
                {
                    var p = part.Trim().Trim('"');
                    if (p.Length > 0) result.Add(p);
                }
            }
            else
            {
                line = line.Trim('"');
                if (line.Length > 0) result.Add(line);
            }
        }
        return result;
    }

    private void OnAutoRefreshChanged(object? sender, EventArgs e)
    {
        if (_autoRefreshCheck.Checked) _autoRefreshTimer.Start();
        else _autoRefreshTimer.Stop();
    }

    private string? SelectedPlayer() => _playersList.SelectedItem as string;

    private async void OnKick(object? sender, EventArgs e)
    {
        var who = SelectedPlayer();
        if (who is null) { ShowSelectPlayer(); return; }
        using var dlg = new TextPromptDialog("Kick player", $"{who}을(를) kick하는 이유:");
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        var safeWho = QuoteName(who);
        var cmd = string.IsNullOrWhiteSpace(dlg.Value)
            ? $"kickuser {safeWho}"
            : $"kickuser {safeWho} -r {QuoteName(dlg.Value)}";
        AppendLog($"> {cmd}");
        AppendLog(await ExecAsync(cmd));
        await RefreshPlayersAsync();
    }

    private async void OnBanSelected(object? sender, EventArgs e)
    {
        var who = SelectedPlayer();
        if (who is null) { ShowSelectPlayer(); return; }
        if (!Confirm($"{who}을(를) ban할까요?")) return;
        var cmd = $"banuser {QuoteName(who)}";
        AppendLog($"> {cmd}");
        AppendLog(await ExecAsync(cmd));
        await RefreshPlayersAsync();
    }

    private async void OnMessageSelected(object? sender, EventArgs e)
    {
        var who = SelectedPlayer();
        if (who is null) { ShowSelectPlayer(); return; }
        using var dlg = new TextPromptDialog("Whisper", $"{who}에게 보낼 메시지:");
        if (dlg.ShowDialog(this) != DialogResult.OK || string.IsNullOrEmpty(dlg.Value)) return;
        var cmd = $"servermsg {QuoteName(dlg.Value)}";
        AppendLog($"> {cmd}");
        AppendLog(await ExecAsync(cmd));
    }

    private async void OnGrantAdminSelected(object? sender, EventArgs e)
    {
        var who = SelectedPlayer();
        if (who is null) { ShowSelectPlayer(); return; }
        if (!Confirm($"{who}에게 admin 권한 부여할까요?")) return;
        var cmd = $"grantadmin {QuoteName(who)}";
        AppendLog($"> {cmd}");
        AppendLog(await ExecAsync(cmd));
    }

    private async void OnSetAccess(object? sender, EventArgs e)
    {
        var who = SelectedPlayer();
        if (who is null) { ShowSelectPlayer(); return; }
        var level = _accessLevelBox.SelectedItem as string ?? "none";
        var cmd = $"setaccesslevel {QuoteName(who)} {QuoteName(level)}";
        AppendLog($"> {cmd}");
        AppendLog(await ExecAsync(cmd));
    }

    private async void OnGiveItem(object? sender, EventArgs e)
    {
        var who = SelectedPlayer();
        if (who is null) { ShowSelectPlayer(); return; }
        using var dlg = new TextPromptDialog("Give item", "Item id (예: Base.Axe):");
        if (dlg.ShowDialog(this) != DialogResult.OK || string.IsNullOrEmpty(dlg.Value)) return;
        var cmd = $"additem {QuoteName(who)} {QuoteName(dlg.Value)}";
        AppendLog($"> {cmd}");
        AppendLog(await ExecAsync(cmd));
    }

    private async void OnTeleport(object? sender, EventArgs e)
    {
        var who = SelectedPlayer();
        if (who is null) { ShowSelectPlayer(); return; }
        using var dlg = new TextPromptDialog("Teleport", $"{who}을(를) 어떤 플레이어에게로 이동시킬까요?");
        if (dlg.ShowDialog(this) != DialogResult.OK || string.IsNullOrEmpty(dlg.Value)) return;
        var cmd = $"teleport {QuoteName(who)} {QuoteName(dlg.Value)}";
        AppendLog($"> {cmd}");
        AppendLog(await ExecAsync(cmd));
    }

    private async void OnWhitelistAdd(object? sender, EventArgs e)
    {
        var name = _whitelistInput.Text.Trim();
        if (string.IsNullOrEmpty(name)) return;
        using var dlg = new TextPromptDialog("Whitelist password", $"{name}의 비밀번호 (비워두면 addusertowhitelist):");
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        var cmd = string.IsNullOrEmpty(dlg.Value)
            ? $"addusertowhitelist {QuoteName(name)}"
            : $"adduser {QuoteName(name)} {QuoteName(dlg.Value)}";
        AppendLog($"> {cmd}");
        AppendLog(await ExecAsync(cmd));
        _whitelistInput.Clear();
    }

    private async void OnWhitelistRemove(object? sender, EventArgs e)
    {
        var name = _whitelistInput.Text.Trim();
        if (string.IsNullOrEmpty(name)) return;
        if (!Confirm($"{name}을(를) whitelist에서 제거할까요?")) return;
        var cmd = $"removeuserfromwhitelist {QuoteName(name)}";
        AppendLog($"> {cmd}");
        AppendLog(await ExecAsync(cmd));
        _whitelistInput.Clear();
    }

    private async void OnBanAdd(object? sender, EventArgs e)
    {
        var target = _banInput.Text.Trim();
        if (string.IsNullOrEmpty(target)) return;
        var bySteam = _banBySteamId.Checked;
        if (!Confirm($"{(bySteam ? "SteamID" : "사용자")} '{target}' 을 ban할까요?")) return;
        var cmd = bySteam ? $"banid {QuoteName(target)}" : $"banuser {QuoteName(target)}";
        AppendLog($"> {cmd}");
        AppendLog(await ExecAsync(cmd));
        _banInput.Clear();
    }

    private async void OnBanRemove(object? sender, EventArgs e)
    {
        var target = _banInput.Text.Trim();
        if (string.IsNullOrEmpty(target)) return;
        var bySteam = _banBySteamId.Checked;
        var cmd = bySteam ? $"unbanid {QuoteName(target)}" : $"unbanuser {QuoteName(target)}";
        AppendLog($"> {cmd}");
        AppendLog(await ExecAsync(cmd));
        _banInput.Clear();
    }

    private async void OnGrantAdmin(object? sender, EventArgs e)
    {
        var name = _adminInput.Text.Trim();
        if (string.IsNullOrEmpty(name)) return;
        var cmd = $"grantadmin {QuoteName(name)}";
        AppendLog($"> {cmd}");
        AppendLog(await ExecAsync(cmd));
        _adminInput.Clear();
    }

    private async void OnRemoveAdmin(object? sender, EventArgs e)
    {
        var name = _adminInput.Text.Trim();
        if (string.IsNullOrEmpty(name)) return;
        if (!Confirm($"{name}의 admin을 회수할까요?")) return;
        var cmd = $"removeadmin {QuoteName(name)}";
        AppendLog($"> {cmd}");
        AppendLog(await ExecAsync(cmd));
        _adminInput.Clear();
    }

    private async void OnSetAccessApply(object? sender, EventArgs e)
    {
        var name = _adminInput.Text.Trim();
        if (string.IsNullOrEmpty(name)) return;
        var level = _adminLevelBox.SelectedItem as string ?? "none";
        var cmd = $"setaccesslevel {QuoteName(name)} {QuoteName(level)}";
        AppendLog($"> {cmd}");
        AppendLog(await ExecAsync(cmd));
    }

    private static string QuoteName(string s)
    {
        if (s.Contains(' ') || s.Contains('"') || s.Length == 0)
            return $"\"{s.Replace("\"", "\\\"")}\"";
        return s;
    }

    private bool Confirm(string text)
        => MessageBox.Show(this, text, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    private void ShowSelectPlayer()
        => MessageBox.Show(this, "리스트에서 플레이어를 먼저 선택하세요.",
            "No player selected", MessageBoxButtons.OK, MessageBoxIcon.Information);

    private void AppendLog(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        _logBox.AppendText(text);
        if (!text.EndsWith('\n')) _logBox.AppendText(Environment.NewLine);
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        _autoRefreshTimer.Stop();
        _autoRefreshTimer.Dispose();
        if (_rcon is not null) _rcon.ConnectionChanged -= OnConnectionChanged;
        base.OnHandleDestroyed(e);
    }
}
