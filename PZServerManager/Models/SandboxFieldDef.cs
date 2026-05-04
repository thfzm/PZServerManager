namespace PZServerManager.Models;

public enum SandboxFieldType
{
    Int,
    IntChoices,
    Bool,
    Float,
}

public sealed record SandboxChoice(int Value, string Label);

public sealed record SandboxFieldDef(
    string Path,
    string Label,
    SandboxFieldType Type,
    string Category,
    string? Description = null,
    string? DefaultValue = null,
    int? IntMin = null,
    int? IntMax = null,
    IReadOnlyList<SandboxChoice>? Choices = null);
