namespace VideoGameCatalogue.Domain.Ports;

/// <summary>
/// Outbound (Driven) Port for abstracting image blob/file persistence.
/// Implementations reside in Infrastructure (e.g. Local Storage, Azure Blob Storage).
/// </summary>
public interface IImageStoragePort
{
    /// <summary>
    /// Persists an image stream and returns an immutable storage identifier.
    /// </summary>
    Task<string> SaveImageAsync(Stream content, string contentType, string extension, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an image from storage by its identifier.
    /// </summary>
    Task<bool> DeleteImageAsync(string imageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves the public, CDN, or relative URL for accessing the image by its identifier.
    /// </summary>
    string GetImageUrl(string imageId);
}
