# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Windows-only GUI for managing a Project Zomboid dedicated server. Stack: **.NET 10 / WinForms** (`net10.0-windows`). Single-user desktop tool — no auth, no multi-tenancy.

## Build & Run

```bash
dotnet build                          # build solution
dotnet run --project PZServerManager  # run the app
```

Open `PZServerManager.sln` in Visual Studio for the WinForms designer. The designer requires Windows.

## Folder convention

```
PZServerManager/
  Forms/      # *.cs + *.Designer.cs pairs — one folder per form is fine when designer files grow
  Services/   # process control, file I/O, external API clients (no UI references)
  Models/     # plain data types shared across Services and Forms
  Program.cs
```

Forms must not call SteamCMD / RCON / file I/O directly — go through a Service. Services must not reference `System.Windows.Forms` types.

## Design principles (load-bearing)

1. **GUI-only — the user never edits PZ config files by hand.** Anything the user can configure must round-trip through the GUI. When a feature changes server behavior, find the underlying config field and surface it as a control; don't tell the user to edit the `.ini` themselves.
2. **The tool installs everything itself.** The user does not pre-install SteamCMD or the PZ dedicated server. On first run the app downloads SteamCMD and runs it to install the server. Never instruct the user to fetch external tools manually.
3. **Mod install is a transaction across multiple files.** Adding a Workshop mod must atomically update `servertest.ini` fields `WorkshopItems=`, `Mods=`, and (for map mods) `Map=`. Removing a mod must remove all three. Partial updates leave the server unable to boot.
4. **Server data lives outside the project dir.** PZ stores server config and saves at `%UserProfile%\Zomboid\Server\` and `%UserProfile%\Zomboid\Saves\`. Never write fixtures or test data there.

## App data layout

- App config: `%LocalAppData%\PZServerManager\config.json` — paths and settings (`Services/AppPaths.cs`, `Services/AppConfigStore.cs`).
- Default install root: `<exe-dir>\pz\` containing `steamcmd\` and `server\` subdirs (overridable in the first-run wizard).
- `AppConfig.IsBootstrapped` gates `MainForm`; if false, `Program.cs` shows `FirstRunForm` instead.

## Key external integrations

- **SteamCMD** — bundled-on-demand. The app downloads `steamcmd.zip` from `https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip` into the configured SteamCMD dir on first run, then invokes `steamcmd.exe` via `Process` for both server install/update (app `380870`) and Workshop downloads (app `108600`). Wrapper: `Services/SteamCmd.cs`. Output streams via `IProgress<string>` so any caller (wizard, mod manager) can show live logs.
- **Steam Web API** — mod search. Endpoint `IPublishedFileService/QueryFiles`. Requires a user-supplied API key (stored in `AppConfig.SteamWebApiKey`). Filter by app `108600`.
- **Source RCON** — runtime server control (kick, ban, broadcast, save, quit). PZ exposes RCON when `RCONPort` and `RCONPassword` are set in `servertest.ini`.
- **Process management** — server is launched via `StartServer64.bat` (or directly `ProjectZomboidServer.exe`) under the configured `ServerDir`. Capture stdout/stderr and stream to a log view; the server console accepts commands on stdin.

App IDs to remember: **`380870`** = PZ Dedicated Server, **`108600`** = PZ (game; used for Workshop content).

When adding a new integration, put the client in `Services/` with no UI dependency, return plain models from `Models/`, and let Forms wire it up.

## Notes

- The project targets `net10.0-windows`; `UseWindowsForms=true` and nullable reference types are on.
- Long-running operations in Forms must run async with `IProgress<string>` (or equivalent) for log streaming and a `CancellationToken` for cancel buttons. Pattern: see `FirstRunForm.OnInstall`.
- Repo is not yet under git. There is no test project yet — add one as `tests/PZServerManager.Tests/` (xUnit) when the first Service is non-trivial enough to warrant it.
