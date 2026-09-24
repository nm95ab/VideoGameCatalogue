using VideoGameCatalogue.Domain.Common;

namespace VideoGameCatalogue.Domain.Games.ValueObjects;

public readonly record struct Platform
{
    public const int MaxLength = 50;
    public string Value { get; }

    private Platform(string value) => Value = value;

    public static Result<Platform> Create(string? platform)
    {
        if (string.IsNullOrWhiteSpace(platform))
            return Result<Platform>.Failure(Error.Validation("Platform.Empty", "Platform cannot be empty."));

        var trimmed = platform.Trim();
        if (trimmed.Length > MaxLength)
            return Result<Platform>.Failure(Error.Validation("Platform.TooLong", $"Platform cannot exceed {MaxLength} characters."));

        return Result<Platform>.Success(new Platform(trimmed));
    }

    public override string ToString() => Value;
}
