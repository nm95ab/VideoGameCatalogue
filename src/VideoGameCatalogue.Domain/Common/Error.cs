namespace VideoGameCatalogue.Domain.Common;

public readonly record struct Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static Error Validation(string code, string desc) => new(code, desc);
    public static Error NotFound(string code, string desc) => new(code, desc);
    public static Error Conflict(string code, string desc) => new(code, desc);
}
