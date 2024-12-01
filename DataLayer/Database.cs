using System.Text.Json;
using DataLayer.Entities;

namespace DataLayer;

public class Database : IDatabase
{
    private List<FileHistory> _fileHistories = new();
    private string _path = "database.json";
    
    Database(string path)
    {
        _path = path;
        Load();
    }

    public bool AddFile(string fileName, FileState fileState)
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

    public FileHistory? GetFileHistory(string fileName)
    {
        return _fileHistories.FirstOrDefault(x => x.FileName == fileName);
    }

    public FileState? GetLatestFileState(string fileName)
    {
        return _fileHistories.FirstOrDefault(x => x.FileName == fileName)?.History.First();
    }

    public void SaveChanges()
    {
        var json = JsonSerializer.Serialize(_fileHistories);
        File.WriteAllText(_path, json);
    }

    public void Load()
    {
        if (!File.Exists(_path))
        {
            return;
        }

        var json = File.ReadAllText(_path);
        _fileHistories = JsonSerializer.Deserialize<List<FileHistory>>(json);
    }
}