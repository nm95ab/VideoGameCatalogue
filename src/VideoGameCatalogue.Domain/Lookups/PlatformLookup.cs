namespace VideoGameCatalogue.Domain.Lookups;

/// <summary>
/// Lookup entity representing an authorized gaming platform in the catalogue.
/// </summary>
public class PlatformLookup : LookupEntity
{
    private PlatformLookup()
    {
    }

    public PlatformLookup(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public PlatformLookup(string name)
    {
        Name = name;
    }
}
