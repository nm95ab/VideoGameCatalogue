using Microsoft.Extensions.Caching.Memory;
using VideoGameCatalogue.Domain.Ports;

namespace VideoGameCatalogue.Infrastructure.Persistence.Repositories;

/// <summary>
/// Outbound (Driven) Adapter decorating <see cref="ILookupRepository"/> with in-memory caching.
/// <para>
/// Caches lookup table query results (Platforms, Genres, Ratings) in <see cref="IMemoryCache"/>
/// with absolute expiration to eliminate redundant database round-trips for static reference data.
/// </para>
/// </summary>
public class CachedLookupRepository(ILookupRepository inner, IMemoryCache cache) : ILookupRepository
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);
    private const string PlatformsCacheKey = "lookup_platforms";
    private const string GenresCacheKey = "lookup_genres";
    private const string RatingsCacheKey = "lookup_ratings";

    public async Task<IReadOnlyList<string>> GetPlatformsAsync(CancellationToken cancellationToken = default)
    {
        return await cache.GetOrCreateAsync(PlatformsCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await inner.GetPlatformsAsync(cancellationToken);
        }) ?? [];
    }

    public async Task<IReadOnlyList<string>> GetGenresAsync(CancellationToken cancellationToken = default)
    {
        return await cache.GetOrCreateAsync(GenresCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await inner.GetGenresAsync(cancellationToken);
        }) ?? [];
    }

    public async Task<IReadOnlyList<string>> GetRatingsAsync(CancellationToken cancellationToken = default)
    {
        return await cache.GetOrCreateAsync(RatingsCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await inner.GetRatingsAsync(cancellationToken);
        }) ?? [];
    }
}
