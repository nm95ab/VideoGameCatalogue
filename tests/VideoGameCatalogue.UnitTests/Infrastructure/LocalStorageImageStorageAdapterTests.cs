using System.Text;
using FluentAssertions;
using VideoGameCatalogue.Infrastructure.Storage;
using Xunit;

namespace VideoGameCatalogue.UnitTests.Infrastructure;

public sealed class LocalStorageImageStorageAdapterTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly LocalStorageImageStorageAdapter _adapter;

    public LocalStorageImageStorageAdapterTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "VideoGameCatalogue_Tests_" + Guid.NewGuid().ToString("N"));
        _adapter = new LocalStorageImageStorageAdapter(_tempDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task SaveImageAsync_SavesFileToStorageDirectoryAndReturnsId()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("sample-image-data"));

        // Act
        var imageId = await _adapter.SaveImageAsync(stream, "image/webp", ".webp");

        // Assert
        imageId.Should().EndWith(".webp");
        var expectedPath = Path.Combine(_tempDirectory, imageId);
        File.Exists(expectedPath).Should().BeTrue();
    }

    [Fact]
    public async Task GetImageAsync_WhenFileExists_ReturnsStreamAndContentType()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("sample-image-data"));
        var imageId = await _adapter.SaveImageAsync(stream, "image/webp", ".webp");

        // Act
        var result = await _adapter.GetImageAsync(imageId);

        // Assert
        result.Should().NotBeNull();
        result!.Value.ContentType.Should().Be("image/webp");
        using var reader = new StreamReader(result.Value.Stream);
        var content = await reader.ReadToEndAsync();
        content.Should().Be("sample-image-data");
    }

    [Theory]
    [InlineData("../secret.txt")]
    [InlineData("..\\secret.txt")]
    [InlineData("/etc/passwd")]
    [InlineData("")]
    public async Task GetImageAsync_WhenPathUnsafeOrMissing_ReturnsNull(string invalidId)
    {
        // Act
        var result = await _adapter.GetImageAsync(invalidId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteImageAsync_WhenFileExists_DeletesFileAndReturnsTrue()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("sample-image-data"));
        var imageId = await _adapter.SaveImageAsync(stream, "image/webp", ".webp");

        // Act
        var deleted = await _adapter.DeleteImageAsync(imageId);

        // Assert
        deleted.Should().BeTrue();
        var expectedPath = Path.Combine(_tempDirectory, imageId);
        File.Exists(expectedPath).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteImageAsync_WhenFileDoesNotExist_ReturnsFalse()
    {
        // Act
        var deleted = await _adapter.DeleteImageAsync("nonexistent.webp");

        // Assert
        deleted.Should().BeFalse();
    }

    [Fact]
    public void GetImageUrl_DefaultConfiguration_ReturnsRelativeApiPath()
    {
        // Act
        var url = _adapter.GetImageUrl("thumb-123.webp");

        // Assert
        url.Should().Be("/api/images/thumb-123.webp");
    }

    [Fact]
    public void GetImageUrl_WithCustomBaseUrl_ReturnsFullUrl()
    {
        // Arrange
        var adapterWithBaseUrl = new LocalStorageImageStorageAdapter(_tempDirectory, "http://localhost:5111/api/images");

        // Act
        var url = adapterWithBaseUrl.GetImageUrl("thumb-123.webp");

        // Assert
        url.Should().Be("http://localhost:5111/api/images/thumb-123.webp");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("../malicious.webp")]
    [InlineData("sub/malicious.webp")]
    public void GetImageUrl_WhenUnsafeOrEmpty_ReturnsEmptyString(string unsafeId)
    {
        // Act
        var url = _adapter.GetImageUrl(unsafeId);

        // Assert
        url.Should().BeEmpty();
    }
}
