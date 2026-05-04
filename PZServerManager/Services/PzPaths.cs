using PZServerManager.Models;

namespace PZServerManager.Services;

/// All filesystem paths the app cares about — split into "where the dedicated server lives"
/// (under ServerDir, configurable) vs "where PZ writes runtime data"
/// (always %UserProfile%\Zomboid\, hardcoded by PZ).
public sealed class PzPaths
{
    public string ServerDir { get; }

    public PzPaths(string serverDir) { ServerDir = serverDir ?? ""; }

    // ---- under ServerDir ----
    public string StartServer64Bat => Path.Combine(ServerDir, "StartServer64.bat");
    public string ProjectZomboidServerExe => Path.Combine(ServerDir, "ProjectZomboidServer.exe");
    public string WorkshopContentDir => Path.Combine(ServerDir, "steamapps", "workshop", "content", "108600");

    // ---- under %UserProfile%\Zomboid (hardcoded by PZ) ----
    public string ZomboidUserDir => AppPaths.ZomboidUserDir;
    public string ServerConfigDir => Path.Combine(ZomboidUserDir, "Server");
    public string SavesDir => Path.Combine(ZomboidUserDir, "Saves");
    public string MultiplayerSavesDir => Path.Combine(SavesDir, "Multiplayer");
    public string LogsDir => Path.Combine(ZomboidUserDir, "Logs");

    // ---- per-profile config files ----
    public string IniPath(ServerProfile p) => Path.Combine(ServerConfigDir, $"{p.Name}.ini");
    public string SandboxLuaPath(ServerProfile p) => Path.Combine(ServerConfigDir, $"{p.Name}_SandboxVars.lua");
    public string SpawnRegionsLuaPath(ServerProfile p) => Path.Combine(ServerConfigDir, $"{p.Name}_spawnregions.lua");

    public string SaveDir(ServerProfile p) => Path.Combine(MultiplayerSavesDir, p.Name);

    /// Profile names are derived from the *.ini files PZ has actually written (it creates them
    /// on first server boot). If the dir doesn't exist yet (server has never run), we yield nothing —
    /// callers fall back to ServerProfile.Default.
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
