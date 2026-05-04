using PZServerManager.Models;
using PZServerManager.Services;

namespace PZServerManager.Tabs;

public partial class ConsoleControl : UserControl
{
    private const int MaxLogLines = 5000;
    private ServerProcess? _server;

    public ConsoleControl()
    {
        InitializeComponent();
        UpdateStdinEnabled(ServerStatus.Stopped);
    }

    public void Bind(ServerProcess server)
    {
        _server = server;
        _server.OutputReceived += OnOutput;
        _server.StatusChanged += OnStatusChanged;
        UpdateStdinEnabled(_server.Status);
    }

    private void OnOutput(string line)
    {
        if (IsDisposed) return;
        BeginInvoke(() => AppendLog(line));
    }

    private void OnStatusChanged(ServerStatus status)
    {
        if (IsDisposed) return;
        BeginInvoke(() => UpdateStdinEnabled(status));
    }

    private void UpdateStdinEnabled(ServerStatus status)
    {
        var running = status == ServerStatus.Running;
        _stdinBox.Enabled = running;
        _sendButton.Enabled = running;
    }

    private void AppendLog(string line)
    {
        _logBox.AppendText(line);
        _logBox.AppendText(Environment.NewLine);
        if (_logBox.Lines.Length > MaxLogLines)
        {
            var keep = string.Join(Environment.NewLine,
                _logBox.Lines.Skip(_logBox.Lines.Length - MaxLogLines));
            _logBox.Text = keep + Environment.NewLine;
            _logBox.SelectionStart = _logBox.Text.Length;
            _logBox.ScrollToCaret();
        }
    }

    private void OnSend(object? sender, EventArgs e)
    {
        if (_server is null || !_server.IsRunning) return;
        var text = _stdinBox.Text;
        if (string.IsNullOrEmpty(text)) return;
        try
        {
            _server.Send(text);
            AppendLog($"> {text}");
            _stdinBox.Clear();
        }
        catch (Exception ex) { AppendLog($"[manager] send error: {ex.Message}"); }
    }

    private void OnStdinKey(object? sender, KeyEventArgs e)
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
