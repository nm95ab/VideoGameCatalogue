using Microsoft.EntityFrameworkCore;
using VideoGameCatalogue.Domain.Common;
using VideoGameCatalogue.Domain.Games;
using VideoGameCatalogue.Domain.Games.ValueObjects;
using VideoGameCatalogue.Domain.Ports;

namespace VideoGameCatalogue.Infrastructure.Persistence.Repositories;

/// <summary>
/// Outbound (Driven) Adapter implementing <see cref="IVideoGameRepository"/> with Entity Framework Core.
/// <para>
/// Persists and queries <see cref="VideoGame"/> Aggregate Roots against Microsoft SQL Server
/// (or In-Memory during testing) via <see cref="VideoGameCatalogueDbContext"/>.
/// </para>
/// </summary>
public class EfCoreVideoGameRepository(VideoGameCatalogueDbContext context) : IVideoGameRepository
{
    public async Task<PagedResult<VideoGame>> GetPagedAsync(
        string? searchTerm = null,
        string? platform = null,
        string? genre = null,
        string? era = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var validPageNumber = pageNumber < 1 ? 1 : pageNumber;
        var validPageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize);

        IQueryable<VideoGame> query = context.VideoGames.AsNoTracking();

        query = ApplySearchFilter(query, searchTerm);
        query = ApplyPlatformFilter(query, platform);
        query = ApplyGenreFilter(query, genre);
        query = ApplyEraFilter(query, era);

        var totalCount = await query.CountAsync(cancellationToken);

        query = ApplyOrdering(query);

        var items = await query
            .Skip((validPageNumber - 1) * validPageSize)
            .Take(validPageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<VideoGame>(items, validPageNumber, validPageSize, totalCount);
    }
    /// <summary>
    /// Retrieves all matching video games based on optional search term, platform, and genre filters.
    /// <para>
    /// Uses <c>AsNoTracking()</c> to bypass EF Core Change Tracker snapshotting, minimizing heap allocations
    /// and maximizing query throughput for read-only catalogue browsing.
    /// </para>
    /// </summary>
    public async Task<IReadOnlyList<VideoGame>> GetAllAsync(
        string? searchTerm = null,
        string? platform = null,
        string? genre = null,
        string? era = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<VideoGame> query = context.VideoGames.AsNoTracking();

        query = ApplySearchFilter(query, searchTerm);
        query = ApplyPlatformFilter(query, platform);
        query = ApplyGenreFilter(query, genre);
        query = ApplyEraFilter(query, era);
        query = ApplyOrdering(query);

        return await query.ToListAsync(cancellationToken);
    }

    private static IQueryable<VideoGame> ApplySearchFilter(IQueryable<VideoGame> query, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        var search = searchTerm.Trim();
        return query.Where(g => g.Title.Value.Contains(search) || g.Description.Contains(search));
    }

    private static IQueryable<VideoGame> ApplyPlatformFilter(IQueryable<VideoGame> query, string? platform)
    {
        if (string.IsNullOrWhiteSpace(platform))
            return query;

        var targetPlatform = platform.Trim();
        return query.Where(g => g.Platform.Value == targetPlatform);
    }

    private static IQueryable<VideoGame> ApplyGenreFilter(IQueryable<VideoGame> query, string? genre)
    {
        if (string.IsNullOrWhiteSpace(genre))
            return query;

        var targetGenre = genre.Trim();
        return query.Where(g => g.Genre.Value == targetGenre);
    }

    private static IQueryable<VideoGame> ApplyEraFilter(IQueryable<VideoGame> query, string? eraKey)
    {
        if (string.IsNullOrWhiteSpace(eraKey))
            return query;

        var era = GamingEra.FromKey(eraKey);
        if (era is null)
            return query;

        var startYear = era.Value.StartYear;
        if (era.Value.EndYear.HasValue)
        {
            var endYear = era.Value.EndYear.Value;
            return query.Where(g => g.ReleaseYear.Value >= startYear && g.ReleaseYear.Value <= endYear);
        }

        return query.Where(g => g.ReleaseYear.Value >= startYear);
    }

    private static IQueryable<VideoGame> ApplyOrdering(IQueryable<VideoGame> query)
    {
        return query.OrderBy(g => g.Title.Value);
    }

    public async Task<VideoGame?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.VideoGames
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task AddAsync(VideoGame game, CancellationToken cancellationToken = default)
    {
        await context.VideoGames.AddAsync(game, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(VideoGame game, CancellationToken cancellationToken = default)
    {
        context.VideoGames.Update(game);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(VideoGame game, CancellationToken cancellationToken = default)
    {
        context.VideoGames.Remove(game);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.VideoGames
            .AnyAsync(g => g.Id == id, cancellationToken);
    }
}
