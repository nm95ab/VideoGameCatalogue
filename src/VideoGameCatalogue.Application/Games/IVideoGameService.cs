using VideoGameCatalogue.Application.Games.DTOs;
using VideoGameCatalogue.Domain.Common;

namespace VideoGameCatalogue.Application.Games;

/// <summary>
/// Inbound (Driving) Port defining the application use cases for the Video Game Catalogue.
/// <para>
/// Acts as the boundary interface between Driving Adapters (ASP.NET Minimal APIs, CLI, or test runners)
/// and the Application Core. It accepts application DTOs/primitives, coordinates domain logic, and returns
/// Railway-Oriented <see cref="Result{TValue}"/> results to the caller.
/// </para>
/// </summary>
public interface IVideoGameService
{
    Task<PagedResult<GameDto>> GetGamesAsync(
        string? searchTerm = null,
        string? platform = null,
        string? genre = null,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GameDto>> GetAllGamesAsync(
        string? searchTerm = null,
        string? platform = null,
        string? genre = null,
        CancellationToken cancellationToken = default);

    Task<Result<GameDto>> GetGameByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<GameDto>> CreateGameAsync(CreateGameRequest request, CancellationToken cancellationToken = default);

    Task<Result<GameDto>> UpdateGameAsync(Guid id, UpdateGameRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteGameAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CatalogueMetadataDto> GetMetadataAsync(CancellationToken cancellationToken = default);
}
