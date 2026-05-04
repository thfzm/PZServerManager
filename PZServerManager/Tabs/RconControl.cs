using PZServerManager.Models;
using PZServerManager.Services;

namespace PZServerManager.Tabs;

public partial class RconControl : UserControl
{
    private const int MaxLogLines = 5000;

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

    private void ReloadProfiles()
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

    private void LoadDefaultsFromIni()
    {
        if (_paths is null) return;
        var ini = IniFile.Load(_paths.IniPath(_profile));
        _hostBox.Text = "127.0.0.1";
        if (ini.TryGet("RCONPort", out var port) && int.TryParse(port, out var p))
            _portBox.Value = Math.Clamp(p, (int)_portBox.Minimum, (int)_portBox.Maximum);
        else _portBox.Value = 27015;
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
        _reloadButton.Enabled = !connected;
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
        if (_rcon.IsConnected) { _rcon.Disconnect(); return; }
        if (string.IsNullOrEmpty(_passwordBox.Text))
        {
            MessageBox.Show(this, "RCON 비밀번호가 비어있습니다. 설정 관리 > Server config에서 RCONPassword를 먼저 설정하세요.",
                "RCON", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        _connectButton.Enabled = false;
        try
        {
            await _rcon.ConnectAsync(_hostBox.Text.Trim(), (int)_portBox.Value, _passwordBox.Text,
                TimeSpan.FromSeconds(8), CancellationToken.None);
            AppendLog($"[manager] connected to {_hostBox.Text}:{_portBox.Value}");
        }
        catch (Exception ex)
        {
            AppendLog($"[manager] connect failed: {ex.Message}");
            MessageBox.Show(this, ex.Message, "RCON 연결 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _connectButton.Enabled = true;
        }
    }

    private void OnReloadDefaults(object? sender, EventArgs e) => LoadDefaultsFromIni();

    private void OnProfileChanged(object? sender, EventArgs e)
    {
        var name = _profileBox.SelectedItem as string;
        if (string.IsNullOrEmpty(name) || name == _profile.Name) return;
        _profile = new ServerProfile { Name = name };
        LoadDefaultsFromIni();
    }

    private async void OnSend(object? sender, EventArgs e) => await ExecAsync(_commandBox.Text);

    private async Task ExecAsync(string command)
    {
        if (_rcon is null || !_rcon.IsConnected || string.IsNullOrWhiteSpace(command)) return;
        AppendLog($"> {command}");
        try
        {
            var resp = await _rcon.ExecuteAsync(command, CancellationToken.None);
            AppendLog(resp);
        }
        catch (Exception ex) { AppendLog($"[manager] exec error: {ex.Message}"); }
        _commandBox.Clear();
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

    private async void OnQuickPlayers(object? sender, EventArgs e) => await ExecAsync("players");
    private async void OnQuickSave(object? sender, EventArgs e) => await ExecAsync("save");

    private async void OnQuickQuit(object? sender, EventArgs e)
    {
        var r = MessageBox.Show(this, "RCON으로 'quit' 보내서 graceful 종료할까요?",
            "RCON quit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (r != DialogResult.Yes) return;
        await ExecAsync("quit");
    }

    private async void OnQuickBroadcast(object? sender, EventArgs e)
    {
        using var dlg = new TextPromptDialog("Broadcast 메시지", "메시지:") { Value = "" };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        var msg = dlg.Value;
        if (string.IsNullOrEmpty(msg)) return;
        await ExecAsync($"servermsg \"{msg.Replace("\"", "\\\"")}\"");
    }

    private void AppendLog(string line)
    {
        if (string.IsNullOrEmpty(line)) return;
        _logBox.AppendText(line);
        _logBox.AppendText(Environment.NewLine);
        if (_logBox.Lines.Length > MaxLogLines)
        {
            var keep = string.Join(Environment.NewLine,
                _logBox.Lines.Skip(_logBox.Lines.Length - MaxLogLines));
            _logBox.Text = keep + Environment.NewLine;
            _logBox.SelectionStart = _logBox.Text.Length;
        }
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        if (_rcon is not null) _rcon.ConnectionChanged -= OnConnectionChanged;
        base.OnHandleDestroyed(e);
    }
}

internal sealed class TextPromptDialog : Form
{
    private readonly TextBox _box;

    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public string Value { get => _box.Text; set => _box.Text = value; }

    public TextPromptDialog(string title, string label)
    {
        Text = title;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(420, 140);
        Font = new Font("Segoe UI", 9.5f);

        var lbl = new Label { Text = label, AutoSize = true, Location = new Point(12, 12) };
        _box = new TextBox
        {
            Location = new Point(12, 38),
            Size = new Size(396, 23),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
        };
        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(244, 92), Size = new Size(80, 28), Anchor = AnchorStyles.Bottom | AnchorStyles.Right };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(330, 92), Size = new Size(80, 28), Anchor = AnchorStyles.Bottom | AnchorStyles.Right };
        Controls.Add(lbl); Controls.Add(_box); Controls.Add(ok); Controls.Add(cancel);
        AcceptButton = ok; CancelButton = cancel;
    }
}
