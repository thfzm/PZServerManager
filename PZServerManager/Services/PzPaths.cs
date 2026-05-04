using PZServerManager.Models;

namespace PZServerManager.Services;

public sealed class PzPaths
{
    public string ServerDir { get; }

    public PzPaths(string serverDir)
    {
        ServerDir = serverDir;
    }

    public string StartServer64Bat => Path.Combine(ServerDir, "StartServer64.bat");
    public string ServerExe => Path.Combine(ServerDir, "ProjectZomboidServer.exe");
    public string Server64Exe => Path.Combine(ServerDir, "ProjectZomboid64.exe");

    public string ZomboidDir => AppPaths.ZomboidUserDir;
    public string ServerConfigDir => Path.Combine(ZomboidDir, "Server");
    public string SavesDir => Path.Combine(ZomboidDir, "Saves");
    public string MultiplayerSavesDir => Path.Combine(SavesDir, "Multiplayer");
    public string LogsDir => Path.Combine(ZomboidDir, "Logs");
    public string DbDir => Path.Combine(ZomboidDir, "db");

    public string IniPath(ServerProfile p) => Path.Combine(ServerConfigDir, $"{p.Name}.ini");
    public string SandboxLuaPath(ServerProfile p) => Path.Combine(ServerConfigDir, $"{p.Name}_SandboxVars.lua");
    public string SpawnRegionsLuaPath(ServerProfile p) => Path.Combine(ServerConfigDir, $"{p.Name}_spawnregions.lua");
    public string SpawnPointsLuaPath(ServerProfile p) => Path.Combine(ServerConfigDir, $"{p.Name}_spawnpoints.lua");

    public string WorkshopContentDir => Path.Combine(ServerDir, "steamapps", "workshop", "content", "108600");
    public string WorkshopAcfPath => Path.Combine(ServerDir, "steamapps", "workshop", "appworkshop_108600.acf");

    public string SaveDir(ServerProfile p) => Path.Combine(MultiplayerSavesDir, p.Name);

    public IEnumerable<string> ListProfileNames()
    {
        if (!Directory.Exists(ServerConfigDir)) yield break;
        foreach (var ini in Directory.EnumerateFiles(ServerConfigDir, "*.ini"))
        {
            var name = Path.GetFileNameWithoutExtension(ini);
            if (!string.IsNullOrWhiteSpace(name)) yield return name;
        }
    }
}
