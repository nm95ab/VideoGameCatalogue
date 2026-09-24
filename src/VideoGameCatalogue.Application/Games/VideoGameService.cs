using VideoGameCatalogue.Application.Games.DTOs;
using VideoGameCatalogue.Domain.Common;
using VideoGameCatalogue.Domain.Games;
using VideoGameCatalogue.Domain.Games.ValueObjects;
using VideoGameCatalogue.Domain.Ports;

namespace VideoGameCatalogue.Application.Games;

/// <summary>
/// Application Service implementing the <see cref="IVideoGameService"/> Inbound Port.
/// <para>
/// Orchestrates application use cases by:
/// 1. Validating incoming DTO primitive inputs into strongly typed Domain Value Objects.
/// 2. Invoking Aggregate Root business methods on <see cref="VideoGame"/>.
/// 3. Delegating persistence to the outbound <see cref="IVideoGameRepository"/> port.
/// 4. Projecting domain entities into presentation-agnostic <see cref="GameDto"/> records.
/// </para>
/// </summary>
public class VideoGameService(
    IVideoGameRepository repository,
    ILookupRepository lookupRepository,
    TimeProvider? timeProvider = null,
    IImageStoragePort? imageStorage = null) : IVideoGameService
{
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    public async Task<IReadOnlyList<GameDto>> GetAllGamesAsync(
        string? searchTerm = null,
        string? platform = null,
        string? genre = null,
        CancellationToken cancellationToken = default)
    {
        var games = await repository.GetAllAsync(searchTerm, platform, genre, cancellationToken);
        return games.Select(g => GameDto.FromDomain(g, imageStorage)).ToList();
    }

    public async Task<Result<GameDto>> GetGameByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return Result<GameDto>.Failure(GameErrors.InvalidId);

        var game = await repository.GetByIdAsync(id, cancellationToken);
        if (game is null)
            return Result<GameDto>.Failure(GameErrors.NotFound);

        return Result<GameDto>.Success(GameDto.FromDomain(game, imageStorage));
    }

    public async Task<Result<GameDto>> CreateGameAsync(CreateGameRequest request, CancellationToken cancellationToken = default)
    {
        var parsed = ParseValueObjects(request.Title, request.Platform, request.Genre, request.ReleaseYear, request.Rating, _timeProvider.GetUtcNow().Year);
        if (parsed.IsFailure)
            return Result<GameDto>.Failure(parsed.Error);

        var (title, platform, genre, releaseYear, rating) = parsed.Value;

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var gameResult = VideoGame.Create(title, platform, genre, releaseYear, rating, request.Description, utcNow, request.ImageId);
        if (gameResult.IsFailure)
            return Result<GameDto>.Failure(gameResult.Error);

        var game = gameResult.Value;
        await repository.AddAsync(game, cancellationToken);

        return Result<GameDto>.Success(GameDto.FromDomain(game, imageStorage));
    }

    public async Task<Result<GameDto>> UpdateGameAsync(Guid id, UpdateGameRequest request, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return Result<GameDto>.Failure(GameErrors.InvalidId);

        var game = await repository.GetByIdAsync(id, cancellationToken);
        if (game is null)
            return Result<GameDto>.Failure(GameErrors.NotFound);

        var parsed = ParseValueObjects(request.Title, request.Platform, request.Genre, request.ReleaseYear, request.Rating, _timeProvider.GetUtcNow().Year);
        if (parsed.IsFailure)
            return Result<GameDto>.Failure(parsed.Error);

        var (title, platform, genre, releaseYear, rating) = parsed.Value;

        var oldImageId = game.ImageId;
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var updateResult = game.UpdateDetails(title, platform, genre, releaseYear, rating, request.Description, utcNow, request.ImageId);
        if (updateResult.IsFailure)
            return Result<GameDto>.Failure(updateResult.Error);

        await repository.UpdateAsync(game, cancellationToken);

        if (imageStorage is not null && !string.IsNullOrWhiteSpace(oldImageId) && oldImageId != request.ImageId)
        {
            await imageStorage.DeleteImageAsync(oldImageId, cancellationToken);
        }

        return Result<GameDto>.Success(GameDto.FromDomain(game, imageStorage));
    }

    public async Task<Result> DeleteGameAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return Result.Failure(GameErrors.InvalidId);

        var game = await repository.GetByIdAsync(id, cancellationToken);
        if (game is null)
            return Result.Failure(GameErrors.NotFound);

        var imageId = game.ImageId;
        await repository.DeleteAsync(game, cancellationToken);

        if (imageStorage is not null && !string.IsNullOrWhiteSpace(imageId))
        {
            await imageStorage.DeleteImageAsync(imageId, cancellationToken);
        }

        return Result.Success();
    }

    public async Task<CatalogueMetadataDto> GetMetadataAsync(CancellationToken cancellationToken = default)
    {
        var platforms = await lookupRepository.GetPlatformsAsync(cancellationToken);
        var genres = await lookupRepository.GetGenresAsync(cancellationToken);
        var ratings = await lookupRepository.GetRatingsAsync(cancellationToken);

        return new CatalogueMetadataDto(platforms, genres, ratings);
    }

    /// <summary>
    /// Validates and parses raw DTO primitives into domain Value Objects.
    /// <para>
    /// Acts as a fail-fast anti-corruption boundary: if any primitive input violates domain invariants
    /// (e.g. empty title, year out of range), evaluation halts and returns an immediate <see cref="Result.Failure(Error)"/>,
    /// preventing invalid arguments from reaching the domain model.
    /// </para>
    /// </summary>
    private static Result<(GameTitle Title, Platform Platform, Genre Genre, ReleaseYear Year, Rating Rating)> ParseValueObjects(
        string? rawTitle,
        string? rawPlatform,
        string? rawGenre,
        int rawYear,
        string? rawRating,
        int currentYear)
    {
        var titleResult = GameTitle.Create(rawTitle);
        if (titleResult.IsFailure) return Result<(GameTitle, Platform, Genre, ReleaseYear, Rating)>.Failure(titleResult.Error);

        var platformResult = Platform.Create(rawPlatform);
        if (platformResult.IsFailure) return Result<(GameTitle, Platform, Genre, ReleaseYear, Rating)>.Failure(platformResult.Error);

        var genreResult = Genre.Create(rawGenre);
        if (genreResult.IsFailure) return Result<(GameTitle, Platform, Genre, ReleaseYear, Rating)>.Failure(genreResult.Error);

        var yearResult = ReleaseYear.Create(rawYear, currentYear);
        if (yearResult.IsFailure) return Result<(GameTitle, Platform, Genre, ReleaseYear, Rating)>.Failure(yearResult.Error);

        var ratingResult = Rating.Create(rawRating);
        if (ratingResult.IsFailure) return Result<(GameTitle, Platform, Genre, ReleaseYear, Rating)>.Failure(ratingResult.Error);

        return Result<(GameTitle, Platform, Genre, ReleaseYear, Rating)>.Success(
            (titleResult.Value, platformResult.Value, genreResult.Value, yearResult.Value, ratingResult.Value));
    }
}
