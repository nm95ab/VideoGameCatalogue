using Azure.Storage.Blobs;
using FluentAssertions;
using VideoGameCatalogue.Infrastructure.Storage;
using Xunit;

namespace VideoGameCatalogue.UnitTests.Infrastructure;

public sealed class AzureBlobStorageImageStorageAdapterTests
{
    private const string DummyConnectionString =
        "DefaultEndpointsProtocol=https;AccountName=teststorageaccount;AccountKey=dGVzdGtleQ==;EndpointSuffix=core.windows.net";

    [Fact]
    public void GetImageUrl_WithConfiguredCdnBaseUrl_ReturnsCdnUrl()
    {
        // Arrange
        var adapter = new AzureBlobStorageImageStorageAdapter(
            DummyConnectionString,
            "game-thumbnails",
            "https://cdn.videogamecatalogue.com/thumbnails");

        // Act
        var url = adapter.GetImageUrl("thumb-456.webp");

        // Assert
        url.Should().Be("https://cdn.videogamecatalogue.com/thumbnails/thumb-456.webp");
    }

    [Fact]
    public void GetImageUrl_WithoutBaseUrl_ReturnsBlobContainerUrl()
    {
        // Arrange
        var adapter = new AzureBlobStorageImageStorageAdapter(
            DummyConnectionString,
            "game-thumbnails");

        // Act
        var url = adapter.GetImageUrl("thumb-456.webp");

        // Assert
        url.Should().Be("https://teststorageaccount.blob.core.windows.net/game-thumbnails/thumb-456.webp");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public void GetImageUrl_WhenEmptyOrNull_ReturnsEmptyString(string? imageId)
    {
        // Arrange
        var adapter = new AzureBlobStorageImageStorageAdapter(DummyConnectionString);

        // Act
        var url = adapter.GetImageUrl(imageId!);

        // Assert
        url.Should().BeEmpty();
    }

    [Fact]
    public void GetImageUrl_SanitizesPathTraversalInImageId()
    {
        // Arrange
        var adapter = new AzureBlobStorageImageStorageAdapter(
            DummyConnectionString,
            "game-thumbnails",
            "https://cdn.videogamecatalogue.com/thumbnails");

        // Act
        var url = adapter.GetImageUrl("../subfolder/thumb.webp");

        // Assert
        url.Should().Be("https://cdn.videogamecatalogue.com/thumbnails/thumb.webp");
    }
}
