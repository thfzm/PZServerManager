using System.Diagnostics;
using System.Text;
using PZServerManager.Models;

namespace PZServerManager.Services;

/// Owns the running PZ dedicated server process.
/// Launches via `cmd /c StartServer64.bat -servername <name>` so the JVM gets the same
/// stdin/stdout/stderr handles we redirect — PZ accepts console commands (save/quit/players)
/// on stdin which is how we shut it down gracefully.
public sealed class ServerProcess : IDisposable
{
    private readonly PzPaths _paths;
    private Process? _proc;
    private ServerStatus _status = ServerStatus.Stopped;

    public ServerProcess(PzPaths paths) { _paths = paths; }

    public ServerProfile? CurrentProfile { get; private set; }
    public bool IsRunning => _proc is { HasExited: false };
    public DateTime? StartedAt { get; private set; }

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

    /// Each line of stdout/stderr from the JVM. Fires on a thread-pool thread —
    /// the UI side must marshal back to the UI thread.
    public event Action<string>? OutputReceived;
    public event Action<ServerStatus>? StatusChanged;
    public event Action<int>? Exited;

    public void Start(ServerProfile profile)
    {
        if (IsRunning) throw new InvalidOperationException("서버가 이미 실행 중입니다.");
        if (string.IsNullOrWhiteSpace(_paths.ServerDir))
            throw new InvalidOperationException("서버 경로가 설정되지 않았습니다 (초기 설정을 먼저 완료하세요).");
        if (!File.Exists(_paths.StartServer64Bat))
            throw new FileNotFoundException("StartServer64.bat을 찾을 수 없습니다.", _paths.StartServer64Bat);

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
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };

        _proc = new Process { StartInfo = psi, EnableRaisingEvents = true };
        _proc.OutputDataReceived += (_, e) => { if (e.Data is not null) OutputReceived?.Invoke(e.Data); };
        _proc.ErrorDataReceived += (_, e) => { if (e.Data is not null) OutputReceived?.Invoke(e.Data); };
        _proc.Exited += OnExited;

        Status = ServerStatus.Starting;
        CurrentProfile = profile;
        OutputReceived?.Invoke($"[manager] 서버 시작 — 프로필 '{profile.Name}', dir '{_paths.ServerDir}'");

        _proc.Start();
        _proc.BeginOutputReadLine();
        _proc.BeginErrorReadLine();
        StartedAt = DateTime.Now;
        Status = ServerStatus.Running;
    }

    /// Pushes one line to the server's stdin (JVM console).
    /// PZ accepts: save / quit / players / kickuser / banuser / servermsg / etc.
    public void Send(string command)
    {
        if (!IsRunning) throw new InvalidOperationException("서버가 실행 중이 아닙니다.");
        _proc!.StandardInput.WriteLine(command);
        _proc.StandardInput.Flush();
    }

    /// Graceful shutdown: send `quit`, wait up to <timeout>, kill process tree if it's still alive.
    public async Task StopAsync(TimeSpan timeout)
    {
        if (!IsRunning) return;
        Status = ServerStatus.Stopping;
        OutputReceived?.Invoke("[manager] 'quit' 전송 — 종료 대기 중");
        try { Send("quit"); } catch { /* pipe may already be gone */ }

        try
        {
            using var cts = new CancellationTokenSource(timeout);
            await _proc!.WaitForExitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            OutputReceived?.Invoke("[manager] graceful 종료 timeout — 프로세스 트리 강제 종료");
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
        Status = (wasStopping || code == 0) ? ServerStatus.Stopped : ServerStatus.Crashed;
        OutputReceived?.Invoke($"[manager] 서버 종료 (exit {code})");
        Exited?.Invoke(code);
    }

    public void Dispose()
    {
        try { Kill(); } catch { }
        _proc?.Dispose();
        _proc = null;
    }
}
