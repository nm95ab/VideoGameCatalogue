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
        DateTime createdAtUtc)
    {
        Id = id;
        Title = title;
        Platform = platform;
        Genre = genre;
        ReleaseYear = releaseYear;
        Rating = rating;
        Description = description.Trim();
        CreatedAtUtc = createdAtUtc;
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
        string? description = null)
    {
        return Create(Guid.NewGuid(), title, platform, genre, releaseYear, rating, description);
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
        string? description = null)
    {
        if (id == Guid.Empty)
            return Result<VideoGame>.Failure(GameErrors.InvalidId);

        var game = new VideoGame(
            id,
            title,
            platform,
            genre,
            releaseYear,
            rating,
            description ?? string.Empty,
            DateTime.UtcNow);

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
        string? description = null)
    {
        Title = title;
        Platform = platform;
        Genre = genre;
        ReleaseYear = releaseYear;
        Rating = rating;
        Description = (description ?? string.Empty).Trim();
        UpdatedAtUtc = DateTime.UtcNow;

        return Result.Success();
    }
}
