namespace wnc.Features.Students.Applications;

internal sealed class StudentApplicationDocumentStorage(IWebHostEnvironment webHostEnvironment)
{
    private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;

    public async Task<string> StoreAsync(Guid applicationId, Guid documentTypeId, IFormFile file, CancellationToken cancellationToken)
    {
        var uploadsDirectory = Path.Combine(
            GetWebRootPath(),
            "uploads",
            applicationId.ToString(),
            documentTypeId.ToString());

        Directory.CreateDirectory(uploadsDirectory);

        var storedFileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}.pdf";
        var physicalPath = Path.Combine(uploadsDirectory, storedFileName);

        await using (var fileStream = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await file.CopyToAsync(fileStream, cancellationToken);
        }

        return $"uploads/{applicationId}/{documentTypeId}/{storedFileName}";
    }

    public Task DeleteAsync(string? storagePath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(storagePath))
        {
            return Task.CompletedTask;
        }

        var normalizedPath = storagePath.TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar);
        var webRootPath = GetWebRootPath();
        var fullWebRootPath = Path.GetFullPath(webRootPath);
        var fullStoragePath = Path.GetFullPath(Path.Combine(webRootPath, normalizedPath));

        if (!fullStoragePath.StartsWith(fullWebRootPath, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        if (File.Exists(fullStoragePath))
        {
            File.Delete(fullStoragePath);
        }

        return Task.CompletedTask;
    }

    private string GetWebRootPath()
    {
        return string.IsNullOrWhiteSpace(_webHostEnvironment.WebRootPath)
            ? Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot")
            : _webHostEnvironment.WebRootPath;
    }
}
