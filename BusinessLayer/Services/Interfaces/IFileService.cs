using BusinessLayer.DTOs;
using DataLayer.Entities;

namespace BusinessLayer.Services;

public interface IFileService
{
    public List<string> ListTrackedFiles();
    public List<FileStateName> ListTrackedFileStates();
    public Task<bool> TrackFileAsync(string fileName);
    public Task<bool> RemoveFileHistoryAsync(string fileName);
    public Task<bool> CreateSnapshot();
    public Task<List<LastFileOperationDto>> GetLastFileOperations();
    public List<FileState>? GetFileStates(string fileName);
    public FileState? GetFileState(string fileName, string version);
}