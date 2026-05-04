namespace PZServerManager.Models;

public sealed class LocalMod
{
    public long WorkshopId { get; set; }       // Steam Workshop publishedfileid
    public string ModId { get; set; } = "";    // mod.info `id=` (goes into Mods=)
    public string DisplayName { get; set; } = "";
    public string ModFolder { get; set; } = "";
    public string PosterPath { get; set; } = "";
    public string Description { get; set; } = "";
    public List<string> MapNames { get; set; } = new(); // dirs found under media/maps/<X>/map.info
    public List<string> Requires { get; set; } = new(); // mod.info `require=` deps

    public bool IsMapMod => MapNames.Count > 0;
    public override string ToString() => string.IsNullOrEmpty(DisplayName) ? ModId : DisplayName;
}
