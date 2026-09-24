using VideoGameCatalogue.Domain.Common;

namespace VideoGameCatalogue.Domain.Games.ValueObjects;

public readonly record struct GameTitle
{
    public const int MaxLength = 150;
    public string Value { get; }

    private GameTitle(string value) => Value = value;

    public static Result<GameTitle> Create(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result<GameTitle>.Failure(Error.Validation("GameTitle.Empty", "Game title cannot be empty."));

        var trimmed = title.Trim();
        if (trimmed.Length > MaxLength)
            return Result<GameTitle>.Failure(Error.Validation("GameTitle.TooLong", $"Game title cannot exceed {MaxLength} characters."));

        return Result<GameTitle>.Success(new GameTitle(trimmed));
    }

    public override string ToString() => Value;
}
