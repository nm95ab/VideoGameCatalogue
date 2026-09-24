namespace VideoGameCatalogue.Domain.Lookups;

/// <summary>
/// Lookup entity representing an authorized video game genre in the catalogue.
/// </summary>
public class GenreLookup : LookupEntity
{
    private GenreLookup()
    {
    }

    public GenreLookup(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public GenreLookup(string name)
    {
        Name = name;
    }
}
