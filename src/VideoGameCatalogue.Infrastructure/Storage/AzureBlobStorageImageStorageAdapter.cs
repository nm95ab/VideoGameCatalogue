using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using VideoGameCatalogue.Domain.Ports;

namespace VideoGameCatalogue.Infrastructure.Storage;

/// <summary>
/// Infrastructure adapter implementing image persistence against Azure Blob Storage.
/// Ready for production activation via configuration.
/// </summary>
public class AzureBlobStorageImageStorageAdapter : IImageStoragePort
{
    private readonly BlobContainerClient _containerClient;
    private readonly string? _baseUrl;

    public AzureBlobStorageImageStorageAdapter(
        BlobServiceClient blobServiceClient,
        string containerName = "game-thumbnails",
        string? baseUrl = null)
    {
        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        _baseUrl = baseUrl;
    }

    public AzureBlobStorageImageStorageAdapter(
        string connectionString,
        string containerName = "game-thumbnails",
        string? baseUrl = null)
        : this(new BlobServiceClient(connectionString), containerName, baseUrl)
    {
    }

    public async Task<string> SaveImageAsync(
        Stream content,
        string contentType,
        string extension,
        CancellationToken cancellationToken = default)
    {
        await _containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken).ConfigureAwait(false);

        var cleanExtension = extension.StartsWith('.') ? extension : "." + extension;
        var imageId = $"{Guid.NewGuid():N}{cleanExtension}";
        var blobClient = _containerClient.GetBlobClient(imageId);

        if (content.CanSeek)
            content.Position = 0;

        var headers = new BlobHttpHeaders { ContentType = contentType };
        await blobClient.UploadAsync(content, headers, cancellationToken: cancellationToken);

        return imageId;
    }

    public async Task<bool> DeleteImageAsync(string imageId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(imageId))
            return false;

        var blobClient = _containerClient.GetBlobClient(imageId);
        var response = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        return response.Value;
    }

    public string GetImageUrl(string imageId)
    {
        if (string.IsNullOrWhiteSpace(imageId))
            return string.Empty;

        var sanitized = Path.GetFileName(imageId);
        if (!string.IsNullOrWhiteSpace(_baseUrl))
        {
            return $"{_baseUrl.TrimEnd('/')}/{sanitized}";
        }

        return $"{_containerClient.Uri.ToString().TrimEnd('/')}/{sanitized}";
    }
}
