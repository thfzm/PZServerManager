using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http;

namespace PZServerManager.Services;

public sealed class SteamCmd
{
    private const string DownloadUrl = "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip";

    public string InstallDir { get; }
    public string ExePath => Path.Combine(InstallDir, "steamcmd.exe");
    public bool IsInstalled => File.Exists(ExePath);

    public SteamCmd(string installDir)
    {
        InstallDir = installDir;
    }

    public async Task DownloadAndExtractAsync(IProgress<string>? log, CancellationToken ct)
    {
        Directory.CreateDirectory(InstallDir);
        var zipPath = Path.Combine(InstallDir, "steamcmd.zip");

        log?.Report($"Downloading SteamCMD from {DownloadUrl}");
        using (var http = new HttpClient())
        using (var response = await http.GetAsync(DownloadUrl, HttpCompletionOption.ResponseHeadersRead, ct))
        {
            response.EnsureSuccessStatusCode();
            await using var src = await response.Content.ReadAsStreamAsync(ct);
            await using var dst = File.Create(zipPath);
            await src.CopyToAsync(dst, ct);
        }

        log?.Report($"Extracting to {InstallDir}");
        ZipFile.ExtractToDirectory(zipPath, InstallDir, overwriteFiles: true);
        File.Delete(zipPath);

        if (!IsInstalled)
            throw new InvalidOperationException($"SteamCMD extraction failed; {ExePath} not found.");

        log?.Report("SteamCMD installed.");
    }

    public async Task<int> RunAsync(string arguments, IProgress<string>? log, CancellationToken ct)
    {
        if (!IsInstalled)
            throw new InvalidOperationException("SteamCMD is not installed.");

        var psi = new ProcessStartInfo
        {
            FileName = ExePath,
            Arguments = arguments,
            WorkingDirectory = InstallDir,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8,
            StandardErrorEncoding = System.Text.Encoding.UTF8,
        };

        using var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };
        proc.OutputDataReceived += (_, e) => { if (e.Data != null) log?.Report(e.Data); };
        proc.ErrorDataReceived += (_, e) => { if (e.Data != null) log?.Report(e.Data); };

        log?.Report($"> steamcmd.exe {arguments}");
        proc.Start();
        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();

        using (ct.Register(() => { try { if (!proc.HasExited) proc.Kill(entireProcessTree: true); } catch { } }))
        {
            await proc.WaitForExitAsync(ct);
        }

        return proc.ExitCode;
    }

    public Task<int> InstallOrUpdatePzServerAsync(string serverDir, IProgress<string>? log, CancellationToken ct)
    {
        Directory.CreateDirectory(serverDir);
        var args = $"+force_install_dir \"{serverDir}\" +login anonymous +app_update 380870 validate +quit";
        return RunAsync(args, log, ct);
    }

    public Task<int> DownloadWorkshopItemAsync(string serverDir, long workshopId, IProgress<string>? log, CancellationToken ct)
    {
        var args = $"+force_install_dir \"{serverDir}\" +login anonymous +workshop_download_item 108600 {workshopId} +quit";
        return RunAsync(args, log, ct);
    }
}
