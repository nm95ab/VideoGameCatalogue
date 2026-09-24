using VideoGameCatalogue.Application.Games.DTOs;
using VideoGameCatalogue.Domain.Common;

namespace VideoGameCatalogue.Application.Games;

public interface IVideoGameService
{
    Task<IReadOnlyList<GameDto>> GetAllGamesAsync(
        string? searchTerm = null,
        string? platform = null,
        string? genre = null,
        CancellationToken cancellationToken = default);

    Task<Result<GameDto>> GetGameByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<GameDto>> CreateGameAsync(CreateGameRequest request, CancellationToken cancellationToken = default);

    Task<Result<GameDto>> UpdateGameAsync(Guid id, UpdateGameRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteGameAsync(Guid id, CancellationToken cancellationToken = default);

    CatalogueMetadataDto GetMetadata();
}
