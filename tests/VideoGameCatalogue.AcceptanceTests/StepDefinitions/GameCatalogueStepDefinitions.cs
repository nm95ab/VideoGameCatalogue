using FluentAssertions;
using Reqnroll;
using VideoGameCatalogue.AcceptanceTests.Drivers;
using VideoGameCatalogue.Application.Games.DTOs;
using VideoGameCatalogue.Domain.Games.ValueObjects;

namespace VideoGameCatalogue.AcceptanceTests.StepDefinitions;

[Binding]
public class GameCatalogueStepDefinitions(VideoGameApiDriver driver)
{
    private CreateGameRequest? _pendingCreateRequest;
    private Guid? _existingGameId;
    private GameDto? _existingGame;
    private GameDto? _lastCreatedGame;
    private GameDto? _lastUpdatedGame;

    [Given("the catalogue is initialized with default seed data")]
    public static void GivenTheCatalogueIsInitializedWithDefaultSeedData()
    {
        // ScenarioHooks automatically resets and seeds the test database before each scenario
    }

    [When("I request all games")]
    public async Task WhenIRequestAllGames()
    {
        await driver.GetAllGamesAsync();
    }

    [When(@"I search games by keyword ""(.*)""")]
    public async Task WhenISearchGamesByKeyword(string keyword)
    {
        await driver.GetAllGamesAsync(search: keyword);
    }

    [When(@"I filter games by platform ""(.*)""")]
    public async Task WhenIFilterGamesByPlatform(string platform)
    {
        await driver.GetAllGamesAsync(platform: platform);
    }

    [When(@"I filter games by genre ""(.*)""")]
    public async Task WhenIFilterGamesByGenre(string genre)
    {
        await driver.GetAllGamesAsync(genre: genre);
    }

    [When(@"I filter games by era ""(.*)""")]
    public async Task WhenIFilterGamesByEra(string era)
    {
        await driver.GetAllGamesAsync(era: era);
    }


    [When(@"I search games with keyword ""(.*)"", platform ""(.*)"", and genre ""(.*)""")]
    public async Task WhenISearchGamesWithCombinedCriteria(string search, string platform, string genre)
    {
        await driver.GetAllGamesAsync(search: search, platform: platform, genre: genre);
    }

    [Then(@"the response status code should be (.*)")]
    public void ThenTheResponseStatusCodeShouldBe(int expectedStatus)
    {
        ((int)driver.LastStatusCode).Should().Be(expectedStatus);
    }

    [Then(@"the catalogue should contain at least (.*) games")]
    public async Task ThenTheCatalogueShouldContainAtLeastGames(int count)
    {
        var games = await driver.ReadGamesListAsync();
        games.Count.Should().BeGreaterThanOrEqualTo(count);
    }

    [Then("each game in the catalogue should have a title, platform, genre, and release year")]
    public async Task ThenEachGameShouldHaveRequiredFields()
    {
        var games = await driver.ReadGamesListAsync();
        var currentYear = DateTime.UtcNow.Year;
        games.Should().AllSatisfy(g =>
        {
            g.Title.Should().NotBeNullOrWhiteSpace();
            g.Platform.Should().NotBeNullOrWhiteSpace();
            g.Genre.Should().NotBeNullOrWhiteSpace();
            g.ReleaseYear.Should().BeInRange(1950, currentYear);
        });
    }

    [Then(@"the results should include a game with title ""(.*)""")]
    public async Task ThenTheResultsShouldIncludeGameWithTitle(string title)
    {
        var games = await driver.ReadGamesListAsync();
        games.Should().Contain(g => g.Title == title);
    }

    [Then(@"the results should not include a game with title ""(.*)""")]
    public async Task ThenTheResultsShouldNotIncludeGameWithTitle(string title)
    {
        var games = await driver.ReadGamesListAsync();
        games.Should().NotContain(g => g.Title == title);
    }

    [Then(@"all returned games should have platform ""(.*)""")]
    public async Task ThenAllReturnedGamesShouldHavePlatform(string platform)
    {
        var games = await driver.ReadGamesListAsync();
        games.Should().NotBeEmpty();
        games.Should().AllSatisfy(g => g.Platform.Should().Be(platform));
    }

    [Then(@"all returned games should have genre ""(.*)""")]
    public async Task ThenAllReturnedGamesShouldHaveGenre(string genre)
    {
        var games = await driver.ReadGamesListAsync();
        games.Should().NotBeEmpty();
        games.Should().AllSatisfy(g => g.Genre.Should().Be(genre));
    }

    [Then(@"all returned games should belong to the (.*) era")]
    public async Task ThenAllReturnedGamesShouldBelongToTheEra(string eraKey)
    {
        var games = await driver.ReadGamesListAsync();
        games.Should().NotBeEmpty();
        var era = GamingEra.FromKey(eraKey);
        era.Should().NotBeNull();
        var maxYear = era!.Value.EndYear ?? DateTime.UtcNow.Year;
        games.Should().AllSatisfy(g =>
        {
            g.ReleaseYear.Should().BeInRange(era.Value.StartYear, maxYear);
            g.Era.Should().NotBeNull();
            g.Era!.Key.Should().Be(era.Value.Key);
        });
    }


    [Then(@"exactly (.*) game(?:s)? should be returned")]
    public async Task ThenExactlyGamesShouldBeReturned(int expectedCount)
    {
        var games = await driver.ReadGamesListAsync();
        games.Count.Should().Be(expectedCount);
    }

    [Then(@"the returned game title should be ""(.*)""")]
    public async Task ThenTheReturnedGameTitleShouldBe(string expectedTitle)
    {
        var games = await driver.ReadGamesListAsync();
        games.Should().ContainSingle();
        games[0].Title.Should().Be(expectedTitle);
    }

    [Then(@"the error code should be ""(.*)""")]
    public async Task ThenTheErrorCodeShouldBe(string expectedCode)
    {
        var problem = await driver.ReadProblemDetailsAsync();
        problem.Should().NotBeNull();
        problem!.ErrorCode.Should().Be(expectedCode);
    }

    [Given("an existing game in the catalogue")]
    public async Task GivenAnExistingGameInTheCatalogue()
    {
        await driver.GetAllGamesAsync();
        var games = await driver.ReadGamesListAsync();
        games.Should().NotBeEmpty();
        _existingGame = games[0];
        _existingGameId = _existingGame.Id;
    }

    [When("I request the game by its ID")]
    public async Task WhenIRequestTheGameByItsId()
    {
        _existingGameId.Should().NotBeNull();
        await driver.GetGameByIdAsync(_existingGameId.ToString()!);
    }

    [When(@"I request a game with ID ""(.*)""")]
    public async Task WhenIRequestAGameWithId(string id)
    {
        await driver.GetGameByIdAsync(id);
    }

    [When(@"I request a game with invalid ID ""(.*)""")]
    public async Task WhenIRequestAGameWithInvalidId(string id)
    {
        await driver.GetGameByIdAsync(id);
    }

    [Then("the returned game should match the requested ID")]
    public async Task ThenTheReturnedGameShouldMatchRequestedId()
    {
        var game = await driver.ReadSingleGameAsync();
        game.Should().NotBeNull();
        game!.Id.Should().Be(_existingGameId!.Value);
    }

    [Then("the returned game should have a non-empty title and platform")]
    public async Task ThenTheReturnedGameShouldHaveNonEmptyTitleAndPlatform()
    {
        var game = await driver.ReadSingleGameAsync();
        game.Should().NotBeNull();
        game!.Title.Should().NotBeNullOrWhiteSpace();
        game.Platform.Should().NotBeNullOrWhiteSpace();
    }

    [When("I delete the game by its ID")]
    public async Task WhenIDeleteTheGameByItsId()
    {
        _existingGameId.Should().NotBeNull();
        await driver.DeleteGameAsync(_existingGameId.ToString()!);
    }

    [When(@"I delete a game with ID ""(.*)""")]
    public async Task WhenIDeleteAGameWithId(string id)
    {
        await driver.DeleteGameAsync(id);
    }

    [Then("the deleted game should no longer exist when fetched by ID")]
    public async Task ThenTheDeletedGameShouldNoLongerExist()
    {
        _existingGameId.Should().NotBeNull();
        await driver.GetGameByIdAsync(_existingGameId.ToString()!);
        ((int)driver.LastStatusCode).Should().Be(404);
    }

    [Given("I have game details:")]
    public void GivenIHaveGameDetails(Table table)
    {
        var dict = table.Rows.ToDictionary(r => r["Field"], r => r["Value"]);
        _pendingCreateRequest = new CreateGameRequest(
            dict.GetValueOrDefault("Title", string.Empty),
            dict.GetValueOrDefault("Platform", string.Empty),
            dict.GetValueOrDefault("Genre", string.Empty),
            int.TryParse(dict.GetValueOrDefault("ReleaseYear", "0"), out var year) ? year : 0,
            dict.GetValueOrDefault("Rating", string.Empty),
            dict.GetValueOrDefault("Description", string.Empty));
    }

    [Given("I have a game with a title that exceeds 150 characters")]
    public void GivenIHaveAGameWithTitleExceeding150Chars()
    {
        _pendingCreateRequest = new CreateGameRequest(
            new string('A', 151),
            "PC",
            "Action",
            2022,
            "Everyone",
            "Long title test");
    }

    [When("I submit the request to create the game")]
    public async Task WhenISubmitRequestToCreateGame()
    {
        _pendingCreateRequest.Should().NotBeNull();
        await driver.CreateGameAsync(_pendingCreateRequest!);
    }

    [Then("the created game should have a valid ID")]
    public async Task ThenTheCreatedGameShouldHaveValidId()
    {
        _lastCreatedGame = await driver.ReadSingleGameAsync();
        _lastCreatedGame.Should().NotBeNull();
        _lastCreatedGame!.Id.Should().NotBeEmpty();
    }

    [Then(@"the created game title should be ""(.*)""")]
    public async Task ThenTheCreatedGameTitleShouldBe(string expectedTitle)
    {
        _lastCreatedGame = await driver.ReadSingleGameAsync();
        _lastCreatedGame.Should().NotBeNull();
        _lastCreatedGame!.Title.Should().Be(expectedTitle);
    }

    [Then("the game should be retrievable by its ID")]
    public async Task ThenTheGameShouldBeRetrievableById()
    {
        _lastCreatedGame.Should().NotBeNull();
        await driver.GetGameByIdAsync(_lastCreatedGame!.Id.ToString());
        ((int)driver.LastStatusCode).Should().Be(200);

        var retrieved = await driver.ReadSingleGameAsync();
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(_lastCreatedGame.Id);
    }

    [When("I update the game with details:")]
    public async Task WhenIUpdateTheGameWithDetails(Table table)
    {
        _existingGameId.Should().NotBeNull();
        var dict = table.Rows.ToDictionary(r => r["Field"], r => r["Value"]);
        var updateRequest = new UpdateGameRequest(
            dict["Title"],
            dict["Platform"],
            dict["Genre"],
            int.Parse(dict["ReleaseYear"]),
            dict["Rating"],
            dict.GetValueOrDefault("Description", string.Empty));

        await driver.UpdateGameAsync(_existingGameId.ToString()!, updateRequest);
    }

    [When(@"I update the game with release year (.*)")]
    public async Task WhenIUpdateGameWithReleaseYear(int year)
    {
        _existingGame.Should().NotBeNull();
        _existingGameId.Should().NotBeNull();
        var updateRequest = new UpdateGameRequest(
            _existingGame!.Title,
            _existingGame.Platform,
            _existingGame.Genre,
            year,
            _existingGame.Rating,
            _existingGame.Description);

        await driver.UpdateGameAsync(_existingGameId.ToString()!, updateRequest);
    }

    [When("I update the game with an empty title")]
    public async Task WhenIUpdateGameWithEmptyTitle()
    {
        _existingGame.Should().NotBeNull();
        _existingGameId.Should().NotBeNull();
        var updateRequest = new UpdateGameRequest(
            string.Empty,
            _existingGame!.Platform,
            _existingGame.Genre,
            _existingGame.ReleaseYear,
            _existingGame.Rating,
            _existingGame.Description);

        await driver.UpdateGameAsync(_existingGameId.ToString()!, updateRequest);
    }

    [When(@"I update a game with ID ""(.*)"" with valid details")]
    public async Task WhenIUpdateGameWithIdWithValidDetails(string id)
    {
        var updateRequest = new UpdateGameRequest(
            "Valid Title",
            "PC",
            "Action",
            2022,
            "Everyone",
            "Valid description");

        await driver.UpdateGameAsync(id, updateRequest);
    }

    [Then(@"the updated game title should be ""(.*)""")]
    public async Task ThenTheUpdatedGameTitleShouldBe(string expectedTitle)
    {
        _lastUpdatedGame = await driver.ReadSingleGameAsync();
        _lastUpdatedGame.Should().NotBeNull();
        _lastUpdatedGame!.Title.Should().Be(expectedTitle);
    }

    [Then(@"the updated game platform should be ""(.*)""")]
    public async Task ThenTheUpdatedGamePlatformShouldBe(string expectedPlatform)
    {
        _lastUpdatedGame = await driver.ReadSingleGameAsync();
        _lastUpdatedGame.Should().NotBeNull();
        _lastUpdatedGame!.Platform.Should().Be(expectedPlatform);
    }

    [Then(@"the updated game release year should be (.*)")]
    public async Task ThenTheUpdatedGameReleaseYearShouldBe(int expectedYear)
    {
        _lastUpdatedGame = await driver.ReadSingleGameAsync();
        _lastUpdatedGame.Should().NotBeNull();
        _lastUpdatedGame!.ReleaseYear.Should().Be(expectedYear);
    }

    [Then("the updated game updatedAtUtc timestamp should be present")]
    public async Task ThenTheUpdatedGameUpdatedAtUtcShouldBePresent()
    {
        _lastUpdatedGame = await driver.ReadSingleGameAsync();
        _lastUpdatedGame.Should().NotBeNull();
        _lastUpdatedGame!.UpdatedAtUtc.Should().NotBeNull();
    }
}
