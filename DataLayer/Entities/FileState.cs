namespace DataLayer.Entities;

public class FileState
{
    public string FileName { get; set; }
    public FileOperation Operation { get; set; }
    public FileVersion CurrentVersion { get; set; }
}