using FluentAssertions;
using Reqnroll;
using VideoGameCatalogue.AcceptanceTests.Drivers;

namespace VideoGameCatalogue.AcceptanceTests.StepDefinitions;

[Binding]
public class MetadataStepDefinitions(VideoGameApiDriver driver)
{
    [When(@"I request the catalogue metadata")]
    public async Task WhenIRequestTheCatalogueMetadata()
    {
        await driver.GetMetadataAsync();
    }

    [Then(@"the metadata should contain platforms including ""(.*)"", ""(.*)"", ""(.*)""")]
    public async Task ThenTheMetadataShouldContainPlatforms(string p1, string p2, string p3)
    {
        var metadata = await driver.ReadMetadataAsync();
        metadata.Should().NotBeNull();
        metadata!.Platforms.Should().Contain([p1, p2, p3]);
    }

    [Then(@"the metadata should contain genres including ""(.*)"", ""(.*)"", ""(.*)""")]
    public async Task ThenTheMetadataShouldContainGenres(string g1, string g2, string g3)
    {
        var metadata = await driver.ReadMetadataAsync();
        metadata.Should().NotBeNull();
        metadata!.Genres.Should().Contain([g1, g2, g3]);
    }

    [Then(@"the metadata should contain ratings including ""(.*)"", ""(.*)"", ""(.*)""")]
    public async Task ThenTheMetadataShouldContainRatings(string r1, string r2, string r3)
    {
        var metadata = await driver.ReadMetadataAsync();
        metadata.Should().NotBeNull();
        metadata!.Ratings.Should().Contain([r1, r2, r3]);
    }
}
