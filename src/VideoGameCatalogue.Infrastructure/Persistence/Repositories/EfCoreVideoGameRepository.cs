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

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.Trim();
            query = query.Where(g =>
                EF.Functions.Like(g.Title.Value, $"%{search}%") ||
                EF.Functions.Like(g.Description, $"%{search}%"));
        }

        if (!string.IsNullOrWhiteSpace(platform))
        {
            var targetPlatform = platform.Trim();
            query = query.Where(g => g.Platform.Value == targetPlatform);
        }

        if (!string.IsNullOrWhiteSpace(genre))
        {
            var targetGenre = genre.Trim();
            query = query.Where(g => g.Genre.Value == targetGenre);
        }

        return await query
            .OrderBy(g => g.Title.Value)
            .ToListAsync(cancellationToken);
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
