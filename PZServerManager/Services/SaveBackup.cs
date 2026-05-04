using System.IO.Compression;
using PZServerManager.Models;

namespace PZServerManager.Services;

public sealed class SaveBackup
{
    private readonly PzPaths _paths;
    private readonly Func<string> _backupDir;

    public SaveBackup(PzPaths paths, Func<string> backupDir)
    {
        _paths = paths;
        _backupDir = backupDir;
    }

    private string ResolveBackupDir()
    {
        var d = _backupDir();
        if (string.IsNullOrWhiteSpace(d))
            d = Path.Combine(AppPaths.AppDataDir, "backups");
        Directory.CreateDirectory(d);
        return d;
    }

    public List<SaveInfo> ListSaves()
    {
        var result = new List<SaveInfo>();
        var root = _paths.MultiplayerSavesDir;
        if (!Directory.Exists(root)) return result;

        var backupRoot = ResolveBackupDir();

        foreach (var dir in Directory.EnumerateDirectories(root))
        {
            var info = new DirectoryInfo(dir);
            result.Add(new SaveInfo
            {
                Name = info.Name,
                Path = info.FullName,
                LastModified = info.LastWriteTime,
                SizeBytes = SafeDirSize(info.FullName),
                BackupCount = ListBackupsIn(info.Name, backupRoot).Count,
            });
        }
        result.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
        return result;
    }

    public List<string> ListBackups(string saveName)
        => ListBackupsIn(saveName, ResolveBackupDir());

    private static List<string> ListBackupsIn(string saveName, string backupRoot)
    {
        if (!Directory.Exists(backupRoot)) return new List<string>();
        return Directory
            .EnumerateFiles(backupRoot, $"{saveName}__*.zip", SearchOption.TopDirectoryOnly)
            .OrderByDescending(p => p)
            .ToList();
    }

    private static long SafeDirSize(string path)
    {
        try
        {
            long total = 0;
            foreach (var f in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            {
                try { total += new FileInfo(f).Length; } catch { }
            }
            return total;
        }
        catch { return 0; }
    }

    public async Task<string> BackupAsync(string saveName, IProgress<string>? log, CancellationToken ct)
    {
        var src = Path.Combine(_paths.MultiplayerSavesDir, saveName);
        if (!Directory.Exists(src)) throw new DirectoryNotFoundException($"Save '{saveName}' not found.");
        var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var dst = Path.Combine(ResolveBackupDir(), $"{saveName}__{stamp}.zip");
        log?.Report($"[backup] zipping {src} → {dst}");
        await Task.Run(() =>
        {
            ct.ThrowIfCancellationRequested();
            ZipFile.CreateFromDirectory(src, dst, CompressionLevel.Optimal, includeBaseDirectory: false);
        }, ct);
        log?.Report($"[backup] done — {new FileInfo(dst).Length / 1024 / 1024} MB");
        return dst;
    }

    public async Task RestoreAsync(string saveName, string zipPath, IProgress<string>? log, CancellationToken ct)
    {
        if (!File.Exists(zipPath)) throw new FileNotFoundException("Backup not found.", zipPath);
        var dst = Path.Combine(_paths.MultiplayerSavesDir, saveName);
        if (Directory.Exists(dst))
        {
            log?.Report($"[restore] removing existing {dst}");
            await Task.Run(() => Directory.Delete(dst, recursive: true), ct);
        }
        Directory.CreateDirectory(dst);
        log?.Report($"[restore] extracting {zipPath} → {dst}");
        await Task.Run(() => ZipFile.ExtractToDirectory(zipPath, dst, overwriteFiles: true), ct);
        log?.Report("[restore] done");
    }

    public void Delete(string saveName)
    {
        var dir = Path.Combine(_paths.MultiplayerSavesDir, saveName);
        if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
    }

    public int Rotate(string saveName, int keep)
    {
        if (keep <= 0) return 0;
        var backups = ListBackups(saveName);
        var deleted = 0;
        for (int i = keep; i < backups.Count; i++)
        {
            try { File.Delete(backups[i]); deleted++; } catch { }
        }
        return deleted;
    }
}
