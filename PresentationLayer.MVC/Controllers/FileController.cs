using BusinessLayer.DTOs;
using DataLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using PresentationLayer.MVC.Models;

namespace PresentationLayer.MVC.Controllers;

public class FileController : Controller
{
    private List<FileStateName> _mockstates = new List<FileStateName>
        {
            new FileStateName
            {
                Name = "Document1.txt",
                FileState = new FileState
                {
                    Operation = FileOperation.Edit,
                    CurrentVersion = new FileVersion
                    {
                        Version = 2,
                        FileSize = 1024,
                        FileHash = "abc123",
                        CreatedAt = DateTime.Now.AddHours(-1)
                    }
                }
            },
            new FileStateName
            {
                Name = "Image1.png",
                FileState = new FileState
                {
                    Operation = FileOperation.Rename,
                    CurrentVersion = new FileVersion
                    {
                        Version = 3,
                        FileSize = 2048,
                        FileHash = "def456",
                        CreatedAt = DateTime.Now.AddHours(-3)
                    }
                }
            },
            new FileStateName
            {
                Name = "Document2.txt",
                FileState = new FileState
                {
                    Operation = FileOperation.Create,
                    CurrentVersion = new FileVersion
                    {
                        Version = 1,
                        FileSize = 512,
                        FileHash = "ghi789",
                        CreatedAt = DateTime.Now.AddHours(-5)
                    }
                }
            },
            new FileStateName
            {
                Name = "Image2.png",
                FileState = new FileState
                {
                    Operation = FileOperation.Delete,
                    CurrentVersion = new FileVersion
                    {
                        Version = 1,
                        FileSize = 512,
                        FileHash = "ghi789",
                        CreatedAt = DateTime.Now.AddHours(-5)
                    }
                }
            }
        };
    
    public IActionResult Files()
    {
        // Simulated tracked files
        var trackedFiles = new ListTrackedFilesModel
        {
            DirectoryPath = "C:\\Projects\\TrackedDirectory",
            FileStates = _mockstates

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
    
    public ActionResult LastChanges()
    {
        var lastChanges = _mockstates.Select(e => new LastFileOperationDto()
        {
            FileName = e.Name,
            Operation = e.FileState.Operation
        });

        return View(lastChanges);
    }
    
    [HttpPost]
    public ActionResult CreateSnapshot(string fileName)
    {
        // Simulated snapshot creation...
        TempData["Message"] = $"Snapshot created for {fileName}";
        return RedirectToAction("Files");
    }
    
    public ActionResult ManageFiles()
    {
        string directoryPath = @"C:\"; // Change to your target directory
        var files = Directory.EnumerateFiles(directoryPath)
            .Select(filePath => new FileIsTrackedDto
            {
                FileName = Path.GetFileName(filePath),
                FilePath = filePath
            })
            .ToList();

        return View(files);
    }
    
    
    [HttpPost]
    public ActionResult EnableTracking(IEnumerable<string> selectedFiles)
    {
        if (selectedFiles != null && selectedFiles.Any())
        {
            // _fileTrackingService.EnableTracking(selectedFiles);
            TempData["Message"] = $"{selectedFiles.Count()} file(s) are now being tracked.";
        }
        else
        {
            TempData["Message"] = "No files were selected for tracking.";
        }

        return RedirectToAction("LastChanges");
    }
}
