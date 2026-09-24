using Microsoft.EntityFrameworkCore;
using VideoGameCatalogue.Domain.Games;
using VideoGameCatalogue.Domain.Ports;

namespace VideoGameCatalogue.Infrastructure.Persistence.Repositories;

public class EfCoreVideoGameRepository(VideoGameCatalogueDbContext context) : IVideoGameRepository
{
    public async Task<IReadOnlyList<VideoGame>> GetAllAsync(
        string? searchTerm = null,
        string? platform = null,
        string? genre = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<VideoGame> query = context.VideoGames.AsNoTracking();
        

        query = ApplySearchFilter(query, searchTerm);
        query = ApplyPlatformFilter(query, platform);
        query = ApplyGenreFilter(query, genre);
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
