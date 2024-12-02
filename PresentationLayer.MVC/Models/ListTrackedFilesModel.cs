using BusinessLayer.DTOs;

namespace PresentationLayer.MVC.Models;

public class ListTrackedFilesModel
{
    public List<FileStateName> FileStates { get; set; }
    public string DirectoryPath { get; set; }
}