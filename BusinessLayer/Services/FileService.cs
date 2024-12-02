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

        _db.AddFileState(fileName, fileState);
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
        foreach (var filename in Directory.GetFiles(_trackedDirectory, "*", SearchOption.AllDirectories)
                     .Except(ListTrackedFiles()).ToList())
        {
            fileHashes.TryAdd(await HashUtils.HashMd5(filename), filename);
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
            
            var lastHash = lastState.CurrentVersion.FileHash;
            
            var operation = FileOperation.Delete;
            var hash = lastHash;
            var newFileName = fileName;
            
            var fileNameFromPrevHash = fileHashes.GetValueOrDefault(lastHash);
            var fileContentUnchanged = fileNameFromPrevHash != null;
            
            if (File.Exists(fileName))
            {
                hash = await HashUtils.HashMd5(fileName);
                if (hash == lastHash)
                    continue;

                if (lastHash != hash)
                    operation = FileOperation.Edit;

            }
            else
            {
                if (fileContentUnchanged)
                {
                    operation = FileOperation.Rename;
                    newFileName = fileNameFromPrevHash;
                }
            }
            
            var fileState = new FileState
            {
                CurrentVersion = new FileVersion
                {
                    Version = lastState.CurrentVersion.Version + 1,
                    FileSize = operation == FileOperation.Delete ? 0 : new FileInfo(newFileName).Length,
                    FileHash = hash,
                    CreatedAt = DateTime.Now
                },
                Operation = operation,
                FileName = fileName
            };
            res.Add((newFileName, fileState));
        }
        
        return res;
    }
    
    public async Task<bool> CreateSnapshotAsync()
    {
        (await GetNewFileStates()).ForEach(x => _db.AddFileState(x.Item1, x.Item2));
        return await _db.SaveChangesAsync();
    }

    public async Task<List<LastFileOperationDto>> GetLastFileOperationsAsync()
    {
        return (await GetNewFileStates()).Select(e => new LastFileOperationDto
        {
            FileName = e.Item1,
            Operation = e.Item2.Operation
        }).ToList();
    }

    public FileHistory? GetFileHistory(string fileName)
    {
        return _db.GetFileHistory(fileName);
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

    public string GetTrackingDirectory()
    {
        return _trackedDirectory;
    }

    public async Task<bool> EnableTrackingAsync(IEnumerable<string> selectedFiles)
    {   
        foreach (var file in selectedFiles)
        {
            if (!File.Exists(file))
            {
                Console.WriteLine($"File {file} does not exist");
                continue;
            }
            if (!ListTrackedFiles().Contains(file))
            {
                await TrackFileAsync(file);
            }
            else
            {
                Console.WriteLine($"File {file} is already tracked");
            }
        }
        return true;
    }
}