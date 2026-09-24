using FluentAssertions;
using NSubstitute;
using VideoGameCatalogue.Application.Games;
using VideoGameCatalogue.Application.Games.DTOs;
using VideoGameCatalogue.Domain.Common;
using VideoGameCatalogue.Domain.Games;
using VideoGameCatalogue.Domain.Games.ValueObjects;
using VideoGameCatalogue.Domain.Ports;
using Xunit;

namespace VideoGameCatalogue.UnitTests.Application;

public class VideoGameServiceTests
{
    private readonly IVideoGameRepository _repository = Substitute.For<IVideoGameRepository>();
    private readonly VideoGameService _service;

    public VideoGameServiceTests()
    {
        _service = new VideoGameService(_repository);
    }

    private static VideoGame CreateSampleGame(string title = "Chrono Trigger", int year = 1995)
    {
        return VideoGame.Create(
            GameTitle.Create(title).Value,
            Platform.Create("SNES").Value,
            Genre.Create("RPG").Value,
            ReleaseYear.Create(year).Value,
            Rating.Create("Everyone").Value,
            "Classic Square RPG.").Value;
    }

    [Fact]
    public async Task GetAllGamesAsync_WhenGamesExist_ShouldReturnMappedDtos()
    {
        // Arrange
        var games = new List<VideoGame>
        {
            CreateSampleGame("Super Mario World", 1990),
            CreateSampleGame("Chrono Trigger", 1995)
        };
        _repository.GetAllAsync(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(games);

        // Act
        var result = await _service.GetAllGamesAsync(null, null, null, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Select(x => x.Title).Should().Contain(["Super Mario World", "Chrono Trigger"]);
    }

    [Fact]
    public async Task GetGameByIdAsync_WhenGameExists_ShouldReturnSuccessWithDto()
    {
        // Arrange
        var game = CreateSampleGame();
        _repository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);

        // Act
        var result = await _service.GetGameByIdAsync(game.Id, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(game.Id);
        result.Value.Title.Should().Be("Chrono Trigger");
    }

    [Fact]
    public async Task GetGameByIdAsync_WhenGameDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((VideoGame?)null);

        // Act
        var result = await _service.GetGameByIdAsync(id, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("VideoGame.NotFound");
    }

    [Fact]
    public async Task CreateGameAsync_WithValidRequest_ShouldAddAndReturnDto()
    {
        // Arrange
        var request = new CreateGameRequest(
            "Elden Ring",
            "PlayStation 5",
            "Action RPG",
            2022,
            "Mature 17+",
            "FromSoftware masterpiece");

        // Act
        var result = await _service.CreateGameAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("Elden Ring");
        result.Value.Platform.Should().Be("PlayStation 5");
        result.Value.ReleaseYear.Should().Be(2022);
        await _repository.Received(1).AddAsync(Arg.Any<VideoGame>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateGameAsync_WithInvalidTitle_ShouldReturnValidationError()
    {
        // Arrange
        var request = new CreateGameRequest(
            "",
            "PlayStation 5",
            "Action RPG",
            2022,
            "Mature 17+",
            "Description");

        // Act
        var result = await _service.CreateGameAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("GameTitle.Empty");
        await _repository.DidNotReceive().AddAsync(Arg.Any<VideoGame>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateGameAsync_WithInvalidYear_ShouldReturnValidationError()
    {
        // Arrange
        var request = new CreateGameRequest(
            "Retro Game",
            "NES",
            "Action",
            1920,
            "Everyone",
            "Description");

        // Act
        var result = await _service.CreateGameAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ReleaseYear.Invalid");
        await _repository.DidNotReceive().AddAsync(Arg.Any<VideoGame>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateGameAsync_WhenGameExistsAndDataValid_ShouldUpdateAndReturnDto()
    {
        // Arrange
        var game = CreateSampleGame();
        _repository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);

        var request = new UpdateGameRequest(
            "Chrono Trigger: Definitive Edition",
            "PC",
            "JRPG",
            2023,
            "Teen",
            "Updated version.");

        // Act
        var result = await _service.UpdateGameAsync(game.Id, request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("Chrono Trigger: Definitive Edition");
        result.Value.Platform.Should().Be("PC");
        result.Value.ReleaseYear.Should().Be(2023);
        await _repository.Received(1).UpdateAsync(game, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateGameAsync_WhenGameDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((VideoGame?)null);

        var request = new UpdateGameRequest("Title", "PC", "RPG", 2020, "Teen", null);

        // Act
        var result = await _service.UpdateGameAsync(id, request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("VideoGame.NotFound");
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<VideoGame>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteGameAsync_WhenGameExists_ShouldDeleteAndReturnSuccess()
    {
        // Arrange
        var game = CreateSampleGame();
        _repository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);

        // Act
        var result = await _service.DeleteGameAsync(game.Id, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).DeleteAsync(game, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteGameAsync_WhenGameDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((VideoGame?)null);

        // Act
        var result = await _service.DeleteGameAsync(id, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("VideoGame.NotFound");
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<VideoGame>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetGameByIdAsync_WithEmptyId_ShouldReturnInvalidIdError()
    {
        var result = await _service.GetGameByIdAsync(Guid.Empty, CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("VideoGame.InvalidId");
    }

    [Fact]
    public async Task UpdateGameAsync_WithEmptyId_ShouldReturnInvalidIdError()
    {
        var request = new UpdateGameRequest("Title", "PC", "RPG", 2020, "Teen", null);
        var result = await _service.UpdateGameAsync(Guid.Empty, request, CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("VideoGame.InvalidId");
    }

    [Fact]
    public async Task UpdateGameAsync_WithInvalidTitle_ShouldReturnValidationError()
    {
        var game = CreateSampleGame();
        _repository.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);

        var request = new UpdateGameRequest("", "PC", "RPG", 2020, "Teen", null);
        var result = await _service.UpdateGameAsync(game.Id, request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("GameTitle.Empty");
    }

    [Fact]
    public async Task DeleteGameAsync_WithEmptyId_ShouldReturnInvalidIdError()
    {
        var result = await _service.DeleteGameAsync(Guid.Empty, CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("VideoGame.InvalidId");
    }

    [Fact]
    public async Task CreateGameAsync_WithInvalidPlatform_ShouldReturnValidationError()
    {
        var request = new CreateGameRequest("Title", "", "RPG", 2020, "Teen", null);
        var result = await _service.CreateGameAsync(request, CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Platform.Empty");
    }

    [Fact]
    public async Task CreateGameAsync_WithInvalidGenre_ShouldReturnValidationError()
    {
        var request = new CreateGameRequest("Title", "PC", "", 2020, "Teen", null);
        var result = await _service.CreateGameAsync(request, CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Genre.Empty");
    }

    [Fact]
    public async Task CreateGameAsync_WithInvalidRating_ShouldReturnValidationError()
    {
        var request = new CreateGameRequest("Title", "PC", "RPG", 2020, "", null);
        var result = await _service.CreateGameAsync(request, CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Rating.Empty");
    }

    [Fact]
    public void GetMetadata_ShouldReturnPopulatedPlatformsGenresAndRatings()
    {
        // Act
        var metadata = _service.GetMetadata();

        // Assert
        metadata.Platforms.Should().NotBeEmpty();
        metadata.Genres.Should().NotBeEmpty();
        metadata.Ratings.Should().NotBeEmpty();
        metadata.Platforms.Should().Contain("PlayStation 5");
        metadata.Ratings.Should().Contain("Mature 17+");
    }
}
