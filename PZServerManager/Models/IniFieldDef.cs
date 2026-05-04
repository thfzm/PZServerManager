namespace PZServerManager.Models;

public enum IniFieldType
{
    String,
    MultilineString,
    Int,
    Bool,
}

public sealed record IniFieldDef(
    string Key,
    string Label,
    IniFieldType Type,
    string Category,
    string? Description = null,
    string? DefaultValue = null,
    int? IntMin = null,
    int? IntMax = null,
    bool IsPassword = false,
    bool ManagedElsewhere = false);
