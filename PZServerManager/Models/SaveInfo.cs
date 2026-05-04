namespace PZServerManager.Models;

public sealed class SaveInfo
{
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public long SizeBytes { get; set; }
    public DateTime LastModified { get; set; }
    public int BackupCount { get; set; }

    public string SizeDisplay
    {
        get
        {
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            double size = SizeBytes;
            int i = 0;
            while (size >= 1024 && i < units.Length - 1) { size /= 1024; i++; }
            return $"{size:0.##} {units[i]}";
        }
    }
}
