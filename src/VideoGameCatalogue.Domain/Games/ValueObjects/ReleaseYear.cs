using VideoGameCatalogue.Domain.Common;

namespace VideoGameCatalogue.Domain.Games.ValueObjects;

public readonly record struct ReleaseYear
{
    public const int MinYear = 1950;
    public int Value { get; }

    private ReleaseYear(int value) => Value = value;

    public static Result<ReleaseYear> Create(int year, int? currentYear = null)
    {
        var baseYear = currentYear ?? TimeProvider.System.GetUtcNow().Year;
        var maxYear = baseYear + 2;
        if (year < MinYear || year > maxYear)
            return Result<ReleaseYear>.Failure(Error.Validation("ReleaseYear.Invalid", $"Release year must be between {MinYear} and {maxYear}."));

        return Result<ReleaseYear>.Success(new ReleaseYear(year));
    }

    public override string ToString() => Value.ToString();
}
