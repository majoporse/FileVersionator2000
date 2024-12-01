namespace DataLayer.Entities;

public class FileHistory
{
    public string FileName { get; set; }
    public List<FileState> History { get; set; }
}