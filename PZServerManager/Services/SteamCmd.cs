using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http;
using System.Text;

namespace PZServerManager.Services;

/// SteamCMD wrapper with the three quirks already burned-in from prior debugging:
///   1. The bootstrap binary self-updates and exits with code 7 — we auto-retry once.
///   2. Right after extraction the binary needs a one-shot "+quit" to settle (Prewarm)
///      so the actual install doesn't suffer the same self-update interruption.
///   3. Fresh SteamCMD has no app-info cache → app_update fails with
///      "Failed to install app '380870' (Missing configuration)" / exit 8 unless
///      `+app_info_update 1` runs after login.
public sealed class SteamCmd
{
    private const string ZipUrl = "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip";
    private const int PzServerAppId = 380870;
    private const int PzGameAppId = 108600; // Workshop content lives under this id

    public string InstallDir { get; }
    public string ExePath => Path.Combine(InstallDir, "steamcmd.exe");
    public bool IsInstalled => File.Exists(ExePath);

    public SteamCmd(string installDir)
    {
        InstallDir = installDir;
    }

    // ---------------- zip download + extract ----------------

    public async Task DownloadAndExtractAsync(IProgress<string>? log, IProgress<int>? percent, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(InstallDir))
            throw new InvalidOperationException("Install dir not set.");
        Directory.CreateDirectory(InstallDir);

        var zipPath = Path.Combine(InstallDir, "steamcmd.zip");
        log?.Report($"Downloading {ZipUrl}");
        percent?.Report(0);

        using (var http = new HttpClient { Timeout = TimeSpan.FromMinutes(2) })
        using (var resp = await http.GetAsync(ZipUrl, HttpCompletionOption.ResponseHeadersRead, ct))
        {
            resp.EnsureSuccessStatusCode();
            var total = resp.Content.Headers.ContentLength ?? -1;
            await using var src = await resp.Content.ReadAsStreamAsync(ct);
            await using var dst = File.Create(zipPath);
            var buf = new byte[81920];
            long copied = 0;
            int lastPct = -1;
            while (true)
            {
                var n = await src.ReadAsync(buf, ct);
                if (n <= 0) break;
                await dst.WriteAsync(buf.AsMemory(0, n), ct);
                copied += n;
                if (total > 0)
                {
                    var pct = (int)(copied * 95 / total); // last 5% reserved for extract
                    if (pct != lastPct) { percent?.Report(pct); lastPct = pct; }
                }
            }
        }

        log?.Report("Extracting…");
        ZipFile.ExtractToDirectory(zipPath, InstallDir, overwriteFiles: true);
        File.Delete(zipPath);
        percent?.Report(100);

        if (!IsInstalled)
            throw new InvalidOperationException($"Extraction failed; {ExePath} not found.");
        log?.Report("SteamCMD ready.");
    }

    // ---------------- run wrappers ----------------

    /// Force the bootstrap exe to self-update and settle. Cheap to call repeatedly —
    /// once SteamCMD is current, this is just a no-op `+quit`.
    public Task<int> PrewarmAsync(IProgress<string>? log, CancellationToken ct)
        => RunAsync("+quit", log, ct);

    public Task<int> InstallOrUpdatePzServerAsync(string serverDir, IProgress<string>? log, CancellationToken ct, bool validate = true)
    {
        if (string.IsNullOrWhiteSpace(serverDir))
            throw new InvalidOperationException("Server dir not set.");
        Directory.CreateDirectory(serverDir);
        var validateFlag = validate ? " validate" : "";
        var args = $"+force_install_dir \"{serverDir}\" +login anonymous +app_info_update 1 +app_update {PzServerAppId}{validateFlag} +quit";
        return RunAsync(args, log, ct);
    }

    public Task<int> DownloadWorkshopItemAsync(string serverDir, long workshopId, IProgress<string>? log, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(serverDir))
            throw new InvalidOperationException("Server dir not set.");
        var args = $"+force_install_dir \"{serverDir}\" +login anonymous +app_info_update 1 +workshop_download_item {PzGameAppId} {workshopId} +quit";
        return RunAsync(args, log, ct);
    }

    /// Public so SettingsControl can run validate / custom commands. Auto-retries exit 7.
    public async Task<int> RunAsync(string arguments, IProgress<string>? log, CancellationToken ct)
    {
        const int maxRetries = 2;
        int exit = 0;
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            exit = await RunOnceAsync(arguments, log, ct);
            if (exit != 7) return exit;
            log?.Report("[manager] SteamCMD self-updated (exit 7); rerunning.");
        }
        return exit;
    }

    private async Task<int> RunOnceAsync(string arguments, IProgress<string>? log, CancellationToken ct)
    {
        if (!IsInstalled) throw new InvalidOperationException("SteamCMD is not installed.");

        var psi = new ProcessStartInfo
        {
            FileName = ExePath,
            Arguments = arguments,
            WorkingDirectory = InstallDir,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };

        using var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };
        proc.OutputDataReceived += (_, e) => { if (e.Data is not null) log?.Report(e.Data); };
        proc.ErrorDataReceived += (_, e) => { if (e.Data is not null) log?.Report(e.Data); };

        log?.Report($"> steamcmd.exe {arguments}");
        proc.Start();
        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();

        using (ct.Register(() =>
        {
            try { if (!proc.HasExited) proc.Kill(entireProcessTree: true); } catch { }
        }))
        {
            await proc.WaitForExitAsync(ct);
        }
        return proc.ExitCode;
    }
}
