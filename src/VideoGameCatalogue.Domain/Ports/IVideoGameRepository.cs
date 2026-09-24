using VideoGameCatalogue.Domain.Games;

namespace VideoGameCatalogue.Domain.Ports;

public interface IVideoGameRepository
{
    Task<IReadOnlyList<VideoGame>> GetAllAsync(
        string? searchTerm = null,
        string? platform = null,
        string? genre = null,
        CancellationToken cancellationToken = default);

    Task<VideoGame?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(VideoGame game, CancellationToken cancellationToken = default);

    Task UpdateAsync(VideoGame game, CancellationToken cancellationToken = default);

    Task DeleteAsync(VideoGame game, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
