using VideoGameCatalogue.Domain.Common;
using VideoGameCatalogue.Domain.Games.ValueObjects;

namespace VideoGameCatalogue.Domain.Games;

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

    // Required for EF Core reflection
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
