using System.Text.Json;
using DataLayer.Entities;
using System.Threading;

namespace DataLayer;

public class Database : IDatabase
{
    private List<FileHistory> _fileHistories = new();
    private string _path = "database.json";
    private readonly SemaphoreSlim _fileSemaphore = new(1, 1);
    private readonly SemaphoreSlim _dbSemaphore = new(1, 1);

    public Database(string path)
    {
        if (!Path.Exists(path))
        {
            //create the file
            InitDatabase(path);
        }
        else
        {
            _path = path;
            if (!Load().Result)
            {
                throw new Exception("Failed to load database");
            }
        }
    }

    private async void InitDatabase(string path)
    {
        var emptyDb = new List<FileHistory>();
        var json = JsonSerializer.Serialize(emptyDb);

        await _fileSemaphore.WaitAsync();
        try
        {
            await File.WriteAllTextAsync(path, json);
            _path = path;
        }
        finally
        {
            _fileSemaphore.Release();
        }
    }

    public void AddFileState(string fileName, FileState fileState)
    {
        _dbSemaphore.Wait();
        try
        {
            var fileHistory = _fileHistories.FirstOrDefault(x => x.FileName == fileName);
            if (fileHistory == null)
            {
                if (fileState.Operation == FileOperation.Rename)
                {
                    //rename the history of the file
                    var oldFileName = fileState.FileName;
                    var oldFileHistory = _fileHistories.FirstOrDefault(x => x.FileName == oldFileName);
                    if (oldFileHistory == null)
                    {
                        Console.WriteLine("File not found in database");
                        return;
                    }
                    var index = _fileHistories.IndexOf(oldFileHistory);
                    _fileHistories[index].FileName = fileName;
                    _fileHistories[index].History.Add(fileState);
                }
                else
                {
                    _fileHistories.Add(new FileHistory
                    {
                        FileName = fileName,
                        History = [fileState]
                    });
                }
            }

            fileHistory?.History.Add(fileState);
        }
        finally
        {
            _dbSemaphore.Release();
        }
    }

    public bool RemoveFileState(string fileName, FileState fileVersion)
    {
        _dbSemaphore.Wait();
        try
        {
            var fileHistory = _fileHistories.FirstOrDefault(x => x.FileName == fileName);
            return fileHistory?.History.Remove(fileVersion) ?? false;
        }
        finally
        {
            _dbSemaphore.Release();
        }
    }

    public bool RemoveFileHistory(string fileName)
    {
        _dbSemaphore.Wait();
        try
        {
            var ret = _fileHistories.RemoveAll(x => x.FileName == fileName);
            if (ret == 0)
            {
                Console.WriteLine("File not found in database");
                return false;
            }
            if (ret > 1)
            {
                Console.WriteLine("Multiple files found in database");
                return false;
            }
            return true;
        }
        finally
        {
            _dbSemaphore.Release();
        }
    }

    public bool UpdateFileState(string fileName, FileState fileVersion)
    {
        _dbSemaphore.Wait();
        try
        {
            var fileHistory = _fileHistories.FirstOrDefault(x => x.FileName == fileName);
            if (fileHistory == null)
            {
                return false;
            }
            var index = fileHistory.History.FindIndex(x => x.CurrentVersion.Version == fileVersion.CurrentVersion.Version);
            if (index == -1)
            {
                return false;
            }
            fileHistory.History[index] = fileVersion;
            return true;
        }
        finally
        {
            _dbSemaphore.Release();
        }
    }

    public List<string> ListTrackedFiles()
    {
        _dbSemaphore.Wait();
        try
        {
            return _fileHistories.Select(x => x.FileName).ToList();
        }
        finally
        {
            _dbSemaphore.Release();
        }
    }

    public FileHistory? GetFileHistory(string fileName)
    {
        _dbSemaphore.Wait();
        try
        {
            return _fileHistories.FirstOrDefault(x => x.FileName == fileName);
        }
        finally
        {
            _dbSemaphore.Release();
        }
    }

    public FileState? GetLatestFileState(string fileName)
    {
        _dbSemaphore.Wait();
        try
        {
            return _fileHistories.LastOrDefault(x => x.FileName == fileName)?.History.Last();
        }
        finally
        {
            _dbSemaphore.Release();
        }
    }

    public async Task<bool> SaveChangesAsync()
    {
        var json = JsonSerializer.Serialize(_fileHistories);

        await _fileSemaphore.WaitAsync();
        try
        {
            await File.WriteAllTextAsync(_path, json);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
        finally
        {
            _fileSemaphore.Release();
        }

        return true;
    }

    public async Task<bool> Load()
    {
        if (!File.Exists(_path))
        {
            return false;
        }

        string json;
        await _fileSemaphore.WaitAsync();
        try
        {
            json = await File.ReadAllTextAsync(_path);
        }
        finally
        {
            _fileSemaphore.Release();
        }

        var db = JsonSerializer.Deserialize<List<FileHistory>>(json);
        if (db == null)
        {
            Console.WriteLine("Failed to load database");
            return false;
        }

        await _dbSemaphore.WaitAsync();
        try
        {
            _fileHistories = db;
        }
        finally
        {
            _dbSemaphore.Release();
        }

        return true;
    }

    public async Task<bool> RevertChanges()
    {
        return await Load();
    }
}