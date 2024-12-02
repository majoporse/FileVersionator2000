namespace BusinessLayer.DTOs;

public class FileIsTrackedDto
{
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public bool IsSelected { get; set; } // For tracking selection
}