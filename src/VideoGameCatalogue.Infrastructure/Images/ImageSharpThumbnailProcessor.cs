using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using VideoGameCatalogue.Application.Common.Ports;
using VideoGameCatalogue.Domain.Common;

namespace VideoGameCatalogue.Infrastructure.Images;

/// <summary>
/// Infrastructure adapter using SixLabors.ImageSharp to validate, resize, and convert uploaded images into WebP thumbnails.
/// </summary>
public class ImageSharpThumbnailProcessor : IImageThumbnailProcessor
{
    private const int MaxAllowedDimension = 4096;

    public async Task<Result<ProcessedImageResult>> ProcessThumbnailAsync(
        Stream inputStream,
        string originalFileName,
        int maxDimension = 300,
        CancellationToken cancellationToken = default)
    {
        if (inputStream is null || inputStream.Length == 0)
            return Result<ProcessedImageResult>.Failure(Error.Validation("Image.Empty", "The image file stream is empty."));

        RewindIfSeekable(inputStream);

        try
        {
            // Inspect header metadata only (O(1) memory) to prevent decompression bombs / pixel flood DoS
            var imageInfo = await Image.IdentifyAsync(inputStream, cancellationToken);
            var validationError = ValidateImageDimensions(imageInfo);
            if (validationError is not null)
            {
                return Result<ProcessedImageResult>.Failure(validationError.Value);
            }

            RewindIfSeekable(inputStream);

            using var image = await Image.LoadAsync(inputStream, cancellationToken);

            var resizeOptions = new ResizeOptions
            {
                Size = new Size(maxDimension, maxDimension),
                Mode = ResizeMode.Max
            };

            image.Mutate(x => x.Resize(resizeOptions));

            // Strip metadata profiles for privacy and bandwidth reduction
            image.Metadata.ExifProfile = null;
            image.Metadata.IptcProfile = null;
            image.Metadata.XmpProfile = null;

            var outputStream = new MemoryStream();
            var encoder = new WebpEncoder
            {
                Quality = 80,
                FileFormat = WebpFileFormatType.Lossy
            };

            await image.SaveAsWebpAsync(outputStream, encoder, cancellationToken);
            outputStream.Position = 0;

            return Result<ProcessedImageResult>.Success(new ProcessedImageResult(
                outputStream,
                "image/webp",
                ".webp",
                image.Width,
                image.Height));
        }
        catch (UnknownImageFormatException)
        {
            return Result<ProcessedImageResult>.Failure(Error.Validation("Image.InvalidFormat", "The uploaded file is not a supported image format (JPEG, PNG, WebP)."));
        }
        catch (InvalidImageContentException)
        {
            return Result<ProcessedImageResult>.Failure(Error.Validation("Image.Corrupted", "The uploaded image file is corrupted."));
        }
    }

    private static Error? ValidateImageDimensions(ImageInfo? imageInfo)
    {
        if (imageInfo is null)
        {
            return Error.Validation("Image.InvalidFormat", "Unable to read image metadata.");
        }

        if (imageInfo.Width > MaxAllowedDimension || imageInfo.Height > MaxAllowedDimension)
        {
            return Error.Validation(
                "Image.TooLargeDimensions",
                $"Image dimensions ({imageInfo.Width}x{imageInfo.Height}) exceed the maximum allowable limit of {MaxAllowedDimension}x{MaxAllowedDimension} pixels.");
        }

        return null;
    }

    private static void RewindIfSeekable(Stream stream)
    {
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }
    }
}
