using PZServerManager.Models;

namespace PZServerManager.Services;

public static class ServerConfigSchema
{
    public const string CatIdentity = "Identity";
    public const string CatNetwork = "Network";
    public const string CatGame = "Game";
    public const string CatPlayers = "Players";
    public const string CatSafehouse = "Safehouse";
    public const string CatSafety = "Safety";
    public const string CatBackups = "Backups";
    public const string CatRcon = "RCON";
    public const string CatDiscord = "Discord";
    public const string CatMisc = "Misc";

    public static readonly string[] CategoryOrder =
    {
        CatIdentity, CatNetwork, CatGame, CatPlayers,
        CatSafehouse, CatSafety, CatBackups, CatRcon, CatDiscord, CatMisc,
    };

    public static readonly IReadOnlyList<IniFieldDef> AllFields = new IniFieldDef[]
    {
        // ---- Identity ----
        new("PublicName", "Public name", IniFieldType.String, CatIdentity,
            "Server name shown in the browser.", "My PZ Server"),
        new("PublicDescription", "Description", IniFieldType.MultilineString, CatIdentity,
            "Description shown in the browser. Use \\n for line breaks (PZ doesn't support real newlines)."),
        new("ServerWelcomeMessage", "Welcome message", IniFieldType.MultilineString, CatIdentity,
            "Shown to players on join.", "Welcome to Project Zomboid Server."),
        new("Public", "Public listing", IniFieldType.Bool, CatIdentity,
            "Advertise on the public server browser.", "false"),
        new("Open", "Open to new players", IniFieldType.Bool, CatIdentity,
            "If false, only whitelisted accounts can join.", "true"),
        new("ResetID", "Reset ID", IniFieldType.Int, CatIdentity,
            "Changing this forces clients to reset world chunks; PZ manages this on first boot."),

        // ---- Network ----
        new("DefaultPort", "Game port (UDP)", IniFieldType.Int, CatNetwork,
            "Main server port. Default 16261.", "16261", IntMin: 1, IntMax: 65535),
        new("UDPPort", "Direct connect UDP", IniFieldType.Int, CatNetwork,
            null, "16262", IntMin: 1, IntMax: 65535),
        new("SteamPort1", "Steam port 1", IniFieldType.Int, CatNetwork, null, "8766", IntMin: 1, IntMax: 65535),
        new("SteamPort2", "Steam port 2", IniFieldType.Int, CatNetwork, null, "8767", IntMin: 1, IntMax: 65535),
        new("Password", "Server password", IniFieldType.String, CatNetwork, "Empty = no password.", IsPassword: true),
        new("MaxPlayers", "Max players", IniFieldType.Int, CatNetwork, null, "32", IntMin: 1, IntMax: 100),
        new("PingLimit", "Ping kick threshold (ms)", IniFieldType.Int, CatNetwork,
            "Kick players whose ping exceeds this. 0 disables.", "400", IntMin: 0, IntMax: 5000),
        new("KickFastPlayers", "Kick fast (cheating) players", IniFieldType.Bool, CatNetwork, null, "false"),
        new("VAC", "Steam VAC", IniFieldType.Bool, CatNetwork, null, "true"),
        new("ServerSteamAccount", "Steam game-server token (GSLT)", IniFieldType.String, CatNetwork,
            "Optional GSLT for the server browser.", IsPassword: true),

        // ---- Game ----
        new("Map", "Map", IniFieldType.String, CatGame,
            "Map list, semicolon-separated. Managed via the Mods tab when map mods are installed.",
            "Muldraugh, KY", ManagedElsewhere: true),
        new("Mods", "Mods (load order)", IniFieldType.String, CatGame,
            "Mod folder ids, semicolon-separated. Managed via the Mods tab.",
            ManagedElsewhere: true),
        new("WorkshopItems", "Workshop IDs", IniFieldType.String, CatGame,
            "Workshop publishedfileids, semicolon-separated. Managed via the Mods tab.",
            ManagedElsewhere: true),
        new("SpawnPoint", "Spawn point override", IniFieldType.String, CatGame,
            "Format: x,y,z. Empty = spawn regions."),
        new("SpawnItems", "Spawn items", IniFieldType.String, CatGame,
            "Comma-separated item ids given to new characters."),
        new("AllowCoop", "Allow co-op (split-screen) clients", IniFieldType.Bool, CatGame, null, "true"),
        new("PauseEmpty", "Pause time when empty", IniFieldType.Bool, CatGame, null, "true"),

        // ---- Players ----
        new("AutoCreateUserInWhiteList", "Auto-add joiners to whitelist", IniFieldType.Bool, CatPlayers, null, "false"),
        new("DropOffWhiteListAfterDeath", "Drop from whitelist on death", IniFieldType.Bool, CatPlayers, null, "false"),
        new("MaxAccountsPerUser", "Max accounts per user (0 = ∞)", IniFieldType.Int, CatPlayers, null, "0", IntMin: 0, IntMax: 100),
        new("AllowNonAsciiUsername", "Allow non-ASCII usernames", IniFieldType.Bool, CatPlayers, null, "false"),
        new("DisplayUserName", "Show usernames in-game", IniFieldType.Bool, CatPlayers, null, "true"),
        new("ShowFirstAndLastName", "Show first & last name", IniFieldType.Bool, CatPlayers, null, "false"),

        // ---- Safehouse ----
        new("AdminSafehouse", "Admin can claim safehouses", IniFieldType.Bool, CatSafehouse, null, "false"),
        new("SafehouseAllowTrepass", "Allow trespass", IniFieldType.Bool, CatSafehouse, null, "true"),
        new("SafehouseAllowFire", "Allow fire damage", IniFieldType.Bool, CatSafehouse, null, "true"),
        new("SafehouseAllowLoot", "Allow looting", IniFieldType.Bool, CatSafehouse, null, "true"),
        new("SafehouseAllowRespawn", "Allow respawn at safehouse", IniFieldType.Bool, CatSafehouse, null, "false"),
        new("SafehouseDaySurvivedToClaim", "Days survived before claim", IniFieldType.Int, CatSafehouse, null, "0", IntMin: 0, IntMax: 365),
        new("SafeHouseRemovalTime", "Inactive removal hours (144 = 6 days)", IniFieldType.Int, CatSafehouse, null, "144", IntMin: 0),
        new("SafehouseAllowNonResidential", "Allow non-residential buildings", IniFieldType.Bool, CatSafehouse, null, "false"),

        // ---- Safety ----
        new("SafetySystem", "PvP safety system", IniFieldType.Bool, CatSafety,
            "When enabled, players must toggle PvP off before being damaged.", "true"),
        new("ShowSafety", "Show safety badge above players", IniFieldType.Bool, CatSafety, null, "true"),
        new("SafetyToggleTimer", "Toggle delay (s)", IniFieldType.Int, CatSafety, null, "2", IntMin: 0, IntMax: 60),
        new("SafetyCooldownTimer", "Toggle cooldown (s)", IniFieldType.Int, CatSafety, null, "3", IntMin: 0, IntMax: 60),

        // ---- Backups ----
        new("BackupsCount", "Keep N backups", IniFieldType.Int, CatBackups, null, "5", IntMin: 0, IntMax: 100),
        new("BackupsOnStart", "Backup on server start", IniFieldType.Bool, CatBackups, null, "true"),
        new("BackupsOnVersionChange", "Backup on PZ version change", IniFieldType.Bool, CatBackups, null, "true"),
        new("BackupsPeriod", "Backup interval (minutes; 0 disables)", IniFieldType.Int, CatBackups, null, "0", IntMin: 0, IntMax: 10080),

        // ---- RCON ----
        new("RCONPort", "RCON port", IniFieldType.Int, CatRcon,
            "Set + RCONPassword to enable remote console.", "27015", IntMin: 0, IntMax: 65535),
        new("RCONPassword", "RCON password", IniFieldType.String, CatRcon, "Empty disables RCON.", IsPassword: true),

        // ---- Discord ----
        new("DiscordEnable", "Bridge in-game chat to Discord", IniFieldType.Bool, CatDiscord, null, "false"),
        new("DiscordToken", "Bot token", IniFieldType.String, CatDiscord, null, IsPassword: true),
        new("DiscordChannel", "Channel name", IniFieldType.String, CatDiscord),
        new("DiscordChannelID", "Channel ID", IniFieldType.String, CatDiscord),

        // ---- Misc ----
        new("AllowDestructionBySledgehammer", "Allow sledgehammer destruction", IniFieldType.Bool, CatMisc, null, "true"),
        new("LogLocalChat", "Log local chat", IniFieldType.Bool, CatMisc, null, "false"),
        new("BanKickGlobalSound", "Global ban/kick sound", IniFieldType.Bool, CatMisc, null, "true"),
        new("RemovePlayerCorpsesOnCorpseRemoval", "Remove player corpses on cleanup", IniFieldType.Bool, CatMisc, null, "false"),
        new("AntiCheatProtectionType1", "Anti-cheat type 1", IniFieldType.Bool, CatMisc, null, "true"),
        new("AntiCheatProtectionType2", "Anti-cheat type 2", IniFieldType.Bool, CatMisc, null, "true"),
        new("AntiCheatProtectionType3", "Anti-cheat type 3", IniFieldType.Bool, CatMisc, null, "true"),
        new("AntiCheatProtectionType4", "Anti-cheat type 4", IniFieldType.Bool, CatMisc, null, "true"),
        new("ClientCommandFilter", "Client command filter", IniFieldType.String, CatMisc,
            "Whitelist of admin commands clients are allowed to invoke."),
        new("ClientActionLogs", "Action logs", IniFieldType.String, CatMisc,
            "Comma-separated action types to log."),
    };

    public static readonly IReadOnlyDictionary<string, IniFieldDef> ByKey =
        AllFields.ToDictionary(f => f.Key, StringComparer.OrdinalIgnoreCase);

    public static IEnumerable<IniFieldDef> InCategory(string category)
        => AllFields.Where(f => f.Category == category);
}
