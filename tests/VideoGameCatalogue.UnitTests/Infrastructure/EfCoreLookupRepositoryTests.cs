using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VideoGameCatalogue.Domain.Lookups;
using VideoGameCatalogue.Infrastructure.Persistence;
using VideoGameCatalogue.Infrastructure.Persistence.Repositories;

namespace VideoGameCatalogue.UnitTests.Infrastructure;

public sealed class EfCoreLookupRepositoryTests : IDisposable
{
    private readonly VideoGameCatalogueDbContext _context;
    private readonly EfCoreLookupRepository _repository;

    public EfCoreLookupRepositoryTests()
    {
        var connectionString = "Server=127.0.0.1,1433;Database=VideoGameCatalogueLookupRepoTestsDb;User Id=sa;Password=YourStrong@Password123!;TrustServerCertificate=True;MultipleActiveResultSets=true;Connect Timeout=15";
        var options = new DbContextOptionsBuilder<VideoGameCatalogueDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        _context = new VideoGameCatalogueDbContext(options);
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _repository = new EfCoreLookupRepository(_context);
    }

    [Fact]
    public async Task GetPlatformsAsync_ReturnsPlatformsOrderedById()
    {
        // Arrange
        _context.Platforms.AddRange(
            new PlatformLookup("PC"),
            new PlatformLookup("PlayStation 5"),
            new PlatformLookup("Nintendo Switch"));
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPlatformsAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Equal("PC", "PlayStation 5", "Nintendo Switch");
    }

    [Fact]
    public async Task GetGenresAsync_ReturnsGenresOrderedById()
    {
        // Arrange
        _context.Genres.AddRange(
            new GenreLookup("Action"),
            new GenreLookup("Role-Playing (RPG)"),
            new GenreLookup("Puzzle"));
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetGenresAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Equal("Action", "Role-Playing (RPG)", "Puzzle");
    }

    [Fact]
    public async Task GetRatingsAsync_ReturnsRatingsOrderedById()
    {
        // Arrange
        _context.Ratings.AddRange(
            new RatingLookup("Everyone"),
            new RatingLookup("Teen"),
            new RatingLookup("Mature 17+"));
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetRatingsAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Equal("Everyone", "Teen", "Mature 17+");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
