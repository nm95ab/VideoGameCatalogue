using FluentAssertions;
using VideoGameCatalogue.Domain.Games.ValueObjects;
using Xunit;

namespace VideoGameCatalogue.UnitTests.Domain;

public class GamingEraTests
{
    [Theory]
    [InlineData(1975, "early-arcade", "Early Era", "Early Arcade & 2nd Gen")]
    [InlineData(1985, "3rd-gen", "3rd Gen", "8-Bit Era")]
    [InlineData(1991, "4th-gen", "4th Gen", "16-Bit Golden Age")]
    [InlineData(1996, "5th-gen", "5th Gen", "3D Revolution")]
    [InlineData(2001, "6th-gen", "6th Gen", "128-Bit Era")]
    [InlineData(2008, "7th-gen", "7th Gen", "HD Generation")]
    [InlineData(2017, "8th-gen", "8th Gen", "Connected Era")]
    [InlineData(2024, "9th-gen", "9th Gen", "Modern Era")]
    public void ForYear_WhenGivenYear_ShouldReturnCorrectEraAndGeneration(
        int year,
        string expectedKey,
        string expectedGeneration,
        string expectedName)
    {
        // Act
        var era = GamingEra.ForYear(year);

        // Assert
        era.Key.Should().Be(expectedKey);
        era.Generation.Should().Be(expectedGeneration);
        era.Name.Should().Be(expectedName);
    }

    [Theory]
    [InlineData("4th-gen")]
    [InlineData("4TH-GEN")]
    [InlineData("16-bit")]
    public void FromKey_WhenGivenValidKeyOrAlias_ShouldReturnEra(string key)
    {
        // Act
        var era = GamingEra.FromKey(key);

        // Assert
        era.Should().NotBeNull();
        era!.Value.Generation.Should().Be("4th Gen");
    }

    [Theory]
    [InlineData("invalid-era")]
    [InlineData("")]
    [InlineData(null)]
    public void FromKey_WhenGivenInvalidKey_ShouldReturnNull(string? key)
    {
        // Act
        var era = GamingEra.FromKey(key);

        // Assert
        era.Should().BeNull();
    }

    [Fact]
    public void All_ShouldContainChronologicallyOrderedErasCovering1950ToPresent()
    {
        // Act
        var all = GamingEra.All;

        // Assert
        all.Should().NotBeEmpty();
        all.First().StartYear.Should().Be(1950);
        all.Last().EndYear.Should().BeNull(); // Ongoing modern era

        // Contiguity check: each era starts when the previous ends + 1
        for (var i = 1; i < all.Count; i++)
        {
            all[i].StartYear.Should().Be(all[i - 1].EndYear!.Value + 1);
        }
    }

    [Fact]
    public void ReleaseYear_ShouldExposeEraDecadeAndAgeProperties()
    {
        // Arrange
        var releaseYear = ReleaseYear.Create(1995).Value;

        // Assert
        releaseYear.Era.Generation.Should().Be("5th Gen");
        releaseYear.Era.Name.Should().Be("3D Revolution");
        releaseYear.Decade.Should().Be("1990s");
    }

    [Theory]
    [InlineData(2004, 2024, 20)]
    [InlineData(2005, 2024, 19)]
    [InlineData(1991, 2024, 33)]
    [InlineData(2024, 2024, 0)]
    public void ReleaseYear_AgeInYears_ShouldComputeAccuratelyWithSpecifiedYear(
        int releaseYearValue,
        int referenceYear,
        int expectedAge)
    {
        // Arrange
        var releaseYear = ReleaseYear.Create(releaseYearValue).Value;

        // Act
        var age = releaseYear.AgeInYears(referenceYear);

        // Assert
        age.Should().Be(expectedAge);
    }
}
