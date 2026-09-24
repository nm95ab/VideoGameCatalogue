using VideoGameCatalogue.Application.Games.DTOs;
using VideoGameCatalogue.Domain.Common;
using VideoGameCatalogue.Domain.Games;
using VideoGameCatalogue.Domain.Games.ValueObjects;
using VideoGameCatalogue.Domain.Ports;

namespace VideoGameCatalogue.Application.Games;

public class VideoGameService(IVideoGameRepository repository) : IVideoGameService
{
    private static readonly string[] PredefinedPlatforms =
    [
        "PC",
        "PlayStation 5",
        "PlayStation 4",
        "Xbox Series X/S",
        "Xbox One",
        "Nintendo Switch",
        "Nintendo 64",
        "SNES",
        "NES",
        "Sega Genesis"
    ];

    private static readonly string[] PredefinedGenres =
    [
        "Action",
        "Action-Adventure",
        "Role-Playing (RPG)",
        "Strategy",
        "Platformer",
        "Shooter",
        "Simulation",
        "Sports",
        "Racing",
        "Fighting",
        "Puzzle",
        "Survival Horror"
    ];

    private static readonly string[] PredefinedRatings =
    [
        "Everyone",
        "Everyone 10+",
        "Teen",
        "Mature 17+",
        "Adults Only 18+",
        "Rating Pending"
    ];

    public async Task<IReadOnlyList<GameDto>> GetAllGamesAsync(
        string? searchTerm = null,
        string? platform = null,
        string? genre = null,
        CancellationToken cancellationToken = default)
    {
        var games = await repository.GetAllAsync(searchTerm, platform, genre, cancellationToken);
        return games.Select(GameDto.FromDomain).ToList();
    }

    public async Task<Result<GameDto>> GetGameByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return Result<GameDto>.Failure(GameErrors.InvalidId);

        var game = await repository.GetByIdAsync(id, cancellationToken);
        if (game is null)
            return Result<GameDto>.Failure(GameErrors.NotFound);

        return Result<GameDto>.Success(GameDto.FromDomain(game));
    }

    public async Task<Result<GameDto>> CreateGameAsync(CreateGameRequest request, CancellationToken cancellationToken = default)
    {
        var parsed = ParseValueObjects(request.Title, request.Platform, request.Genre, request.ReleaseYear, request.Rating);
        if (parsed.IsFailure)
            return Result<GameDto>.Failure(parsed.Error);

        var (title, platform, genre, releaseYear, rating) = parsed.Value;

        var gameResult = VideoGame.Create(title, platform, genre, releaseYear, rating, request.Description);
        if (gameResult.IsFailure)
            return Result<GameDto>.Failure(gameResult.Error);

        var game = gameResult.Value;
        await repository.AddAsync(game, cancellationToken);

        return Result<GameDto>.Success(GameDto.FromDomain(game));
    }

    public async Task<Result<GameDto>> UpdateGameAsync(Guid id, UpdateGameRequest request, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return Result<GameDto>.Failure(GameErrors.InvalidId);

        var game = await repository.GetByIdAsync(id, cancellationToken);
        if (game is null)
            return Result<GameDto>.Failure(GameErrors.NotFound);

        var parsed = ParseValueObjects(request.Title, request.Platform, request.Genre, request.ReleaseYear, request.Rating);
        if (parsed.IsFailure)
            return Result<GameDto>.Failure(parsed.Error);

        var (title, platform, genre, releaseYear, rating) = parsed.Value;

        var updateResult = game.UpdateDetails(title, platform, genre, releaseYear, rating, request.Description);
        if (updateResult.IsFailure)
            return Result<GameDto>.Failure(updateResult.Error);

        await repository.UpdateAsync(game, cancellationToken);

        return Result<GameDto>.Success(GameDto.FromDomain(game));
    }

    public async Task<Result> DeleteGameAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return Result.Failure(GameErrors.InvalidId);

        var game = await repository.GetByIdAsync(id, cancellationToken);
        if (game is null)
            return Result.Failure(GameErrors.NotFound);

        await repository.DeleteAsync(game, cancellationToken);
        return Result.Success();
    }

    public CatalogueMetadataDto GetMetadata() => new(
        PredefinedPlatforms,
        PredefinedGenres,
        PredefinedRatings);

    private static Result<(GameTitle Title, Platform Platform, Genre Genre, ReleaseYear Year, Rating Rating)> ParseValueObjects(
        string? rawTitle,
        string? rawPlatform,
        string? rawGenre,
        int rawYear,
        string? rawRating)
    {
        var titleResult = GameTitle.Create(rawTitle);
        if (titleResult.IsFailure) return Result<(GameTitle, Platform, Genre, ReleaseYear, Rating)>.Failure(titleResult.Error);

        var platformResult = Platform.Create(rawPlatform);
        if (platformResult.IsFailure) return Result<(GameTitle, Platform, Genre, ReleaseYear, Rating)>.Failure(platformResult.Error);

        var genreResult = Genre.Create(rawGenre);
        if (genreResult.IsFailure) return Result<(GameTitle, Platform, Genre, ReleaseYear, Rating)>.Failure(genreResult.Error);

        var yearResult = ReleaseYear.Create(rawYear);
        if (yearResult.IsFailure) return Result<(GameTitle, Platform, Genre, ReleaseYear, Rating)>.Failure(yearResult.Error);

        var ratingResult = Rating.Create(rawRating);
        if (ratingResult.IsFailure) return Result<(GameTitle, Platform, Genre, ReleaseYear, Rating)>.Failure(ratingResult.Error);

        return Result<(GameTitle, Platform, Genre, ReleaseYear, Rating)>.Success(
            (titleResult.Value, platformResult.Value, genreResult.Value, yearResult.Value, ratingResult.Value));
    }
}
