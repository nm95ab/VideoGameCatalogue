using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FluentAssertions;
using NSubstitute;
using VideoGameCatalogue.Infrastructure.Storage;
using Xunit;

namespace VideoGameCatalogue.UnitTests.Infrastructure;

public sealed class AzureBlobStorageImageStorageAdapterTests
{
    private const string DummyConnectionString =
        "DefaultEndpointsProtocol=https;AccountName=teststorageaccount;AccountKey=dGVzdGtleQ==;EndpointSuffix=core.windows.net";

    [Fact]
    public async Task SaveImageAsync_CreatesContainerAndUploadsBlob_ReturnsGeneratedImageId()
    {
        // Arrange
        var blobServiceClient = Substitute.For<BlobServiceClient>();
        var containerClient = Substitute.For<BlobContainerClient>();
        var blobClient = Substitute.For<BlobClient>();

        blobServiceClient.GetBlobContainerClient("game-thumbnails").Returns(containerClient);
        containerClient.GetBlobClient(Arg.Any<string>()).Returns(blobClient);

        var adapter = new AzureBlobStorageImageStorageAdapter(blobServiceClient, "game-thumbnails");
        using var stream = new MemoryStream([1, 2, 3, 4]);

        // Act
        var imageId = await adapter.SaveImageAsync(stream, "image/webp", ".webp", CancellationToken.None);

        // Assert
        imageId.Should().EndWith(".webp");
        await containerClient.Received(1).CreateIfNotExistsAsync(
            PublicAccessType.None,
            cancellationToken: Arg.Any<CancellationToken>());
        await blobClient.Received(1).UploadAsync(
            Arg.Any<Stream>(),
            Arg.Is<BlobHttpHeaders>(h => h.ContentType == "image/webp"),
            cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveImageAsync_WhenExtensionMissingDot_PrependsDot()
    {
        // Arrange
        var blobServiceClient = Substitute.For<BlobServiceClient>();
        var containerClient = Substitute.For<BlobContainerClient>();
        var blobClient = Substitute.For<BlobClient>();

        blobServiceClient.GetBlobContainerClient("game-thumbnails").Returns(containerClient);
        containerClient.GetBlobClient(Arg.Any<string>()).Returns(blobClient);

        var adapter = new AzureBlobStorageImageStorageAdapter(blobServiceClient, "game-thumbnails");
        using var stream = new MemoryStream([1, 2, 3, 4]);

        // Act
        var imageId = await adapter.SaveImageAsync(stream, "image/png", "png", CancellationToken.None);

        // Assert
        imageId.Should().EndWith(".png");
    }

    [Fact]
    public async Task DeleteImageAsync_WhenImageIdValid_CallsDeleteIfExists()
    {
        // Arrange
        var blobServiceClient = Substitute.For<BlobServiceClient>();
        var containerClient = Substitute.For<BlobContainerClient>();
        var blobClient = Substitute.For<BlobClient>();

        blobServiceClient.GetBlobContainerClient("game-thumbnails").Returns(containerClient);
        containerClient.GetBlobClient("test-img.webp").Returns(blobClient);

        var rawResponse = Substitute.For<Response>();
        blobClient.DeleteIfExistsAsync(cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Response.FromValue(true, rawResponse));

        var adapter = new AzureBlobStorageImageStorageAdapter(blobServiceClient, "game-thumbnails");

        // Act
        var result = await adapter.DeleteImageAsync("test-img.webp", CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        await blobClient.Received(1).DeleteIfExistsAsync(cancellationToken: Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task DeleteImageAsync_WhenImageIdNullOrWhitespace_ReturnsFalse(string? imageId)
    {
        // Arrange
        var blobServiceClient = Substitute.For<BlobServiceClient>();
        var containerClient = Substitute.For<BlobContainerClient>();
        blobServiceClient.GetBlobContainerClient("game-thumbnails").Returns(containerClient);

        var adapter = new AzureBlobStorageImageStorageAdapter(blobServiceClient, "game-thumbnails");

        // Act
        var result = await adapter.DeleteImageAsync(imageId!, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        containerClient.DidNotReceive().GetBlobClient(Arg.Any<string>());
    }



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
