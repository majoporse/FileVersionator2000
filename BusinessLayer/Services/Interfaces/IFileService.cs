using BusinessLayer.DTOs;
using DataLayer.Entities;

namespace BusinessLayer.Services;

public interface IFileService
{
    public List<string> ListTrackedFiles();
    public List<FileStateName> ListTrackedFileStates();
    public Task<bool> TrackFileAsync(string fileName);
    public Task<bool> RemoveFileHistoryAsync(string fileName);
    public Task<bool> CreateSnapshotAsync();
    public Task<List<LastFileOperationDto>> GetLastFileOperationsAsync();
    public FileHistory? GetFileHistory(string fileName);
    public FileState? GetFileState(string fileName, string version);
    public List<FileIsTrackedDto> GetTrackedFilesWithUntracked();
    public string GetTrackingDirectory();
    public Task<bool> EnableTrackingAsync(IEnumerable<string> selectedFiles);
}