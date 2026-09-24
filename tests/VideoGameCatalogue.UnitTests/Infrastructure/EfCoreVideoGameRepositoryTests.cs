using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VideoGameCatalogue.Domain.Games;
using VideoGameCatalogue.Domain.Games.ValueObjects;
using VideoGameCatalogue.Infrastructure.Persistence;
using VideoGameCatalogue.Infrastructure.Persistence.Repositories;

namespace VideoGameCatalogue.UnitTests.Infrastructure;

public sealed class EfCoreVideoGameRepositoryTests : IDisposable
{
    private readonly VideoGameCatalogueDbContext _context;
    private readonly EfCoreVideoGameRepository _repository;

    public EfCoreVideoGameRepositoryTests()
    {
        var connectionString = "Server=127.0.0.1,1433;Database=VideoGameCatalogueRepoTestsDb;User Id=sa;Password=YourStrong@Password123!;TrustServerCertificate=True;MultipleActiveResultSets=true;Connect Timeout=15";
        var options = new DbContextOptionsBuilder<VideoGameCatalogueDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        _context = new VideoGameCatalogueDbContext(options);
        _context.Database.EnsureCreated();
        DatabaseSeeder.MigrateSchema(_context);
        _context.VideoGames.ExecuteDelete();

        _repository = new EfCoreVideoGameRepository(_context);
    }

    [Fact]
    public async Task GetAllAsync_WithSearchTerm_FiltersByTitleOrDescription()
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
    public async Task GetAllAsync_WithPlatformAndGenre_FiltersCorrectly()
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
    public async Task GetAllAsync_ShouldOrderByTitle()
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

    [Fact]
    public async Task UpdateAsync_ModifiesExistingGameInDatabase()
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
        game.UpdateDetails(
            GameTitle.Create("Metroid Prime Remastered").Value,
            Platform.Create("Nintendo Switch").Value,
            Genre.Create("Action-Adventure").Value,
            ReleaseYear.Create(2023).Value,
            Rating.Create("Teen").Value,
            "Remastered version for Switch");

        await _repository.UpdateAsync(game);

        // Assert
        var updated = await _repository.GetByIdAsync(game.Id);
        updated.Should().NotBeNull();
        updated!.Title.Value.Should().Be("Metroid Prime Remastered");
        updated.Platform.Value.Should().Be("Nintendo Switch");
        updated.ReleaseYear.Value.Should().Be(2023);
        updated.Description.Should().Be("Remastered version for Switch");
    }

    [Fact]
    public async Task GetPagedAsync_WithPageAndPageSize_ReturnsPagedSliceAndTotalCount()
    {
        // Arrange
        for (var i = 1; i <= 5; i++)
        {
            var game = VideoGame.Create(
                GameTitle.Create($"Game {i:D2}").Value,
                Platform.Create("PC").Value,
                Genre.Create("Action").Value,
                ReleaseYear.Create(2000 + i).Value,
                Rating.Create("Everyone").Value).Value;
            await _repository.AddAsync(game);
        }

        // Act - Page 2 with PageSize 2
        var pagedResult = await _repository.GetPagedAsync(pageNumber: 2, pageSize: 2);

        // Assert
        pagedResult.TotalCount.Should().Be(5);
        pagedResult.PageNumber.Should().Be(2);
        pagedResult.PageSize.Should().Be(2);
        pagedResult.TotalPages.Should().Be(3);
        pagedResult.Items.Should().HaveCount(2);
        pagedResult.Items[0].Title.Value.Should().Be("Game 03");
        pagedResult.Items[1].Title.Value.Should().Be("Game 04");
    }

    [Fact]
    public async Task GetPagedAsync_WithFilters_AppliesFiltersBeforePaging()
    {
        // Arrange
        var g1 = VideoGame.Create(GameTitle.Create("Halo 1").Value, Platform.Create("Xbox").Value, Genre.Create("Shooter").Value, ReleaseYear.Create(2001).Value, Rating.Create("Mature 17+").Value).Value;
        var g2 = VideoGame.Create(GameTitle.Create("Halo 2").Value, Platform.Create("Xbox").Value, Genre.Create("Shooter").Value, ReleaseYear.Create(2004).Value, Rating.Create("Mature 17+").Value).Value;
        var g3 = VideoGame.Create(GameTitle.Create("Mario").Value, Platform.Create("Nintendo 64").Value, Genre.Create("Platformer").Value, ReleaseYear.Create(1996).Value, Rating.Create("Everyone").Value).Value;

        await _repository.AddAsync(g1);
        await _repository.AddAsync(g2);
        await _repository.AddAsync(g3);

        // Act
        var result = await _repository.GetPagedAsync(searchTerm: "Halo", platform: "Xbox", genre: "Shooter", pageNumber: 1, pageSize: 10);

        // Assert
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.Select(x => x.Title.Value).Should().Contain(["Halo 1", "Halo 2"]);
    }

    [Fact]
    public async Task GetPagedAsync_WhenPageOutOfRange_ReturnsEmptyItemsWithCorrectTotalCount()
    {
        // Arrange
        var g = VideoGame.Create(GameTitle.Create("Zelda").Value, Platform.Create("NES").Value, Genre.Create("Action-Adventure").Value, ReleaseYear.Create(1986).Value, Rating.Create("Everyone").Value).Value;
        await _repository.AddAsync(g);

        // Act
        var result = await _repository.GetPagedAsync(pageNumber: 5, pageSize: 10);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPagedAsync_WhenPageNumberOrPageSizeInvalid_ClampsToValidRanges()
    {
        // Arrange
        var g = VideoGame.Create(GameTitle.Create("Doom").Value, Platform.Create("PC").Value, Genre.Create("Shooter").Value, ReleaseYear.Create(1993).Value, Rating.Create("Mature 17+").Value).Value;
        await _repository.AddAsync(g);

        // Act - negative page number and excessive page size
        var result = await _repository.GetPagedAsync(pageNumber: -1, pageSize: 500);

        // Assert
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(100);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPagedAsync_WithEraFilter_FiltersByEraYears()
    {
        // Arrange
        var retroGame = VideoGame.Create(GameTitle.Create("Super Mario World").Value, Platform.Create("SNES").Value, Genre.Create("Platformer").Value, ReleaseYear.Create(1990).Value, Rating.Create("Everyone").Value).Value;
        var modernGame = VideoGame.Create(GameTitle.Create("Elden Ring").Value, Platform.Create("PS5").Value, Genre.Create("Action").Value, ReleaseYear.Create(2022).Value, Rating.Create("Mature 17+").Value).Value;

        await _repository.AddAsync(retroGame);
        await _repository.AddAsync(modernGame);

        // Act - 4th Gen is 1987-1992
        var result = await _repository.GetPagedAsync(era: "4th-gen");

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items[0].Title.Value.Should().Be("Super Mario World");
    }

    [Fact]
    public async Task GetPagedAsync_WithOngoingEraFilter_FiltersByYearGreaterThanOrEqualToStartYear()
    {
        // Arrange
        var retroGame = VideoGame.Create(GameTitle.Create("Super Mario World").Value, Platform.Create("SNES").Value, Genre.Create("Platformer").Value, ReleaseYear.Create(1990).Value, Rating.Create("Everyone").Value).Value;
        var modernGame = VideoGame.Create(GameTitle.Create("Elden Ring").Value, Platform.Create("PS5").Value, Genre.Create("Action").Value, ReleaseYear.Create(2022).Value, Rating.Create("Mature 17+").Value).Value;

        await _repository.AddAsync(retroGame);
        await _repository.AddAsync(modernGame);

        // Act - 9th Gen is 2020-null
        var result = await _repository.GetPagedAsync(era: "9th-gen");

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items[0].Title.Value.Should().Be("Elden Ring");
    }

    [Fact]
    public async Task GetPagedAsync_WithInvalidEraFilter_IgnoresEraFilter()
    {
        // Arrange
        var retroGame = VideoGame.Create(GameTitle.Create("Super Mario World").Value, Platform.Create("SNES").Value, Genre.Create("Platformer").Value, ReleaseYear.Create(1990).Value, Rating.Create("Everyone").Value).Value;
        var modernGame = VideoGame.Create(GameTitle.Create("Elden Ring").Value, Platform.Create("PS5").Value, Genre.Create("Action").Value, ReleaseYear.Create(2022).Value, Rating.Create("Mature 17+").Value).Value;

        await _repository.AddAsync(retroGame);
        await _repository.AddAsync(modernGame);

        // Act
        var result = await _repository.GetPagedAsync(era: "not-a-valid-era");

        // Assert
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_WithEraFilter_FiltersByEraYears()
    {
        // Arrange
        var retroGame = VideoGame.Create(GameTitle.Create("Super Mario World").Value, Platform.Create("SNES").Value, Genre.Create("Platformer").Value, ReleaseYear.Create(1990).Value, Rating.Create("Everyone").Value).Value;
        var modernGame = VideoGame.Create(GameTitle.Create("Elden Ring").Value, Platform.Create("PS5").Value, Genre.Create("Action").Value, ReleaseYear.Create(2022).Value, Rating.Create("Mature 17+").Value).Value;

        await _repository.AddAsync(retroGame);
        await _repository.AddAsync(modernGame);

        // Act
        var results = await _repository.GetAllAsync(era: "4th-gen");

        // Assert
        results.Should().HaveCount(1);
        results[0].Title.Value.Should().Be("Super Mario World");
    }


    public void Dispose()
    {
        _context.Dispose();
    }
}
