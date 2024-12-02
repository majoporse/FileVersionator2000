using BusinessLayer.DTOs;
using DataLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using PresentationLayer.MVC.Models;

namespace PresentationLayer.MVC.Controllers;

public class FileController : Controller
{
    public IActionResult Files()
    {
        // Simulated tracked files
        var trackedFiles = new ListTrackedFilesModel
        {
            DirectoryPath = "C:\\Projects\\TrackedDirectory",
            FileStates = new List<FileStateName>
            {
                new FileStateName()
                {
                    Name = "file1.txt",
                    FileState = new FileState
                    {
                        Operation = FileOperation.Create,
                        CurrentVersion = new FileVersion
                        {
                            Version = 1,
                            FileSize = 1024,
                            FileHash = "hash123",
                            CreatedAt = DateTime.Now.AddMinutes(-30)
                        }
                    }
                },
                new FileStateName
                {
                    Name = "file2.txt",
                    FileState = new FileState
                    {
                        Operation = FileOperation.Edit,
                        CurrentVersion = new FileVersion
                        {
                            Version = 2,
                            FileSize = 2048,
                            FileHash = "hash456",
                            CreatedAt = DateTime.Now.AddMinutes(-10)
                        }
                    }
                },
                new FileStateName
                {
                    Name = "file3.txt",
                    FileState = new FileState
                    {
                        Operation = FileOperation.Delete,
                        CurrentVersion = null // Deleted files won't have a current version
                    }
                }
            }
        };

        return View(trackedFiles);
    }
    public IActionResult FileHistory(string fileName)
    {
        // Simulated data for file history
        var history = new FileHistory
        {
            FileName = fileName,
            History = new List<FileState>
            {
                new FileState
                {
                    Operation = FileOperation.Create,
                    CurrentVersion = new FileVersion
                    {
                        Version = 1,
                        FileSize = 1024,
                        FileHash = "hash123",
                        CreatedAt = DateTime.Now.AddHours(-3)
                    }
                },
                new FileState
                {
                    Operation = FileOperation.Edit,
                    CurrentVersion = new FileVersion
                    {
                        Version = 2,
                        FileSize = 2048,
                        FileHash = "hash456",
                        CreatedAt = DateTime.Now.AddHours(-2)
                    }
                },
                new FileState
                {
                    Operation = FileOperation.Delete,
                    CurrentVersion = null,
                }
            }
        };

        return View(history);
    }

}
