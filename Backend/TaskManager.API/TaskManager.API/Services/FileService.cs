namespace TaskManager.API.Services
{
    public class FileService : IFileService
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        private static readonly Dictionary<string, string> ContentTypeMap = new()
        {
            [".pdf"] = "application/pdf",
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png"
        };

        public FileService(IConfiguration config, IWebHostEnvironment env)
        {
            _config = config;
            _env = env;
        }

        public async Task<(bool success, string message, string? filePath, string? originalFileName, long fileSize, string? contentType)> SaveFileAsync(IFormFile file)
        {
            var maxSizeMb = _config.GetValue<int>("FileUpload:MaxSizeMb");
            var allowedExtensions = _config.GetSection("FileUpload:AllowedExtensions").Get<string[]>() ?? Array.Empty<string>();
            var storageFolder = _config.GetValue<string>("FileUpload:StoragePath") ?? "Uploads";

            if (file == null || file.Length == 0)
                return (false, "No file was provided.", null, null, 0, null);

            if (file.Length > maxSizeMb * 1024 * 1024)
                return (false, $"File exceeds the maximum allowed size of {maxSizeMb} MB.", null, null, 0, null);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return (false, "Only PDF, JPG, and PNG files are allowed.", null, null, 0, null);

            var uploadsPath = Path.Combine(_env.ContentRootPath, storageFolder);
            Directory.CreateDirectory(uploadsPath);

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(uploadsPath, uniqueFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = Path.Combine(storageFolder, uniqueFileName).Replace("\\", "/");
            var contentType = ContentTypeMap.GetValueOrDefault(extension, "application/octet-stream");

            return (true, "File uploaded successfully.", relativePath, file.FileName, file.Length, contentType);
        }

        public string? GetPhysicalPath(string relativePath)
        {
            var fullPath = Path.Combine(_env.ContentRootPath, relativePath);
            return File.Exists(fullPath) ? fullPath : null;
        }

        public void DeletePhysicalFile(string relativePath)
        {
            var fullPath = Path.Combine(_env.ContentRootPath, relativePath);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
    }
}
