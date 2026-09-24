using System.Text;
using FluentAssertions;
using NSubstitute;
using VideoGameCatalogue.Application.Common.Ports;
using VideoGameCatalogue.Application.Images;
using VideoGameCatalogue.Domain.Common;
using VideoGameCatalogue.Domain.Ports;
using Xunit;

namespace VideoGameCatalogue.UnitTests.Application;

public class ImageManagementServiceTests
{
    private readonly IImageThumbnailProcessor _processor = Substitute.For<IImageThumbnailProcessor>();
    private readonly IImageStoragePort _storage = Substitute.For<IImageStoragePort>();
    private readonly ImageManagementService _service;

    public ImageManagementServiceTests()
    {
        _service = new ImageManagementService(_processor, _storage);
    }

    [Fact]
    public async Task UploadThumbnailAsync_WhenStreamIsNull_ShouldReturnEmptyFailure()
    {
        // Act
        var result = await _service.UploadThumbnailAsync(null!, "test.jpg", "image/jpeg");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Image.Empty");
    }

    [Fact]
    public async Task UploadThumbnailAsync_WhenStreamIsEmpty_ShouldReturnEmptyFailure()
    {
        // Arrange
        using var stream = new MemoryStream();

        // Act
        var result = await _service.UploadThumbnailAsync(stream, "test.jpg", "image/jpeg");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Image.Empty");
    }

    [Fact]
    public async Task UploadThumbnailAsync_WhenStreamExceeds10MB_ShouldReturnTooLargeFailure()
    {
        // Arrange (create a mock or seekable stream of 11MB)
        var largeStream = Substitute.For<Stream>();
        largeStream.Length.Returns(11 * 1024 * 1024);

        // Act
        var result = await _service.UploadThumbnailAsync(largeStream, "large.jpg", "image/jpeg");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Image.TooLarge");
    }

    [Fact]
    public async Task UploadThumbnailAsync_WhenProcessorFails_ShouldReturnProcessorError()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("not-an-image"));
        _processor.ProcessThumbnailAsync(stream, "bad.txt", 300, Arg.Any<CancellationToken>())
            .Returns(Result<ProcessedImageResult>.Failure(Error.Validation("Image.InvalidFormat", "Unsupported file format.")));

        // Act
        var result = await _service.UploadThumbnailAsync(stream, "bad.txt", "text/plain");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Image.InvalidFormat");
    }

    [Fact]
    public async Task UploadThumbnailAsync_WhenValid_ShouldSaveAndReturnResponse()
    {
        // Arrange
        using var rawStream = new MemoryStream(Encoding.UTF8.GetBytes("image-content"));
        var processedStream = new MemoryStream(Encoding.UTF8.GetBytes("processed-webp"));
        var processedResult = new ProcessedImageResult(processedStream, "image/webp", ".webp", 300, 300);

        _processor.ProcessThumbnailAsync(rawStream, "photo.png", 300, Arg.Any<CancellationToken>())
            .Returns(Result<ProcessedImageResult>.Success(processedResult));

        const string generatedId = "game-photo-123.webp";
        _storage.SaveImageAsync(processedStream, "image/webp", ".webp", Arg.Any<CancellationToken>())
            .Returns(generatedId);

        // Act
        var result = await _service.UploadThumbnailAsync(rawStream, "photo.png", "image/png");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ImageId.Should().Be(generatedId);
        result.Value.Url.Should().Be($"/api/images/{generatedId}");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task DeleteImageAsync_WhenIdIsInvalid_ShouldReturnInvalidIdFailure(string invalidId)
    {
        // Act
        var result = await _service.DeleteImageAsync(invalidId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Image.InvalidId");
    }

    [Fact]
    public async Task DeleteImageAsync_WhenNotFound_ShouldReturnNotFoundFailure()
    {
        // Arrange
        _storage.DeleteImageAsync("missing.webp", Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _service.DeleteImageAsync("missing.webp");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Image.NotFound");
    }

    [Fact]
    public async Task DeleteImageAsync_WhenFound_ShouldReturnSuccess()
    {
        // Arrange
        _storage.DeleteImageAsync("existing.webp", Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _service.DeleteImageAsync("existing.webp");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
