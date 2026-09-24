using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VideoGameCatalogue.Domain.Games;
using VideoGameCatalogue.Domain.Games.ValueObjects;
using VideoGameCatalogue.Infrastructure.Persistence;
using VideoGameCatalogue.Infrastructure.Persistence.Repositories;

namespace VideoGameCatalogue.UnitTests.Infrastructure;

public sealed class SqlServerLiveIntegrationTests : IDisposable
{
    private readonly VideoGameCatalogueDbContext _context;
    private readonly EfCoreVideoGameRepository _repository;

    public SqlServerLiveIntegrationTests()
    {
        var connectionString = "Server=127.0.0.1,1433;Database=VideoGameCatalogueLiveTestDb;User Id=sa;Password=YourStrong@Password123!;TrustServerCertificate=True;MultipleActiveResultSets=true;Connect Timeout=15";
        var options = new DbContextOptionsBuilder<VideoGameCatalogueDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        _context = new VideoGameCatalogueDbContext(options);
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _repository = new EfCoreVideoGameRepository(_context);
    }

    [Fact]
    public async Task LiveSqlServer_InsertAndQueryVideoGames_WorksSuccessfully()
    {
        var game = VideoGame.Create(
            GameTitle.Create("The Legend of Zelda: Ocarina of Time").Value,
            Platform.Create("Nintendo 64").Value,
            Genre.Create("Action-Adventure").Value,
            ReleaseYear.Create(1998).Value,
            Rating.Create("Everyone").Value,
            "Hero of Time adventure").Value;

        await _repository.AddAsync(game);

        var results = await _repository.GetAllAsync(searchTerm: "Zelda");

        results.Should().HaveCount(1);
        results[0].Title.Value.Should().Be("The Legend of Zelda: Ocarina of Time");
        results[0].Platform.Value.Should().Be("Nintendo 64");
    }

    [Fact]
    public async Task LiveSqlServer_MigrateSchema_CreatesExpectedIndexesOnVideoGamesTable()
    {
        await DatabaseSeeder.MigrateSchemaAsync(_context);

        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT name FROM sys.indexes WHERE object_id = OBJECT_ID('VideoGames') AND is_primary_key = 0";
        using var reader = await cmd.ExecuteReaderAsync();

        var indexNames = new List<string>();
        while (await reader.ReadAsync())
        {
            indexNames.Add(reader.GetString(0));
        }

        indexNames.Should().Contain([
            "IX_VideoGames_Platform",
            "IX_VideoGames_Genre",
            "IX_VideoGames_Title",
            "IX_VideoGames_Platform_Genre_Title"
        ]);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
