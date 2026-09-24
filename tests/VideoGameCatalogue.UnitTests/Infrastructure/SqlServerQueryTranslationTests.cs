using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VideoGameCatalogue.Domain.Games;
using VideoGameCatalogue.Infrastructure.Persistence;

namespace VideoGameCatalogue.UnitTests.Infrastructure;

public sealed class SqlServerQueryTranslationTests
{
    private readonly VideoGameCatalogueDbContext _context;

    public SqlServerQueryTranslationTests()
    {
        var options = new DbContextOptionsBuilder<VideoGameCatalogueDbContext>()
            .UseSqlServer("Server=localhost;Database=VideoGameCatalogueDb;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        _context = new VideoGameCatalogueDbContext(options);
    }

    [Fact]
    public void GetAllQuery_CompilesToValidSqlServerSql_WithZeroTranslationErrors()
    {
        var search = "Zelda";
        var platform = "Nintendo 64";
        var genre = "Action-Adventure";

        IQueryable<VideoGame> query = _context.VideoGames.AsNoTracking();

        query = query.Where(g =>
            g.Title.Value.Contains(search) ||
            g.Description.Contains(search));

        query = query.Where(g => g.Platform.Value == platform);
        query = query.Where(g => g.Genre.Value == genre);
        query = query.OrderBy(g => g.Title.Value);

        var sql = query.ToQueryString();

        sql.Should().NotBeNullOrWhiteSpace();
        sql.Should().Contain("FROM [VideoGames] AS [v]");
        sql.Should().Contain("WHERE");
        sql.Should().Contain("[v].[Title] LIKE");
        sql.Should().Contain("[v].[Platform] = @platform");
        sql.Should().Contain("[v].[Genre] = @genre");
        sql.Should().Contain("ORDER BY [v].[Title]");
    }
}
