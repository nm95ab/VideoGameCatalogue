namespace VideoGameCatalogue.Domain.Lookups;

/// <summary>
/// Lookup entity representing an authorized content rating (e.g. ESRB) in the catalogue.
/// </summary>
public class RatingLookup : LookupEntity
{
    private RatingLookup()
    {
    }

    public RatingLookup(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public RatingLookup(string name)
    {
        Name = name;
    }
}
