using DataLayer.Entities;

namespace BusinessLayer.DTOs;

public class LastFileOperationDto
{
    public string FileName { get; set; }
    public FileOperation Operation { get; set; }
}