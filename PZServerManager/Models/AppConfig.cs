namespace PZServerManager.Models;

public sealed class AppConfig
{
    public string SteamCmdDir { get; set; } = "";
    public string ServerDir { get; set; } = "";
    public string? SteamWebApiKey { get; set; }

    // ---- backup & schedule ----
    public string BackupDir { get; set; } = "";
    public int BackupRetention { get; set; } = 5;

    public bool AutoBackupEnabled { get; set; } = false;
    public double AutoBackupIntervalHours { get; set; } = 4;
    public DateTime? LastAutoBackupAt { get; set; }

    public bool AutoRestartEnabled { get; set; } = false;
    public double AutoRestartIntervalHours { get; set; } = 24;
    public DateTime? LastAutoRestartAt { get; set; }

    public bool AutoRestartOnCrash { get; set; } = true;

    public bool IsBootstrapped =>
        !string.IsNullOrWhiteSpace(SteamCmdDir) &&
        !string.IsNullOrWhiteSpace(ServerDir) &&
        File.Exists(Path.Combine(SteamCmdDir, "steamcmd.exe"));
}
