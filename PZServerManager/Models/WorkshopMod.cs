namespace PZServerManager.Models;

public sealed class WorkshopMod
{
    public long PublishedFileId { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string PreviewUrl { get; set; } = "";
    public long Subscriptions { get; set; }
    public double VoteScore { get; set; }
    public DateTime? LastUpdated { get; set; }
    public List<string> Tags { get; set; } = new();
}
