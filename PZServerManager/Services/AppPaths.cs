namespace PZServerManager.Services;

public static class AppPaths
{
    public static string AppDataDir { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PZServerManager");

    public static string ConfigFile { get; } = Path.Combine(AppDataDir, "config.json");

    public static string DefaultInstallRoot { get; } = Path.Combine(AppContext.BaseDirectory, "pz");
    public static string DefaultSteamCmdDir { get; } = Path.Combine(DefaultInstallRoot, "steamcmd");
    public static string DefaultServerDir { get; } = Path.Combine(DefaultInstallRoot, "server");

    public static string ZomboidUserDir { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "Zomboid");
}
