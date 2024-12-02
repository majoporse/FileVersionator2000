namespace DataLayer.Entities;

public interface IDatabase
{
    public void AddFileState(string fileName, FileState fileState);
    public bool RemoveFileState(string fileName, FileState fileVersion);
    public bool RemoveFileHistory(string fileName);
    public bool UpdateFileState(string fileName, FileState fileVersion);
    public List<string> ListTrackedFiles();
    public FileHistory? GetFileHistory(string fileName);
    public FileState? GetLatestFileState(string fileName);
    public Task<bool> SaveChangesAsync();
    public Task<bool> Load();
    public Task<bool> RevertChanges();
}