using System.Text;
using FluentAssertions;
using Reqnroll;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using VideoGameCatalogue.AcceptanceTests.Drivers;
using VideoGameCatalogue.Application.Games.DTOs;

namespace VideoGameCatalogue.AcceptanceTests.StepDefinitions;

[Binding]
public class GameThumbnailStepDefinitions(VideoGameApiDriver driver)
{
    private string? _lastUploadedImageId;

    [When(@"I upload a valid sample image ""(.*)""")]
    public async Task WhenIUploadAValidSampleImage(string fileName)
    {
        var bytes = await CreateSamplePngBytesAsync();
        await driver.UploadImageAsync(bytes, fileName, "image/png");
        var resp = await driver.ReadImageUploadResponseAsync();
        if (resp is not null)
        {
            _lastUploadedImageId = resp.ImageId;
        }
    }

    [Then("the image upload response should contain a valid image ID and URL")]
    public async Task ThenTheImageUploadResponseShouldContainAValidImageIdAndUrl()
    {
        var resp = await driver.ReadImageUploadResponseAsync();
        resp.Should().NotBeNull();
        resp!.ImageId.Should().EndWith(".webp");
        resp.Url.Should().Be($"/api/images/{resp.ImageId}");
        _lastUploadedImageId = resp.ImageId;
    }

    [Given(@"an uploaded image ""(.*)""")]
    public async Task GivenAnUploadedImage(string fileName)
    {
        var bytes = await CreateSamplePngBytesAsync();
        await driver.UploadImageAsync(bytes, fileName, "image/png");
        var resp = await driver.ReadImageUploadResponseAsync();
        resp.Should().NotBeNull();
        _lastUploadedImageId = resp!.ImageId;
    }

    [When("I request the image by its ID")]
    public async Task WhenIRequestTheImageByItsId()
    {
        _lastUploadedImageId.Should().NotBeNull();
        await driver.GetImageDirectAsync(_lastUploadedImageId!);
    }

    [Then(@"the response content type should be ""(.*)""")]
    public void ThenTheResponseContentTypeShouldBe(string expectedType)
    {
        driver.LastResponse.Should().NotBeNull();
        driver.LastResponse!.Content.Headers.ContentType?.MediaType.Should().Be(expectedType);
    }

    [When(@"I upload an invalid image file ""(.*)"" with text content")]
    public async Task WhenIUploadAnInvalidImageFileWithTextContent(string fileName)
    {
        var bytes = Encoding.UTF8.GetBytes("Not a valid image file content.");
        await driver.UploadImageAsync(bytes, fileName, "text/plain");
    }

    [When("I create a game with the uploaded image ID")]
    public async Task WhenICreateAGameWithTheUploadedImageId()
    {
        _lastUploadedImageId.Should().NotBeNull();
        var request = new CreateGameRequest(
            "Game with Thumbnail",
            "PC",
            "Action",
            2023,
            "Everyone",
            "Game description with thumbnail",
            _lastUploadedImageId);

        await driver.CreateGameAsync(request);
    }

    [Then("the created game should reference the image ID")]
    public async Task ThenTheCreatedGameShouldReferenceTheImageId()
    {
        var game = await driver.ReadSingleGameAsync();
        game.Should().NotBeNull();
        game!.ImageId.Should().Be(_lastUploadedImageId);
    }

    [Then("the created game should have an image URL")]
    public async Task ThenTheCreatedGameShouldHaveAnImageUrl()
    {
        var game = await driver.ReadSingleGameAsync();
        game.Should().NotBeNull();
        game!.ImageUrl.Should().Be($"/api/images/{_lastUploadedImageId}");
    }

    [When("I delete the image by its ID")]
    public async Task WhenIDeleteTheImageByItsId()
    {
        _lastUploadedImageId.Should().NotBeNull();
        await driver.DeleteImageDirectAsync(_lastUploadedImageId!);
    }

    [Then("requesting the deleted image should return 404")]
    public async Task ThenRequestingTheDeletedImageShouldReturn404()
    {
        _lastUploadedImageId.Should().NotBeNull();
        await driver.GetImageDirectAsync(_lastUploadedImageId!);
        ((int)driver.LastStatusCode).Should().Be(404);
    }

    private static async Task<byte[]> CreateSamplePngBytesAsync()
    {
        using var img = new Image<Rgba32>(120, 120);
        using var ms = new MemoryStream();
        await img.SaveAsPngAsync(ms);
        return ms.ToArray();
    }
}
