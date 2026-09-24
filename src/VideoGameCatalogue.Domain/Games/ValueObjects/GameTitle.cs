using VideoGameCatalogue.Domain.Common;

namespace VideoGameCatalogue.Domain.Games.ValueObjects;

/// <summary>
/// Value Object encapsulating a game's title and its domain invariants.
/// <para>
/// Implemented as a <c>readonly record struct</c> to:
/// 1. Eliminate Primitive Obsession (a title is not just any raw string).
/// 2. Provide zero-allocation stack semantics and structural value equality.
/// 3. Enforce immutability and self-validation through its factory method.
/// </para>
/// </summary>
public readonly record struct GameTitle
{
    public const int MaxLength = 150;
    public string Value { get; }

    private GameTitle(string value) => Value = value;

    /// <summary>
    /// Factory method to validate and instantiate a <see cref="GameTitle"/>.
    /// Trims input and enforces non-empty and maximum length constraints.
    /// </summary>
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
