using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using VideoGameCatalogue.Domain.Ports;
using VideoGameCatalogue.Infrastructure.Persistence.Repositories;
using Xunit;

namespace VideoGameCatalogue.UnitTests.Infrastructure;

public class CachedLookupRepositoryTests : IDisposable
{
    private readonly ILookupRepository _inner;
    private readonly IMemoryCache _cache;
    private readonly CachedLookupRepository _repository;

    public CachedLookupRepositoryTests()
    {
        _inner = Substitute.For<ILookupRepository>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _repository = new CachedLookupRepository(_inner, _cache);
    }

    [Fact]
    public async Task GetPlatformsAsync_FirstCall_ShouldQueryInnerRepositoryAndCacheResult()
    {
        // Arrange
        IReadOnlyList<string> platforms = ["PC", "PlayStation 5", "Nintendo Switch"];
        _inner.GetPlatformsAsync(Arg.Any<CancellationToken>()).Returns(platforms);

        // Act
        var result = await _repository.GetPlatformsAsync();

        // Assert
        result.Should().BeEquivalentTo(platforms);
        await _inner.Received(1).GetPlatformsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPlatformsAsync_SubsequentCalls_ShouldReturnCachedResultWithoutCallingInnerAgain()
    {
        // Arrange
        IReadOnlyList<string> platforms = ["PC", "Xbox Series X"];
        _inner.GetPlatformsAsync(Arg.Any<CancellationToken>()).Returns(platforms);

        // Act
        var result1 = await _repository.GetPlatformsAsync();
        var result2 = await _repository.GetPlatformsAsync();
        var result3 = await _repository.GetPlatformsAsync();

        // Assert
        result1.Should().BeEquivalentTo(platforms);
        result2.Should().BeEquivalentTo(platforms);
        result3.Should().BeEquivalentTo(platforms);
        await _inner.Received(1).GetPlatformsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetGenresAsync_SubsequentCalls_ShouldReturnCachedResultWithoutCallingInnerAgain()
    {
        // Arrange
        IReadOnlyList<string> genres = ["Action", "RPG", "Platformer"];
        _inner.GetGenresAsync(Arg.Any<CancellationToken>()).Returns(genres);

        // Act
        var result1 = await _repository.GetGenresAsync();
        var result2 = await _repository.GetGenresAsync();

        // Assert
        result1.Should().BeEquivalentTo(genres);
        result2.Should().BeEquivalentTo(genres);
        await _inner.Received(1).GetGenresAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetRatingsAsync_SubsequentCalls_ShouldReturnCachedResultWithoutCallingInnerAgain()
    {
        // Arrange
        IReadOnlyList<string> ratings = ["Everyone", "Teen", "Mature 17+"];
        _inner.GetRatingsAsync(Arg.Any<CancellationToken>()).Returns(ratings);

        // Act
        var result1 = await _repository.GetRatingsAsync();
        var result2 = await _repository.GetRatingsAsync();

        // Assert
        result1.Should().BeEquivalentTo(ratings);
        result2.Should().BeEquivalentTo(ratings);
        await _inner.Received(1).GetRatingsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPlatformsAsync_WithCancellationToken_ShouldForwardTokenToInner()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        _inner.GetPlatformsAsync(token).Returns(["PC"]);

        // Act
        var result = await _repository.GetPlatformsAsync(token);

        // Assert
        result.Should().ContainSingle().Which.Should().Be("PC");
        await _inner.Received(1).GetPlatformsAsync(token);
    }

    public void Dispose()
    {
        _cache.Dispose();
        GC.SuppressFinalize(this);
    }
}
