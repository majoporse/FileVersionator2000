namespace DataLayer.Entities;

public class FileVersion
{
    public int Version { get; set; }
    public long FileSize { get; set; }
    public string FileHash { get; set; }
    
    public DateTime CreatedAt { get; set; }
}