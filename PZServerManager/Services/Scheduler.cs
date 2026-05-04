using PZServerManager.Models;

namespace PZServerManager.Services;

/// Polls every minute and raises events when scheduled backups / restarts come due.
/// Reads + writes timestamps directly on the shared AppConfig so the schedule survives restarts.
/// Crash auto-restart is *not* here — that lives in MainForm where ServerProcess is.
public sealed class Scheduler : IDisposable
{
    private readonly AppConfig _config;
    private readonly Action<AppConfig> _save;
    private readonly System.Threading.Timer _timer;
    private bool _running;

    /// Raised on a thread-pool thread. UI subscribers must marshal to UI thread.
    public event Action? BackupDue;
    public event Action? RestartDue;

    public Scheduler(AppConfig config, Action<AppConfig> save)
    {
        _config = config;
        _save = save;
        _timer = new System.Threading.Timer(OnTick, null, Timeout.Infinite, Timeout.Infinite);
    }

    public void Start()
    {
        if (_running) return;
        _running = true;
        _timer.Change(TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(1));
    }

    public void Stop()
    {
        if (!_running) return;
        _running = false;
        _timer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    public void MarkBackupDone()
    {
        _config.LastAutoBackupAt = DateTime.UtcNow;
        _save(_config);
    }

    public void MarkRestartDone()
    {
        _config.LastAutoRestartAt = DateTime.UtcNow;
        _save(_config);
    }

    private static TimeSpan DueIn(DateTime? last, double intervalHours)
    {
        if (intervalHours <= 0) return TimeSpan.Zero;
        var due = (last ?? DateTime.UtcNow) + TimeSpan.FromHours(intervalHours);
        var diff = due - DateTime.UtcNow;
        return diff < TimeSpan.Zero ? TimeSpan.Zero : diff;
    }

    private void OnTick(object? state)
    {
        try
        {
            if (_config.AutoBackupEnabled && DueIn(_config.LastAutoBackupAt, _config.AutoBackupIntervalHours) <= TimeSpan.Zero)
                BackupDue?.Invoke();
            if (_config.AutoRestartEnabled && DueIn(_config.LastAutoRestartAt, _config.AutoRestartIntervalHours) <= TimeSpan.Zero)
                RestartDue?.Invoke();
        }
        catch
        {
            // Never crash the timer thread — let the next tick try again.
        }
    }

    public void Dispose()
    {
        Stop();
        _timer.Dispose();
    }
}
