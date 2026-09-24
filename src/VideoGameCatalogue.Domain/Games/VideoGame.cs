using VideoGameCatalogue.Domain.Common;
using VideoGameCatalogue.Domain.Games.ValueObjects;

namespace VideoGameCatalogue.Domain.Games;

/// <summary>
/// Aggregate Root representing a Video Game in the catalogue.
/// <para>
/// Domain-Driven Design (DDD) Invariants:
/// - Encapsulates all state mutations: properties have private setters to prevent external corruption.
/// - Guarantees valid state at all times: instances can only be created or modified via domain factory/methods
///   (<see cref="Create(Guid, GameTitle, Platform, Genre, ReleaseYear, Rating, string?)"/> and <see cref="UpdateDetails"/>).
/// - Uses strongly typed Value Objects (<see cref="GameTitle"/>, <see cref="Platform"/>, etc.) to eliminate Primitive Obsession.
/// </para>
/// </summary>
public class VideoGame
{
    public Guid Id { get; private set; }
    public GameTitle Title { get; private set; }
    public Platform Platform { get; private set; }
    public Genre Genre { get; private set; }
    public ReleaseYear ReleaseYear { get; private set; }
    public Rating Rating { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? ImageId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Parameterless constructor required by EF Core reflection for entity materialization.
    /// Kept private to prevent instantiation outside of domain factory methods.
    /// </summary>
    private VideoGame()
    {
    }

    private VideoGame(
        Guid id,
        GameTitle title,
        Platform platform,
        Genre genre,
        ReleaseYear releaseYear,
        Rating rating,
        string description,
        DateTime createdAtUtc,
        string? imageId = null)
    {
        Id = id;
        Title = title;
        Platform = platform;
        Genre = genre;
        ReleaseYear = releaseYear;
        Rating = rating;
        Description = description.Trim();
        CreatedAtUtc = createdAtUtc;
        ImageId = imageId;
    }

    /// <summary>
    /// Factory method to create a new <see cref="VideoGame"/> with a generated identity.
    /// </summary>
    public static Result<VideoGame> Create(
        GameTitle title,
        Platform platform,
        Genre genre,
        ReleaseYear releaseYear,
        Rating rating,
        string? description = null,
        DateTime? createdAtUtc = null,
        string? imageId = null)
    {
        return Create(Guid.NewGuid(), title, platform, genre, releaseYear, rating, description, createdAtUtc, imageId);
    }

    /// <summary>
    /// Factory method to reconstitute or create a <see cref="VideoGame"/> with an explicit identity.
    /// Enforces defensive invariants (e.g. non-empty ID) and returns a Railway-Oriented <see cref="Result{VideoGame}"/>
    /// to avoid throwing exceptions for validation failures.
    /// </summary>
    public static Result<VideoGame> Create(
        Guid id,
        GameTitle title,
        Platform platform,
        Genre genre,
        ReleaseYear releaseYear,
        Rating rating,
        string? description = null,
        DateTime? createdAtUtc = null,
        string? imageId = null)
    {
        if (id == Guid.Empty)
            return Result<VideoGame>.Failure(GameErrors.InvalidId);

        if (!IsValidImageId(imageId))
            return Result<VideoGame>.Failure(GameErrors.InvalidImageId);

        var timestamp = createdAtUtc ?? TimeProvider.System.GetUtcNow().UtcDateTime;

        var game = new VideoGame(
            id,
            title,
            platform,
            genre,
            releaseYear,
            rating,
            description ?? string.Empty,
            timestamp,
            imageId?.Trim());

        return Result<VideoGame>.Success(game);
    }

    /// <summary>
    /// Updates game details while preserving domain invariants and recording the update timestamp in UTC.
    /// </summary>
    public Result UpdateDetails(
        GameTitle title,
        Platform platform,
        Genre genre,
        ReleaseYear releaseYear,
        Rating rating,
        string? description = null,
        DateTime? updatedAtUtc = null,
        string? imageId = null)
    {
        if (!IsValidImageId(imageId))
            return Result.Failure(GameErrors.InvalidImageId);

        Title = title;
        Platform = platform;
        Genre = genre;
        ReleaseYear = releaseYear;
        Rating = rating;
        Description = (description ?? string.Empty).Trim();
        UpdatedAtUtc = updatedAtUtc ?? TimeProvider.System.GetUtcNow().UtcDateTime;
        ImageId = imageId?.Trim();

        return Result.Success();
    }

    /// <summary>
    /// Updates the image identifier associated with this game.
    /// </summary>
    public Result SetImage(string? imageId)
    {
        if (!IsValidImageId(imageId))
            return Result.Failure(GameErrors.InvalidImageId);

        ImageId = imageId?.Trim();
        UpdatedAtUtc = TimeProvider.System.GetUtcNow().UtcDateTime;
        return Result.Success();
    }

    /// <summary>
    /// Removes the image association from this game.
    /// </summary>
    public void RemoveImage()
    {
        ImageId = null;
        UpdatedAtUtc = TimeProvider.System.GetUtcNow().UtcDateTime;
    }

    private static bool IsValidImageId(string? imageId)
    {
        if (string.IsNullOrWhiteSpace(imageId))
            return true;

        return !imageId.Contains('/') &&
               !imageId.Contains('\\') &&
               !imageId.Contains("..");
    }
}
