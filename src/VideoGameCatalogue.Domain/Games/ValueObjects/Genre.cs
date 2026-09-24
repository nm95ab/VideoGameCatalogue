using VideoGameCatalogue.Domain.Common;

namespace VideoGameCatalogue.Domain.Games.ValueObjects;

public readonly record struct Genre
{
    public const int MaxLength = 50;
    public string Value { get; }

    private Genre(string value) => Value = value;

    public static Result<Genre> Create(string? genre)
    {
        if (string.IsNullOrWhiteSpace(genre))
            return Result<Genre>.Failure(Error.Validation("Genre.Empty", "Genre cannot be empty."));

        var trimmed = genre.Trim();
        if (trimmed.Length > MaxLength)
            return Result<Genre>.Failure(Error.Validation("Genre.TooLong", $"Genre cannot exceed {MaxLength} characters."));

        return Result<Genre>.Success(new Genre(trimmed));
    }

    public override string ToString() => Value;
}
