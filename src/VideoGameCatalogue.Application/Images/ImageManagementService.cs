using VideoGameCatalogue.Application.Common.Ports;
using VideoGameCatalogue.Domain.Common;
using VideoGameCatalogue.Domain.Ports;

namespace VideoGameCatalogue.Application.Images;

public class ImageManagementService(
    IImageThumbnailProcessor processor,
    IImageStoragePort storagePort) : IImageManagementService
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public async Task<Result<ImageUploadResponse>> UploadThumbnailAsync(
        Stream inputStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (inputStream is null || inputStream.Length == 0)
            return Result<ImageUploadResponse>.Failure(Error.Validation("Image.Empty", "The image file cannot be empty."));

        if (inputStream.Length > MaxFileSizeBytes)
            return Result<ImageUploadResponse>.Failure(Error.Validation("Image.TooLarge", "The image exceeds the maximum allowed size of 10MB."));

        var processResult = await processor.ProcessThumbnailAsync(inputStream, fileName, 300, cancellationToken);
        if (processResult.IsFailure)
            return Result<ImageUploadResponse>.Failure(processResult.Error);

        var processed = processResult.Value;
        using (processed.Stream)
        {
            var imageId = await storagePort.SaveImageAsync(
                processed.Stream,
                processed.ContentType,
                processed.Extension,
                cancellationToken);

            var url = $"/api/images/{imageId}";
            return Result<ImageUploadResponse>.Success(new ImageUploadResponse(imageId, url));
        }
    }

    public async Task<Result> DeleteImageAsync(
        string imageId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(imageId))
            return Result.Failure(Error.Validation("Image.InvalidId", "The image ID cannot be empty."));

        var deleted = await storagePort.DeleteImageAsync(imageId, cancellationToken);
        if (!deleted)
            return Result.Failure(Error.NotFound("Image.NotFound", $"Image '{imageId}' was not found."));

        return Result.Success();
    }
}
