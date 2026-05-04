using PZServerManager.Models;

namespace PZServerManager.Services;

/// Polls every minute and raises events when scheduled backups / restarts come due.
/// Reads + writes timestamps directly on the shared AppConfig so the schedule survives restarts.
/// Crash auto-restart is intentionally separate — that lives in MainForm where ServerProcess is.
public sealed class Scheduler : IDisposable
{
    private readonly AppConfig _config;
    private readonly Action<AppConfig> _save;
    private readonly System.Threading.Timer _timer;
    private bool _running;

    /// Raised on a thread-pool thread. Subscribers must marshal to UI if they touch controls.
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

    public void NotifyConfigChanged()
    {
        // No-op for now; the periodic tick re-reads _config each time.
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

    public TimeSpan? UntilNextBackup()
    {
        if (!_config.AutoBackupEnabled) return null;
        return DueIn(_config.LastAutoBackupAt, _config.AutoBackupIntervalHours);
    }

    public TimeSpan? UntilNextRestart()
    {
        if (!_config.AutoRestartEnabled) return null;
        return DueIn(_config.LastAutoRestartAt, _config.AutoRestartIntervalHours);
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
