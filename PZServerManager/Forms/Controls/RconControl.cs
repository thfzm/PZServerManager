using PZServerManager.Models;
using PZServerManager.Services;

namespace PZServerManager.Forms.Controls;

public partial class RconControl : UserControl
{
    private const int MaxBufferLines = 5000;

    private PzPaths? _paths;
    private RconClient? _rcon;
    private ServerProfile _profile = new() { Name = ServerProfile.Default };

    public RconControl()
    {
        InitializeComponent();
        UpdateConnectedUi(false);
    }

    public void Bind(PzPaths paths, RconClient rcon)
    {
        _paths = paths;
        _rcon = rcon;
        _rcon.ConnectionChanged += OnConnectionChanged;
        ReloadProfiles();
        LoadDefaultsFromIni();
        UpdateConnectedUi(_rcon.IsConnected);
    }

    public void ReloadProfiles()
    {
        if (_paths is null) return;
        var profiles = _paths.ListProfileNames().ToList();
        if (profiles.Count == 0) profiles.Add(ServerProfile.Default);

        _profileBox.BeginUpdate();
        _profileBox.Items.Clear();
        foreach (var p in profiles) _profileBox.Items.Add(p);
        if (_profileBox.Items.Contains(_profile.Name))
            _profileBox.SelectedItem = _profile.Name;
        else
            _profileBox.SelectedIndex = 0;
        _profileBox.EndUpdate();
    }

    private void LoadDefaultsFromIni()
    {
        if (_paths is null) return;
        var ini = IniFile.Load(_paths.IniPath(_profile));
        _hostBox.Text = "127.0.0.1";
        if (ini.TryGet("RCONPort", out var port) && int.TryParse(port, out var p))
            _portBox.Value = Math.Min(Math.Max(p, 0), (int)_portBox.Maximum);
        else
            _portBox.Value = 27015;
        if (ini.TryGet("RCONPassword", out var pwd))
            _passwordBox.Text = pwd;
    }

    private void OnConnectionChanged(bool connected)
    {
        if (IsDisposed) return;
        BeginInvoke(() => UpdateConnectedUi(connected));
    }

    private void UpdateConnectedUi(bool connected)
    {
        _connectButton.Text = connected ? "Disconnect" : "Connect";
        _statusLabel.Text = connected
            ? $"Connected to {_rcon?.Host}:{_rcon?.Port}"
            : "Disconnected";
        _statusLabel.ForeColor = connected ? Color.SeaGreen : SystemColors.GrayText;
        _hostBox.Enabled = !connected;
        _portBox.Enabled = !connected;
        _passwordBox.Enabled = !connected;
        _profileBox.Enabled = !connected;
        _commandBox.Enabled = connected;
        _sendButton.Enabled = connected;
        _quickPlayers.Enabled = connected;
        _quickSave.Enabled = connected;
        _quickQuit.Enabled = connected;
        _quickBroadcast.Enabled = connected;
    }

    private async void OnConnect(object? sender, EventArgs e)
    {
        if (_rcon is null) return;
        if (_rcon.IsConnected)
        {
            _rcon.Disconnect();
            return;
        }
        var host = _hostBox.Text.Trim();
        var port = (int)_portBox.Value;
        var pwd = _passwordBox.Text;
        if (string.IsNullOrEmpty(pwd))
        {
            MessageBox.Show(this, "RCON password is empty. Set RCONPassword in Config first.",
                "RCON", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _connectButton.Enabled = false;
        try
        {
            await _rcon.ConnectAsync(host, port, pwd, TimeSpan.FromSeconds(8), CancellationToken.None);
            AppendLine($"[manager] connected to {host}:{port}");
        }
        catch (Exception ex)
        {
            AppendLine($"[manager] connect failed: {ex.Message}");
            MessageBox.Show(this, ex.Message, "RCON connect failed",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _connectButton.Enabled = true;
        }
    }

    private async void OnSend(object? sender, EventArgs e) => await ExecAsync(_commandBox.Text);

    private async Task ExecAsync(string command)
    {
        if (_rcon is null || !_rcon.IsConnected) return;
        if (string.IsNullOrWhiteSpace(command)) return;
        AppendLine($"> {command}");
        try
        {
            var resp = await _rcon.ExecuteAsync(command, CancellationToken.None);
            AppendLine(resp);
        }
        catch (Exception ex)
        {
            AppendLine($"[manager] exec error: {ex.Message}");
        }
        _commandBox.Clear();
    }

    private async void OnQuickPlayers(object? sender, EventArgs e) => await ExecAsync("players");
    private async void OnQuickSave(object? sender, EventArgs e) => await ExecAsync("save");

    private async void OnQuickQuit(object? sender, EventArgs e)
    {
        var r = MessageBox.Show(this, "Send 'quit' to the server (graceful shutdown)?",
            "RCON quit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (r != DialogResult.Yes) return;
        await ExecAsync("quit");
    }

    private async void OnQuickBroadcast(object? sender, EventArgs e)
    {
        var msg = PromptDialog.Ask(this, "Broadcast message", "Message:", "");
        if (string.IsNullOrEmpty(msg)) return;
        await ExecAsync($"servermsg \"{msg.Replace("\"", "\\\"")}\"");
    }

    private void OnCommandKey(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter && !e.Shift)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            OnSend(sender, EventArgs.Empty);
        }
    }

    private void OnProfileChanged(object? sender, EventArgs e)
    {
        var name = _profileBox.SelectedItem as string;
        if (string.IsNullOrEmpty(name) || name == _profile.Name) return;
        _profile = new ServerProfile { Name = name };
        LoadDefaultsFromIni();
    }

    private void AppendLine(string line)
    {
        _logBox.AppendText(line);
        _logBox.AppendText(Environment.NewLine);
        if (_logBox.Lines.Length > MaxBufferLines)
        {
            var keep = string.Join(Environment.NewLine, _logBox.Lines.Skip(_logBox.Lines.Length - MaxBufferLines));
            _logBox.Text = keep + Environment.NewLine;
            _logBox.SelectionStart = _logBox.Text.Length;
        }
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        if (_rcon is not null)
            _rcon.ConnectionChanged -= OnConnectionChanged;
        base.OnHandleDestroyed(e);
    }
}
