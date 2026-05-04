namespace PZServerManager.Models;

public sealed class LocalMod
{
    public long WorkshopId { get; set; }
    public string ModId { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string ModFolder { get; set; } = "";
    public string PosterPath { get; set; } = "";
    public string Description { get; set; } = "";
    public List<string> MapNames { get; set; } = new();
    public List<string> Requires { get; set; } = new();
    public string VersionMin { get; set; } = "";
    public string VersionMax { get; set; } = "";

    public bool IsMapMod => MapNames.Count > 0;
    public override string ToString() => string.IsNullOrEmpty(DisplayName) ? ModId : DisplayName;
}
