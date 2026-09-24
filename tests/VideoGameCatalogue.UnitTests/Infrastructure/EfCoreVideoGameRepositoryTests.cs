using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VideoGameCatalogue.Domain.Games;
using VideoGameCatalogue.Domain.Games.ValueObjects;
using VideoGameCatalogue.Infrastructure.Persistence;
using VideoGameCatalogue.Infrastructure.Persistence.Repositories;

namespace VideoGameCatalogue.UnitTests.Infrastructure;

public sealed class EfCoreVideoGameRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly VideoGameCatalogueDbContext _context;
    private readonly EfCoreVideoGameRepository _repository;

    public EfCoreVideoGameRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<VideoGameCatalogueDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new VideoGameCatalogueDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new EfCoreVideoGameRepository(_context);
    }

    [Fact]
    public async Task GetAllAsync_WithSearchTerm_TranslatesAndFiltersSuccessfullyOnRelationalProvider()
    {
        // Arrange
        var game1 = VideoGame.Create(
            GameTitle.Create("The Legend of Zelda: Ocarina of Time").Value,
            Platform.Create("Nintendo 64").Value,
            Genre.Create("Action-Adventure").Value,
            ReleaseYear.Create(1998).Value,
            Rating.Create("Everyone").Value,
            "Epic hero quest").Value;

        var game2 = VideoGame.Create(
            GameTitle.Create("Super Mario 64").Value,
            Platform.Create("Nintendo 64").Value,
            Genre.Create("Platformer").Value,
            ReleaseYear.Create(1996).Value,
            Rating.Create("Everyone").Value,
            "Pioneering 3D platformer").Value;

        await _repository.AddAsync(game1);
        await _repository.AddAsync(game2);

        // Act
        var results = await _repository.GetAllAsync(searchTerm: "Zelda");

        // Assert
        results.Should().HaveCount(1);
        results[0].Title.Value.Should().Be("The Legend of Zelda: Ocarina of Time");
    }

    [Fact]
    public async Task GetAllAsync_WithPlatformAndGenre_TranslatesAndFiltersSuccessfullyOnRelationalProvider()
    {
        // Arrange
        var game = VideoGame.Create(
            GameTitle.Create("Metroid Prime").Value,
            Platform.Create("GameCube").Value,
            Genre.Create("Action-Adventure").Value,
            ReleaseYear.Create(2002).Value,
            Rating.Create("Teen").Value,
            "First person adventure").Value;

        await _repository.AddAsync(game);

        // Act
        var results = await _repository.GetAllAsync(platform: "GameCube", genre: "Action-Adventure");

        // Assert
        results.Should().HaveCount(1);
        results[0].Title.Value.Should().Be("Metroid Prime");
    }

    [Fact]
    public async Task GetAllAsync_ShouldOrderByTitleOnRelationalProvider()
    {
        // Arrange
        var gameB = VideoGame.Create(
            GameTitle.Create("Braid").Value,
            Platform.Create("PC").Value,
            Genre.Create("Puzzle").Value,
            ReleaseYear.Create(2008).Value,
            Rating.Create("Everyone").Value,
            "Time manipulation puzzle").Value;

        var gameA = VideoGame.Create(
            GameTitle.Create("Alan Wake").Value,
            Platform.Create("PC").Value,
            Genre.Create("Action-Adventure").Value,
            ReleaseYear.Create(2010).Value,
            Rating.Create("Teen").Value,
            "Psychological thriller").Value;

        await _repository.AddAsync(gameB);
        await _repository.AddAsync(gameA);

        // Act
        var results = await _repository.GetAllAsync();

        // Assert
        results.Should().HaveCount(2);
        results[0].Title.Value.Should().Be("Alan Wake");
        results[1].Title.Value.Should().Be("Braid");
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsVideoGame()
    {
        // Arrange
        var game = VideoGame.Create(
            GameTitle.Create("Halo: Combat Evolved").Value,
            Platform.Create("Xbox").Value,
            Genre.Create("Shooter").Value,
            ReleaseYear.Create(2001).Value,
            Rating.Create("Mature").Value,
            "Sci-fi shooter").Value;

        await _repository.AddAsync(game);

        // Act
        var result = await _repository.GetByIdAsync(game.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Value.Should().Be("Halo: Combat Evolved");
    }

    [Fact]
    public async Task ExistsAsync_WhenGameExists_ReturnsTrue()
    {
        // Arrange
        var game = VideoGame.Create(
            GameTitle.Create("Portal 2").Value,
            Platform.Create("PC").Value,
            Genre.Create("Puzzle").Value,
            ReleaseYear.Create(2011).Value,
            Rating.Create("Everyone").Value,
            "Physics puzzle game").Value;

        await _repository.AddAsync(game);

        // Act & Assert
        (await _repository.ExistsAsync(game.Id)).Should().BeTrue();
        (await _repository.ExistsAsync(Guid.NewGuid())).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_RemovesGameFromDatabase()
    {
        // Arrange
        var game = VideoGame.Create(
            GameTitle.Create("Doom").Value,
            Platform.Create("PC").Value,
            Genre.Create("Shooter").Value,
            ReleaseYear.Create(1993).Value,
            Rating.Create("Mature").Value,
            "Classic FPS").Value;

        await _repository.AddAsync(game);

        // Act
        await _repository.DeleteAsync(game);

        // Assert
        (await _repository.ExistsAsync(game.Id)).Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
