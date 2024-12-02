namespace DataLayer.Entities;

public class FileState
{
    public FileOperation Operation { get; set; }
    public FileVersion CurrentVersion { get; set; }
}