namespace VideoGameCatalogue.Domain.Ports;

/// <summary>
/// Outbound (Driven) Port interface for querying reference/lookup catalogue data.
/// <para>
/// In Hexagonal (Ports &amp; Adapters) Architecture, this interface is owned by the Domain Core.
/// It defines the contract for accessing reference lookup tables (Platforms, Genres, Ratings)
/// without coupling domain logic to any specific persistence technology.
/// </para>
/// </summary>
public interface ILookupRepository
{
    Task<IReadOnlyList<string>> GetPlatformsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetGenresAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetRatingsAsync(CancellationToken cancellationToken = default);
}
