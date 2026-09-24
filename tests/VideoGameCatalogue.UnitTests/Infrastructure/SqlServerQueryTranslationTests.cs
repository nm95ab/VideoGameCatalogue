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
            ((string)(object)g.Title).Contains(search) ||
            g.Description.Contains(search));

        query = query.Where(g => (string)(object)g.Platform == platform);
        query = query.Where(g => (string)(object)g.Genre == genre);
        query = query.OrderBy(g => g.Title);

        var sql = query.ToQueryString();

        sql.Should().NotBeNullOrWhiteSpace();
        sql.Should().Contain("FROM [VideoGames] AS [v]");
        sql.Should().Contain("WHERE");
        sql.Should().Contain("CAST([v].[Title] AS nvarchar(max)) LIKE");
        sql.Should().Contain("CAST([v].[Platform] AS nvarchar(max)) = @platform");
        sql.Should().Contain("CAST([v].[Genre] AS nvarchar(max)) = @genre");
        sql.Should().Contain("ORDER BY [v].[Title]");
    }
}
