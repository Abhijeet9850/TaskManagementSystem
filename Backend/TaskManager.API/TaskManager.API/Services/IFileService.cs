namespace TaskManager.API.Services
{
    public interface IFileService
    {
        Task<(bool success, string message, string? filePath, string? originalFileName, long fileSize, string? contentType)> SaveFileAsync(IFormFile file);

        // Returns the full physical path for a stored relative path, or null if it doesn't exist.
        string? GetPhysicalPath(string relativePath);

        void DeletePhysicalFile(string relativePath);

    }
}
