using VideoGameCatalogue.Domain.Common;

namespace VideoGameCatalogue.Domain.Games.ValueObjects;

public readonly record struct Rating
{
    public const int MaxLength = 30;
    public string Value { get; }

    private Rating(string value) => Value = value;

    public static Result<Rating> Create(string? rating)
    {
        if (string.IsNullOrWhiteSpace(rating))
            return Result<Rating>.Failure(Error.Validation("Rating.Empty", "Rating cannot be empty."));

        var trimmed = rating.Trim();
        if (trimmed.Length > MaxLength)
            return Result<Rating>.Failure(Error.Validation("Rating.TooLong", $"Rating cannot exceed {MaxLength} characters."));

        return Result<Rating>.Success(new Rating(trimmed));
    }

    public override string ToString() => Value;
}
