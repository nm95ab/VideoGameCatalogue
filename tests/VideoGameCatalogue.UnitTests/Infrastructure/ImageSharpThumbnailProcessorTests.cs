using System.Text;
using FluentAssertions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using VideoGameCatalogue.Infrastructure.Images;
using Xunit;

namespace VideoGameCatalogue.UnitTests.Infrastructure;

public class ImageSharpThumbnailProcessorTests
{
    private readonly ImageSharpThumbnailProcessor _processor = new();

    [Fact]
    public async Task ProcessThumbnailAsync_WhenStreamIsEmpty_ShouldReturnEmptyFailure()
    {
        // Arrange
        using var stream = new MemoryStream();

        // Act
        var result = await _processor.ProcessThumbnailAsync(stream, "empty.png");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Image.Empty");
    }

    [Fact]
    public async Task ProcessThumbnailAsync_WhenStreamIsNotAnImage_ShouldReturnInvalidFormatFailure()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("this is just plain text content not an image"));

        // Act
        var result = await _processor.ProcessThumbnailAsync(stream, "text.txt");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Image.InvalidFormat");
    }

    [Fact]
    public async Task ProcessThumbnailAsync_WhenValidLargeImage_ShouldResizeAndConvertToWebP()
    {
        // Arrange: create a 600x400 image in memory
        using var testImage = new Image<Rgba32>(600, 400);
        using var inputStream = new MemoryStream();
        await testImage.SaveAsPngAsync(inputStream);
        inputStream.Position = 0;

        // Act: max dimension 300
        var result = await _processor.ProcessThumbnailAsync(inputStream, "hero.png", maxDimension: 300);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var processed = result.Value;
        processed.ContentType.Should().Be("image/webp");
        processed.Extension.Should().Be(".webp");
        processed.Width.Should().BeLessThanOrEqualTo(300);
        processed.Height.Should().BeLessThanOrEqualTo(300);
        processed.Stream.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ProcessThumbnailAsync_WhenImageExceedsMaxDimensions_ShouldReturnTooLargeDimensionsFailure()
    {
        // Arrange: create an image with width 4097 and height 10 (exceeds 4096px limit)
        using var largeImage = new Image<Rgba32>(4097, 10);
        using var inputStream = new MemoryStream();
        await largeImage.SaveAsPngAsync(inputStream);
        inputStream.Position = 0;

        // Act
        var result = await _processor.ProcessThumbnailAsync(inputStream, "bomb.png");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Image.TooLargeDimensions");
    }
}
