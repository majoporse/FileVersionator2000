using BusinessLayer.DTOs;
using DataLayer.Entities;
using Utils;

namespace BusinessLayer.Services;

public class FileService: IFileService
{
    private readonly IDatabase _db;
    private readonly string _trackedDirectory;
    
    public FileService(IDatabase db, string trackedDirectory)
    {
        if (!Directory.Exists(trackedDirectory))
        {
            Directory.CreateDirectory(trackedDirectory);
        }
        _db = db;
        _trackedDirectory = trackedDirectory;
    }

    public List<FileStateName> ListTrackedFileStates()
    {
        return _db.ListTrackedFiles().Select(e => new FileStateName()
        {
            Name = e,
            FileState = _db.GetLatestFileState(e)!
        }).ToList();
    }
    
    public List<string> ListTrackedFiles()
    {
        return _db.ListTrackedFiles();
    }
    

    public async Task<bool> TrackFileAsync(string fileName)
    {
        var hash = await HashUtils.HashMd5(fileName);
        var fileSize = new FileInfo(fileName).Length;
        var fileState = new FileState
        {
            CurrentVersion = new FileVersion
            {
                Version = 1,
                FileSize = fileSize,
                FileHash = hash,
                CreatedAt = DateTime.Now
            },
            Operation = FileOperation.Create
        };
        
        if (!_db.AddFileState(fileName, fileState))
        {
            Console.WriteLine("Failed to add file to database");
            return false;
        }
        return await _db.SaveChangesAsync();
    }

    public async Task<bool> RemoveFileHistoryAsync(string fileName)
    {        
        if (!_db.RemoveFileHistory(fileName))
        {
            Console.WriteLine("Failed to add file to database");
            return false;
        }
        
        return await _db.SaveChangesAsync();
    }

    private async Task<List<(string, FileState)>> GetNewFileStates()
    {
        var fileHashes = new Dictionary<string, string>();
        foreach (var filename in Directory.GetFiles(_trackedDirectory, "*", SearchOption.AllDirectories).ToList())
        {
            fileHashes.Add(await HashUtils.HashMd5(filename), filename);
        }

        var res = new List<(string, FileState)>();
        
        foreach (var fileName in ListTrackedFiles())
        {
            var lastState = _db.GetLatestFileState(fileName);
            if (lastState == null)
            {
                Console.WriteLine($"File {fileName} not found in database"); //shouldn't happen
                continue;
            }
            
            if (lastState.Operation == FileOperation.Delete)
                continue;
            
            var hash = await HashUtils.HashMd5(fileName);
            var fileNameFromPrevHash = fileHashes.GetValueOrDefault(lastState.CurrentVersion.FileHash);

            var operation = FileOperation.Delete;
            if (fileNameFromPrevHash == fileName && lastState.CurrentVersion.FileHash == hash)
            {
                continue;
            }
            if (fileNameFromPrevHash == fileName && lastState.CurrentVersion.FileHash != hash)
            {
                operation = FileOperation.Edit;
            }
            if (fileNameFromPrevHash != fileName && fileNameFromPrevHash != null)
            {
                operation = FileOperation.Rename;
            }

            
            var fileState = new FileState
            {
                CurrentVersion = new FileVersion
                {
                    Version = lastState.CurrentVersion.Version + 1,
                    FileSize = new FileInfo(fileName).Length,
                    FileHash = hash,
                    CreatedAt = DateTime.Now
                },
                Operation = operation
            };
            res.Add((fileName, fileState));
        }
        
        return res;
    }
    
    public async Task<bool> CreateSnapshot()
    {
        (await GetNewFileStates()).ForEach(x => _db.AddFileState(x.Item1, x.Item2));
        return await _db.SaveChangesAsync();
    }

    public async Task<List<LastFileOperationDto>> GetLastFileOperations()
    {
        return (await GetNewFileStates()).Select(e => new LastFileOperationDto
        {
            FileName = e.Item1,
            Operation = e.Item2.Operation
        }).ToList();
    }

    public List<FileState>? GetFileStates(string fileName)
    {
        return _db.GetFileHistory(fileName)?.History;
    }

    public FileState? GetFileState(string fileName, string version)
    {
        return _db.GetFileHistory(fileName)?.History.FirstOrDefault(x => x.CurrentVersion.Version.ToString() == version);
    }

    public List<FileIsTrackedDto> GetTrackedFilesWithUntracked()
    {
        var trackedFiles = ListTrackedFiles();
        var allFiles = Directory.GetFiles(_trackedDirectory, "*", SearchOption.AllDirectories).ToList();
        
        var untrackedFiles = allFiles.Except(trackedFiles).ToList();
        return trackedFiles.Select(e => new FileIsTrackedDto
        {
            FileName = e,
            FilePath = e,
            IsSelected = true
        }).Concat(untrackedFiles.Select(e => new FileIsTrackedDto
        {
            FileName = e,
            FilePath = e,
            IsSelected = false
        })).OrderBy(e1 => e1.FileName).ToList();
    }
}