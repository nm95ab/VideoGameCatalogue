using VideoGameCatalogue.Domain.Common;

namespace VideoGameCatalogue.Domain.Games.ValueObjects;

public readonly record struct ReleaseYear
{
    public const int MinYear = 1950;
    public int Value { get; }

    private ReleaseYear(int value) => Value = value;

    public GamingEra Era => GamingEra.ForYear(Value);

    public string Decade => $"{Value / 10 * 10}s";

    public int AgeInYears(int? referenceYear = null)
    {
        var current = referenceYear ?? TimeProvider.System.GetUtcNow().Year;
        return Math.Max(0, current - Value);
    }

    public static Result<ReleaseYear> Create(int year, int? currentYear = null)
    {
        var maxYear = currentYear ?? TimeProvider.System.GetUtcNow().Year;
        if (year < MinYear || year > maxYear)
            return Result<ReleaseYear>.Failure(Error.Validation("ReleaseYear.Invalid", $"Release year must be between {MinYear} and {maxYear}."));

        return Result<ReleaseYear>.Success(new ReleaseYear(year));
    }

    public override string ToString() => Value.ToString();
}
