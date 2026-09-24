namespace VideoGameCatalogue.Domain.Ports;

/// <summary>
/// Result container for retrieved image streams from storage adapters.
/// </summary>
public readonly record struct ImageFileResult(Stream Stream, string ContentType, string FileName);

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
    /// Retrieves an image file stream and its content type by identifier.
    /// </summary>
    Task<ImageFileResult?> GetImageAsync(string imageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an image from storage by its identifier.
    /// </summary>
    Task<bool> DeleteImageAsync(string imageId, CancellationToken cancellationToken = default);
}
