using VideoGameCatalogue.Domain.Common;

namespace VideoGameCatalogue.Application.Common.Ports;

/// <summary>
/// Result container for processed thumbnail images.
/// </summary>
public readonly record struct ProcessedImageResult(Stream Stream, string ContentType, string Extension, int Width, int Height);

/// <summary>
/// Outbound Port for validating, downscaling, and encoding raw image uploads into standardized thumbnails.
/// </summary>
public interface IImageThumbnailProcessor
{
    /// <summary>
    /// Processes an input image stream, validating its content and resizing to a maximum dimension while converting to WebP.
    /// </summary>
    Task<Result<ProcessedImageResult>> ProcessThumbnailAsync(
        Stream inputStream,
        string originalFileName,
        int maxDimension = 300,
        CancellationToken cancellationToken = default);
}
