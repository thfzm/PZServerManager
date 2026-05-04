namespace PZServerManager.Models;

public sealed record IniFieldDef(
    string Key,
    string Label,
    IniFieldType Type,
    string Category,
    string? Description = null,
    string? DefaultValue = null,
    int? IntMin = null,
    int? IntMax = null,
    double? FloatMin = null,
    double? FloatMax = null,
    bool IsPassword = false,
    bool IsManagedElsewhere = false);
