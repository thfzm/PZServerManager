using System.Diagnostics;
using PZServerManager.Models;

namespace PZServerManager.Services;

public sealed class ServerProcess : IDisposable
{
    private readonly PzPaths _paths;
    private Process? _proc;
    private ServerStatus _status = ServerStatus.Stopped;

    public ServerProcess(PzPaths paths)
    {
        _paths = paths;
    }

    public ServerStatus Status
    {
        get => _status;
        private set
        {
            if (_status == value) return;
            _status = value;
            StatusChanged?.Invoke(value);
        }
    }

    public bool IsRunning => _proc is { HasExited: false };
    public ServerProfile? CurrentProfile { get; private set; }
    public DateTime? StartedAt { get; private set; }

    public event Action<string>? OutputReceived;
    public event Action<ServerStatus>? StatusChanged;
    public event Action<int>? Exited;

    public void Start(ServerProfile profile)
    {
        if (IsRunning) throw new InvalidOperationException("Server is already running.");
        if (!File.Exists(_paths.StartServer64Bat))
            throw new FileNotFoundException("StartServer64.bat not found in server directory.", _paths.StartServer64Bat);

        var psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c \"\"{_paths.StartServer64Bat}\" -servername {profile.Name}\"",
            WorkingDirectory = _paths.ServerDir,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8,
            StandardErrorEncoding = System.Text.Encoding.UTF8,
        };

        _proc = new Process { StartInfo = psi, EnableRaisingEvents = true };
        _proc.OutputDataReceived += (_, e) => { if (e.Data != null) OutputReceived?.Invoke(e.Data); };
        _proc.ErrorDataReceived += (_, e) => { if (e.Data != null) OutputReceived?.Invoke(e.Data); };
        _proc.Exited += OnExited;

        Status = ServerStatus.Starting;
        CurrentProfile = profile;
        OutputReceived?.Invoke($"[manager] starting server '{profile.Name}' from {_paths.ServerDir}");

        _proc.Start();
        _proc.BeginOutputReadLine();
        _proc.BeginErrorReadLine();
        StartedAt = DateTime.Now;
        Status = ServerStatus.Running;
    }

    public void Send(string command)
    {
        if (!IsRunning) throw new InvalidOperationException("Server is not running.");
        _proc!.StandardInput.WriteLine(command);
        _proc.StandardInput.Flush();
    }

    public async Task StopAsync(TimeSpan timeout)
    {
        if (!IsRunning) return;
        Status = ServerStatus.Stopping;
        OutputReceived?.Invoke("[manager] sending 'quit' to server");
        try { Send("quit"); } catch { }

        var p = _proc!;
        try
        {
            using var cts = new CancellationTokenSource(timeout);
            await p.WaitForExitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            OutputReceived?.Invoke("[manager] graceful stop timed out, killing process tree");
            Kill();
        }
    }

    public void Kill()
    {
        if (_proc is null) return;
        try { if (!_proc.HasExited) _proc.Kill(entireProcessTree: true); } catch { }
    }

    private void OnExited(object? sender, EventArgs e)
    {
        var code = _proc?.ExitCode ?? -1;
        var wasStopping = Status == ServerStatus.Stopping;
        Status = wasStopping || code == 0 ? ServerStatus.Stopped : ServerStatus.Crashed;
        OutputReceived?.Invoke($"[manager] server exited with code {code}");
        Exited?.Invoke(code);
    }

    public void Dispose()
    {
        try { Kill(); } catch { }
        _proc?.Dispose();
        _proc = null;
    }
}
