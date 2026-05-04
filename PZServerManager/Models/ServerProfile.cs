namespace PZServerManager.Models;

public sealed class ServerProfile
{
    public const string Default = "servertest";

    public string Name { get; set; } = Default;

    public override string ToString() => Name;
}
