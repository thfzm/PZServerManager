using PZServerManager.Models;
using PZServerManager.Services;

namespace PZServerManager.Forms.Controls;

public partial class ConsoleControl : UserControl
{
    private const int MaxBufferLines = 5000;

    private ServerProcess? _server;
    private PzPaths? _paths;

    public ConsoleControl()
    {
        InitializeComponent();
        _stopButton.Enabled = false;
        _sendButton.Enabled = false;
        _inputBox.Enabled = false;
    }

    public void Bind(PzPaths paths, ServerProcess server)
    {
        _paths = paths;
        _server = server;
        _server.OutputReceived += OnOutput;
        _server.StatusChanged += OnStatusChanged;
        ReloadProfiles();
        OnStatusChanged(_server.Status);
    }

    public void ReloadProfiles()
    {
        if (_paths is null) return;
        var profiles = _paths.ListProfileNames().ToList();
        if (profiles.Count == 0) profiles.Add(ServerProfile.Default);

        _profileBox.BeginUpdate();
        _profileBox.Items.Clear();
        foreach (var p in profiles) _profileBox.Items.Add(p);
        if (_profileBox.Items.Contains(ServerProfile.Default))
            _profileBox.SelectedItem = ServerProfile.Default;
        else
            _profileBox.SelectedIndex = 0;
        _profileBox.EndUpdate();
    }

    private void OnOutput(string line)
    {
        if (IsDisposed) return;
        BeginInvoke(() => AppendLine(line));
    }

    private void OnStatusChanged(ServerStatus status)
    {
        if (IsDisposed) return;
        BeginInvoke(() =>
        {
            _statusLabel.Text = $"Status: {status}";
            var running = status is ServerStatus.Starting or ServerStatus.Running;
            _startButton.Enabled = !running && _server is not null;
            _stopButton.Enabled = running;
            _sendButton.Enabled = status == ServerStatus.Running;
            _inputBox.Enabled = status == ServerStatus.Running;
            _profileBox.Enabled = !running;
        });
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

    private void OnStart(object? sender, EventArgs e)
    {
        if (_server is null) return;
        var name = (_profileBox.SelectedItem as string) ?? ServerProfile.Default;
        try
        {
            _server.Start(new ServerProfile { Name = name });
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Failed to start server",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void OnStop(object? sender, EventArgs e)
    {
        if (_server is null) return;
        _stopButton.Enabled = false;
        try { await _server.StopAsync(TimeSpan.FromSeconds(45)); }
        catch (Exception ex) { AppendLine($"[manager] stop error: {ex.Message}"); }
    }

    private void OnSend(object? sender, EventArgs e)
    {
        if (_server is null || !_server.IsRunning) return;
        var text = _inputBox.Text;
        if (string.IsNullOrEmpty(text)) return;
        try
        {
            _server.Send(text);
            AppendLine($"> {text}");
            _inputBox.Clear();
        }
        catch (Exception ex)
        {
            AppendLine($"[manager] send error: {ex.Message}");
        }
    }

    private void OnInputKey(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter && !e.Shift)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            OnSend(sender, EventArgs.Empty);
        }
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        if (_server is not null)
        {
            _server.OutputReceived -= OnOutput;
            _server.StatusChanged -= OnStatusChanged;
        }
        base.OnHandleDestroyed(e);
    }
}
