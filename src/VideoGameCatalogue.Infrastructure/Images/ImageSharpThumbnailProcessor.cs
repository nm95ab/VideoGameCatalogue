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
    public async Task<Result<ProcessedImageResult>> ProcessThumbnailAsync(
        Stream inputStream,
        string originalFileName,
        int maxDimension = 300,
        CancellationToken cancellationToken = default)
    {
        if (inputStream is null || inputStream.Length == 0)
            return Result<ProcessedImageResult>.Failure(Error.Validation("Image.Empty", "The image file stream is empty."));

        if (inputStream.CanSeek)
            inputStream.Position = 0;

        try
        {
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
}
