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

    public AzureBlobStorageImageStorageAdapter(BlobServiceClient blobServiceClient, string containerName = "game-thumbnails")
    {
        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        _containerClient.CreateIfNotExists(PublicAccessType.None);
    }

    public AzureBlobStorageImageStorageAdapter(string connectionString, string containerName = "game-thumbnails")
        : this(new BlobServiceClient(connectionString), containerName)
    {
    }

    public async Task<string> SaveImageAsync(
        Stream content,
        string contentType,
        string extension,
        CancellationToken cancellationToken = default)
    {
        var cleanExtension = extension.StartsWith('.') ? extension : "." + extension;
        var imageId = $"{Guid.NewGuid():N}{cleanExtension}";
        var blobClient = _containerClient.GetBlobClient(imageId);

        if (content.CanSeek)
            content.Position = 0;

        var headers = new BlobHttpHeaders { ContentType = contentType };
        await blobClient.UploadAsync(content, headers, cancellationToken: cancellationToken);

        return imageId;
    }

    public async Task<ImageFileResult?> GetImageAsync(string imageId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(imageId))
            return null;

        var blobClient = _containerClient.GetBlobClient(imageId);
        if (!await blobClient.ExistsAsync(cancellationToken))
            return null;

        var downloadResult = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
        var contentType = downloadResult.Value.Details.ContentType ?? "application/octet-stream";

        return new ImageFileResult(downloadResult.Value.Content, contentType, imageId);
    }

    public async Task<bool> DeleteImageAsync(string imageId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(imageId))
            return false;

        var blobClient = _containerClient.GetBlobClient(imageId);
        var response = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        return response.Value;
    }
}
