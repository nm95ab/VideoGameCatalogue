using VideoGameCatalogue.Domain.Common;
using VideoGameCatalogue.Domain.Games;

namespace VideoGameCatalogue.Domain.Ports;

/// <summary>
/// Outbound (Driven) Port interface for <see cref="VideoGame"/> persistence.
/// <para>
/// In Hexagonal (Ports &amp; Adapters) Architecture, this port is owned by the Core Domain/Application layer.
/// It defines the contract for persisting and retrieving Aggregate Roots without coupling the core
/// to any specific database technology or ORM (e.g. EF Core, SQL Server).
/// </para>
/// </summary>
public interface IVideoGameRepository
{
    Task<PagedResult<VideoGame>> GetPagedAsync(
        string? searchTerm = null,
        string? platform = null,
        string? genre = null,
        string? era = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VideoGame>> GetAllAsync(
        string? searchTerm = null,
        string? platform = null,
        string? genre = null,
        string? era = null,
        CancellationToken cancellationToken = default);

    Task<VideoGame?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(VideoGame game, CancellationToken cancellationToken = default);

    Task UpdateAsync(VideoGame game, CancellationToken cancellationToken = default);

    Task DeleteAsync(VideoGame game, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
