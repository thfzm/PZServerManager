using System.Text.RegularExpressions;
using PZServerManager.Forms;
using PZServerManager.Services;

namespace PZServerManager.Forms.Controls;

public partial class PlayersControl : UserControl
{
    private RconClient? _rcon;
    private readonly System.Windows.Forms.Timer _autoRefreshTimer = new() { Interval = 10_000 };

    public PlayersControl()
    {
        InitializeComponent();
        _autoRefreshTimer.Tick += async (_, _) => await RefreshPlayersAsync();
        _accessLevelBox.Items.AddRange(new object[]
        {
            "none", "admin", "moderator", "overseer", "gm", "observer",
        });
        _accessLevelBox.SelectedIndex = 0;
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
            ? "Connected — actions execute via RCON."
            : "Not connected. Open the RCON tab to connect first.";
        _bannerLabel.ForeColor = connected ? Color.SeaGreen : Color.OrangeRed;

        var ctlsToToggle = new Control[]
        {
            _refreshButton, _autoRefreshCheck, _kickButton, _banSelectedButton,
            _msgSelectedButton, _grantAdminSelectedButton, _setAccessButton,
            _giveItemButton, _teleportButton,
            _whitelistAddButton, _whitelistRemoveButton,
            _banAddButton, _banRemoveButton,
            _grantAdminButton, _removeAdminButton, _setAccessApplyButton,
        };
        foreach (var c in ctlsToToggle) c.Enabled = connected;

        if (!connected)
        {
            _autoRefreshTimer.Stop();
            _autoRefreshCheck.Checked = false;
        }
    }

    private async Task<string> ExecAsync(string command)
    {
        if (_rcon is null || !_rcon.IsConnected) return "";
        try
        {
            return await _rcon.ExecuteAsync(command, CancellationToken.None);
        }
        catch (Exception ex)
        {
            AppendLog($"[error] {ex.Message}");
            return "";
        }
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
            if (Regex.IsMatch(line, @"^\s*[Pp]layers\s+connected", RegexOptions.None)) continue;
            if (line.StartsWith("-")) line = line[1..].Trim();
            // Some versions output "Player1", "Player2" comma-separated
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

    private string? SelectedPlayer()
    {
        if (_playersList.SelectedItem is string s) return s;
        return null;
    }

    private async void OnKick(object? sender, EventArgs e)
    {
        var who = SelectedPlayer();
        if (who is null) { ShowSelectPlayer(); return; }
        var reason = PromptDialog.Ask(this, "Kick player", $"Reason for kicking {who}:", "");
        if (reason is null) return;
        var safeWho = QuoteName(who);
        var cmd = string.IsNullOrWhiteSpace(reason)
            ? $"kickuser {safeWho}"
            : $"kickuser {safeWho} -r {QuoteName(reason)}";
        AppendLog($"> {cmd}");
        AppendLog(await ExecAsync(cmd));
        await RefreshPlayersAsync();
    }

    private async void OnBanSelected(object? sender, EventArgs e)
    {
        var who = SelectedPlayer();
        if (who is null) { ShowSelectPlayer(); return; }
        if (!Confirm($"Ban player {who}?")) return;
        var cmd = $"banuser {QuoteName(who)}";
        AppendLog($"> {cmd}");
        AppendLog(await ExecAsync(cmd));
        await RefreshPlayersAsync();
    }

    private async void OnMessageSelected(object? sender, EventArgs e)
    {
        var who = SelectedPlayer();
        if (who is null) { ShowSelectPlayer(); return; }
        var msg = PromptDialog.Ask(this, "Whisper", $"Message to {who}:", "");
        if (string.IsNullOrEmpty(msg)) return;
        var cmd = $"servermsg {QuoteName(msg)}";
        AppendLog($"> {cmd}");
        AppendLog(await ExecAsync(cmd));
    }

    private async void OnGrantAdminSelected(object? sender, EventArgs e)
    {
        var who = SelectedPlayer();
        if (who is null) { ShowSelectPlayer(); return; }
        if (!Confirm($"Grant admin to {who}?")) return;
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
        var item = PromptDialog.Ask(this, "Give item", "Item id (e.g. Base.Axe):", "");
        if (string.IsNullOrEmpty(item)) return;
        var cmd = $"additem {QuoteName(who)} {QuoteName(item)}";
        AppendLog($"> {cmd}");
        AppendLog(await ExecAsync(cmd));
    }

    private async void OnTeleport(object? sender, EventArgs e)
    {
        var who = SelectedPlayer();
        if (who is null) { ShowSelectPlayer(); return; }
        var dst = PromptDialog.Ask(this, "Teleport", $"Teleport {who} to which player?", "");
        if (string.IsNullOrEmpty(dst)) return;
        var cmd = $"teleport {QuoteName(who)} {QuoteName(dst)}";
        AppendLog($"> {cmd}");
        AppendLog(await ExecAsync(cmd));
    }

    private async void OnWhitelistAdd(object? sender, EventArgs e)
    {
        var name = _whitelistInput.Text.Trim();
        if (string.IsNullOrEmpty(name)) return;
        var pwd = PromptDialog.Ask(this, "Whitelist password",
            $"Password for {name} (leave empty to use addusertowhitelist):", "", isPassword: true);
        if (pwd is null) return;
        var cmd = string.IsNullOrEmpty(pwd)
            ? $"addusertowhitelist {QuoteName(name)}"
            : $"adduser {QuoteName(name)} {QuoteName(pwd)}";
        AppendLog($"> {cmd}");
        AppendLog(await ExecAsync(cmd));
        _whitelistInput.Clear();
    }

    private async void OnWhitelistRemove(object? sender, EventArgs e)
    {
        var name = _whitelistInput.Text.Trim();
        if (string.IsNullOrEmpty(name)) return;
        if (!Confirm($"Remove {name} from whitelist?")) return;
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
        if (!Confirm($"Ban {(bySteam ? "SteamID" : "user")} '{target}'?")) return;
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
        if (!Confirm($"Remove admin from {name}?")) return;
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
        => MessageBox.Show(this, "Select a player from the list first.",
            "No player selected", MessageBoxButtons.OK, MessageBoxIcon.Information);

    private void AppendLog(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        _logBox.AppendText(text);
        if (!text.EndsWith("\n")) _logBox.AppendText(Environment.NewLine);
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        _autoRefreshTimer.Stop();
        if (_rcon is not null) _rcon.ConnectionChanged -= OnConnectionChanged;
        base.OnHandleDestroyed(e);
    }
}
