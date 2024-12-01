namespace DataLayer.Entities;

public interface IDatabase
{
    public bool AddFile(string fileName, FileState fileState);
    public bool RemoveFileState(string fileName, FileState fileVersion);
    public bool UpdateFileState(string fileName, FileState fileVersion);
    public FileHistory? GetFileHistory(string fileName);
    public FileState? GetLatestFileState(string fileName);
    public void SaveChanges();
    public void Load();
}