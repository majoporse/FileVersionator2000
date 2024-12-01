namespace DataLayer.Entities;

public class FileState
{
    public FileOperation LastOperation { get; set; }
    public FileVersion CurrentVersion { get; set; }
}