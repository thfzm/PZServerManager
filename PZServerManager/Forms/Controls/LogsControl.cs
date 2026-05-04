using System.Diagnostics;
using PZServerManager.Services;

namespace PZServerManager.Forms.Controls;

public partial class LogsControl : UserControl
{
    private PzPaths? _paths;
    private string? _currentFile;
    private long _lastReadLength;
    private readonly System.Windows.Forms.Timer _tailTimer = new() { Interval = 1500 };

    public LogsControl()
    {
        InitializeComponent();
        _tailTimer.Tick += (_, _) => TailRefresh();
    }

    public void Bind(PzPaths paths)
    {
        _paths = paths;
        Refresh();
    }

    public new void Refresh() => OnRefresh(this, EventArgs.Empty);

    private void OnRefresh(object? sender, EventArgs e)
    {
        if (_paths is null) return;
        _filesList.BeginUpdate();
        _filesList.Items.Clear();
        if (Directory.Exists(_paths.LogsDir))
        {
            var files = new DirectoryInfo(_paths.LogsDir)
                .GetFiles("*.txt")
                .OrderByDescending(f => f.LastWriteTime)
                .ToList();
            foreach (var f in files)
            {
                var lvi = new ListViewItem(f.Name) { Tag = f.FullName };
                lvi.SubItems.Add(f.Length < 1024 ? $"{f.Length} B" :
                                  f.Length < 1024 * 1024 ? $"{f.Length / 1024} KB" :
                                  $"{f.Length / 1024 / 1024} MB");
                lvi.SubItems.Add(f.LastWriteTime.ToString("yyyy-MM-dd HH:mm"));
                _filesList.Items.Add(lvi);
            }
            _statusLabel.Text = $"{files.Count} files in {_paths.LogsDir}";
        }
        else
        {
            _statusLabel.Text = $"{_paths.LogsDir} does not exist (server hasn't run yet).";
        }
        _filesList.EndUpdate();
    }

    private void OnFileSelected(object? sender, EventArgs e)
    {
        if (_filesList.SelectedItems.Count == 0) return;
        var path = _filesList.SelectedItems[0].Tag as string;
        if (string.IsNullOrEmpty(path)) return;
        _currentFile = path;
        _lastReadLength = 0;
        _logBox.Clear();
        AppendNewBytes();
    }

    private void AppendNewBytes()
    {
        if (string.IsNullOrEmpty(_currentFile) || !File.Exists(_currentFile)) return;
        try
        {
            using var fs = new FileStream(_currentFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if (fs.Length == _lastReadLength) return;
            if (fs.Length < _lastReadLength)
            {
                // file truncated / rotated
                _logBox.Clear();
                _lastReadLength = 0;
            }
            fs.Seek(_lastReadLength, SeekOrigin.Begin);
            using var sr = new StreamReader(fs);
            var newText = sr.ReadToEnd();
            _lastReadLength = fs.Position;
            _logBox.AppendText(newText);
            _logBox.SelectionStart = _logBox.TextLength;
            _logBox.ScrollToCaret();
        }
        catch (Exception ex)
        {
            _logBox.AppendText($"\n[error reading file: {ex.Message}]");
        }
    }

    private void TailRefresh()
    {
        if (string.IsNullOrEmpty(_currentFile)) return;
        AppendNewBytes();
    }

    private void OnTailToggle(object? sender, EventArgs e)
    {
        if (_tailCheck.Checked) _tailTimer.Start();
        else _tailTimer.Stop();
    }

    private void OnOpenFolder(object? sender, EventArgs e)
    {
        if (_paths is null) return;
        if (!Directory.Exists(_paths.LogsDir))
        {
            MessageBox.Show(this, $"{_paths.LogsDir} does not exist yet.",
                "Logs", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        Process.Start(new ProcessStartInfo { FileName = _paths.LogsDir, UseShellExecute = true });
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        _tailTimer.Stop();
        _tailTimer.Dispose();
        base.OnHandleDestroyed(e);
    }
}
