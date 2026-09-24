using VideoGameCatalogue.Domain.Common;
using VideoGameCatalogue.Domain.Ports;

namespace VideoGameCatalogue.Application.Images;

/// <summary>
/// Driving Port / Application Service for managing game image thumbnails.
/// Coordinates validation, thumbnail processing, and blob/file persistence.
/// </summary>
public interface IImageManagementService
{
    /// <summary>
    /// Processes and stores a thumbnail image from the uploaded stream.
    /// </summary>
    Task<Result<ImageUploadResponse>> UploadThumbnailAsync(
        Stream inputStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the stored image file stream and metadata.
    /// </summary>
    Task<Result<ImageFileResult>> GetImageAsync(
        string imageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an image from storage.
    /// </summary>
    Task<Result> DeleteImageAsync(
        string imageId,
        CancellationToken cancellationToken = default);
}
