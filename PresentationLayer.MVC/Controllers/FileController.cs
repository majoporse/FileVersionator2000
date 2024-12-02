using BusinessLayer.Services;
using Microsoft.AspNetCore.Mvc;
using PresentationLayer.MVC.Models;

namespace PresentationLayer.MVC.Controllers;

public class FileController(IFileService _fileService) : Controller
{
    public IActionResult Files()
    {
        var trackedFiles = new ListTrackedFilesModel
        {
            DirectoryPath = _fileService.GetTrackingDirectory(),
            FileStates = _fileService.ListTrackedFileStates()
        };

        return View(trackedFiles);
    }

    public IActionResult FileHistory(string fileName)
    {
        var history = _fileService.GetFileHistory(fileName);
        if (history == null)
        {
            return NotFound();
        }
        
        return View(history);
    }
    
    public async Task<ActionResult> LastChanges()
    {
        var lastChanges = await _fileService.GetLastFileOperationsAsync();
        return View(lastChanges);
    }
    
    [HttpPost]
    public async Task<ActionResult> CreateSnapshot(string fileName)
    {
        // Simulated snapshot creation...
        if (!await _fileService.CreateSnapshotAsync())
        {
            TempData["Message"] = "Snapshot creation failed.";
        }
        else
        {
            TempData["Message"] = $"Snapshot created for {fileName}";
        }
        
        return RedirectToAction("Files");
    }
    
    public ActionResult ManageFiles()
    {
        var files = _fileService.GetTrackedFilesWithUntracked();
        return View(files);
    }
    
    
    [HttpPost]
    public async Task<ActionResult> EnableTracking(IEnumerable<string> selectedFiles)
    {
        var enumerable = selectedFiles.ToList();
        if (await _fileService.EnableTrackingAsync(enumerable))
        {
            // _fileTrackingService.EnableTracking(selectedFiles);
            TempData["Message"] = $"{enumerable.Count} file(s) are now being tracked.";
        }
        else
        {
            TempData["Message"] = "No files were selected for tracking.";
        }

        return RedirectToAction("LastChanges");
    }
}
