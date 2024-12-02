using System.Text.Json;
using DataLayer.Entities;

namespace DataLayer;

public class Database : IDatabase
{
    private List<FileHistory> _fileHistories = new();
    private string _path = "database.json";
    
    Database(string path)
    {
        if (!Path.Exists(path))
        {
            //create the file
            InitDatabase();
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

    private async void InitDatabase()
    {
        var emptyDb = new List<FileHistory>();
        var json = JsonSerializer.Serialize(emptyDb);
        await File.WriteAllTextAsync(_path, json);
        
    }

    public bool AddFileState(string fileName, FileState fileState)
    {
        var fileHistory = _fileHistories.FirstOrDefault(x => x.FileName == fileName);
        fileHistory?.History.Add(fileState);
        return fileHistory != null;
    }

    public bool RemoveFileState(string fileName, FileState fileVersion)
    {
        var fileHistory = _fileHistories.FirstOrDefault(x => x.FileName == fileName);
        return fileHistory?.History.Remove(fileVersion) ?? false;
    }

    public bool RemoveFileHistory(string fileName)
    {
        var ret =  _fileHistories.RemoveAll(x => x.FileName == fileName);
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

    public bool UpdateFileState(string fileName, FileState fileVersion)
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

    public List<string> ListTrackedFiles()
    {
        return _fileHistories.Select(x => x.FileName).ToList();
    }

    public FileHistory? GetFileHistory(string fileName)
    {
        return _fileHistories.FirstOrDefault(x => x.FileName == fileName);
    }

    public FileState? GetLatestFileState(string fileName)
    {
        return _fileHistories.FirstOrDefault(x => x.FileName == fileName)?.History.First();
    }

    public async Task<bool> SaveChangesAsync()
    {
        var json = JsonSerializer.Serialize(_fileHistories);
        try
        {
            await File.WriteAllTextAsync(_path, json);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }

        return true;
    }

    public async Task<bool> Load()
    {
        if (!File.Exists(_path))
        {
            return false;
        }

        var json = await File.ReadAllTextAsync(_path);
        var db = JsonSerializer.Deserialize<List<FileHistory>>(json);
        if (db == null)
        {
            Console.WriteLine("Failed to load database");
            return false;
        }
        _fileHistories = db;
        return true;
    }

    public async Task<bool> RevertChanges()
    {
        return await Load();
    }
}