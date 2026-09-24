namespace VideoGameCatalogue.Domain.Lookups;

/// <summary>
/// Abstract base class for reference/lookup entities providing an identifier and display name.
/// </summary>
public abstract class LookupEntity
{
    public int Id { get; protected set; }
    public string Name { get; protected set; } = string.Empty;
}
