using Microsoft.EntityFrameworkCore;
using VideoGameCatalogue.Domain.Ports;

namespace VideoGameCatalogue.Infrastructure.Persistence.Repositories;

/// <summary>
/// Outbound (Driven) Adapter implementing <see cref="ILookupRepository"/> using Entity Framework Core.
/// <para>
/// Queries the Platforms, Genres, and Ratings lookup tables, using <c>AsNoTracking()</c>
/// for optimal read performance without Change Tracker overhead.
/// </para>
/// </summary>
public class EfCoreLookupRepository(VideoGameCatalogueDbContext context) : ILookupRepository
{
    public async Task<IReadOnlyList<string>> GetPlatformsAsync(CancellationToken cancellationToken = default)
    {
        return await context.Platforms
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetGenresAsync(CancellationToken cancellationToken = default)
    {
        return await context.Genres
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetRatingsAsync(CancellationToken cancellationToken = default)
    {
        return await context.Ratings
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}
