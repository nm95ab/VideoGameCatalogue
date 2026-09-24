using FluentAssertions;
using VideoGameCatalogue.Domain.Lookups;

namespace VideoGameCatalogue.UnitTests.Domain;

public sealed class LookupEntityTests
{
    [Fact]
    public void PlatformLookup_Constructors_SetPropertiesCorrectly()
    {
        var lookup1 = new PlatformLookup(1, "PC");
        lookup1.Id.Should().Be(1);
        lookup1.Name.Should().Be("PC");

        var lookup2 = new PlatformLookup("Xbox Series X/S");
        lookup2.Id.Should().Be(0);
        lookup2.Name.Should().Be("Xbox Series X/S");
    }

    [Fact]
    public void GenreLookup_Constructors_SetPropertiesCorrectly()
    {
        var lookup1 = new GenreLookup(2, "Role-Playing (RPG)");
        lookup1.Id.Should().Be(2);
        lookup1.Name.Should().Be("Role-Playing (RPG)");

        var lookup2 = new GenreLookup("Strategy");
        lookup2.Id.Should().Be(0);
        lookup2.Name.Should().Be("Strategy");
    }

    [Fact]
    public void RatingLookup_Constructors_SetPropertiesCorrectly()
    {
        var lookup1 = new RatingLookup(3, "Mature 17+");
        lookup1.Id.Should().Be(3);
        lookup1.Name.Should().Be("Mature 17+");

        var lookup2 = new RatingLookup("Everyone");
        lookup2.Id.Should().Be(0);
        lookup2.Name.Should().Be("Everyone");
    }
}
