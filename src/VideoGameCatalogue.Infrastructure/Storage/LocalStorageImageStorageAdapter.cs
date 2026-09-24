using VideoGameCatalogue.Domain.Ports;

namespace VideoGameCatalogue.Infrastructure.Storage;

/// <summary>
/// Infrastructure adapter storing image files on the local filesystem.
/// </summary>
public class LocalStorageImageStorageAdapter : IImageStoragePort
{
    private readonly string _storageDirectory;
    private readonly string? _baseUrl;

    public LocalStorageImageStorageAdapter(string? storageDirectory = null, string? baseUrl = null)
    {
        _storageDirectory = string.IsNullOrWhiteSpace(storageDirectory)
            ? Path.Combine(Directory.GetCurrentDirectory(), "uploads", "images")
            : storageDirectory;

        _baseUrl = baseUrl;

        if (!Directory.Exists(_storageDirectory))
        {
            Directory.CreateDirectory(_storageDirectory);
        }
    }

    public async Task<string> SaveImageAsync(
        Stream content,
        string contentType,
        string extension,
        CancellationToken cancellationToken = default)
    {
        var cleanExtension = extension.StartsWith('.') ? extension : "." + extension;
        var imageId = $"{Guid.NewGuid():N}{cleanExtension}";
        var filePath = GetSafeFilePath(imageId);

        await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
        if (content.CanSeek)
            content.Position = 0;

        await content.CopyToAsync(fileStream, cancellationToken);
        return imageId;
    }

    public Task<bool> DeleteImageAsync(string imageId, CancellationToken cancellationToken = default)
    {
        if (IsUnsafePath(imageId))
            return Task.FromResult(false);

        var filePath = GetSafeFilePath(imageId);
        if (!File.Exists(filePath))
            return Task.FromResult(false);

        File.Delete(filePath);
        return Task.FromResult(true);
    }

    public string GetImageUrl(string imageId)
    {
        if (IsUnsafePath(imageId))
            return string.Empty;

        var sanitized = Path.GetFileName(imageId);
        if (!string.IsNullOrWhiteSpace(_baseUrl))
        {
            return $"{_baseUrl.TrimEnd('/')}/{sanitized}";
        }

        return $"/api/images/{sanitized}";
    }

    private string GetSafeFilePath(string imageId)
    {
        var sanitized = Path.GetFileName(imageId);
        return Path.Combine(_storageDirectory, sanitized);
    }

    private static bool IsUnsafePath(string imageId)
    {
        return string.IsNullOrWhiteSpace(imageId) ||
               imageId.Contains('/') ||
               imageId.Contains('\\') ||
               imageId.Contains("..");
    }
}
