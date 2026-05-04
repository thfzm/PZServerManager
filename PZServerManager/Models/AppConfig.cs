namespace PZServerManager.Models;

public sealed class AppConfig
{
    public string SteamCmdDir { get; set; } = "";
    public string ServerDir { get; set; } = "";

    public bool IsBootstrapped =>
        !string.IsNullOrWhiteSpace(SteamCmdDir) &&
        !string.IsNullOrWhiteSpace(ServerDir) &&
        File.Exists(Path.Combine(SteamCmdDir, "steamcmd.exe"));
}
