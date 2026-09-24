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
    private readonly ILookupRepository _lookupRepository = Substitute.For<ILookupRepository>();
    private readonly VideoGameService _service;

    public VideoGameServiceTests()
    {
        _service = new VideoGameService(_repository, _lookupRepository);
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
        _repository.GetAllAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(games);

        // Act
        var result = await _service.GetAllGamesAsync(cancellationToken: CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Select(x => x.Title).Should().Contain(["Super Mario World", "Chrono Trigger"]);
    }

    [Fact]
    public async Task GetAllGamesAsync_WithEraFilter_ShouldPassEraToRepository()
    {
        // Arrange
        _repository.GetAllAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            "4th-gen",
            Arg.Any<CancellationToken>())
            .Returns([CreateSampleGame("Super Mario World", 1990)]);

        // Act
        var result = await _service.GetAllGamesAsync(era: "4th-gen", cancellationToken: CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        await _repository.Received(1).GetAllAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            "4th-gen",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetGamesAsync_WithEraFilter_ShouldPassEraToRepository()
    {
        // Arrange
        var pagedResult = new PagedResult<VideoGame>([CreateSampleGame("Super Mario World", 1990)], 1, 10, 1);
        _repository.GetPagedAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            "4th-gen",
            1,
            10,
            Arg.Any<CancellationToken>()).Returns(pagedResult);

        // Act
        var result = await _service.GetGamesAsync(era: "4th-gen", pageNumber: 1, pageSize: 10, cancellationToken: CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(1);
        await _repository.Received(1).GetPagedAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            "4th-gen",
            1,
            10,
            Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task GetGamesAsync_WithPagination_ShouldReturnMappedPagedResult()
    {
        // Arrange
        var games = new List<VideoGame>
        {
            CreateSampleGame("Super Mario World", 1990),
            CreateSampleGame("Chrono Trigger", 1995)
        };
        var pagedResult = new PagedResult<VideoGame>(games, 1, 10, 2);
        _repository.GetPagedAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>()).Returns(pagedResult);

        // Act
        var result = await _service.GetGamesAsync(pageNumber: 1, pageSize: 10, cancellationToken: CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Items.Should().HaveCount(2);
        result.Items.Select(x => x.Title).Should().Contain(["Super Mario World", "Chrono Trigger"]);
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
    public async Task GetMetadataAsync_ShouldReturnPlatformsGenresAndRatingsFromLookupRepository()
    {
        // Arrange
        _lookupRepository.GetPlatformsAsync(Arg.Any<CancellationToken>())
            .Returns(["PC", "PlayStation 5"]);
        _lookupRepository.GetGenresAsync(Arg.Any<CancellationToken>())
            .Returns(["Action", "Role-Playing (RPG)"]);
        _lookupRepository.GetRatingsAsync(Arg.Any<CancellationToken>())
            .Returns(["Everyone", "Mature 17+"]);

        // Act
        var metadata = await _service.GetMetadataAsync(CancellationToken.None);

        // Assert
        metadata.Platforms.Should().Equal("PC", "PlayStation 5");
        metadata.Genres.Should().Equal("Action", "Role-Playing (RPG)");
        metadata.Ratings.Should().Equal("Everyone", "Mature 17+");
        metadata.Eras.Should().NotBeNull();
        metadata.Eras!.Should().HaveCount(GamingEra.All.Count);
        metadata.Eras!.Select(e => e.Key).Should().Equal(GamingEra.All.Select(e => e.Key));
        metadata.Eras!.Select(e => e.Generation).Should().Equal(GamingEra.All.Select(e => e.Generation));
        metadata.Eras!.Select(e => e.DisplayTitle).Should().Equal(GamingEra.All.Select(e => e.DisplayTitle));
    }

    [Fact]
    public void EraDto_Properties_ShouldBeRetained()
    {
        var dto = new EraDto("key", "gen", "name", "display", "icon", "badge", "desc", 1990, 1995);
        dto.Key.Should().Be("key");
        dto.Generation.Should().Be("gen");
        dto.Name.Should().Be("name");
        dto.DisplayTitle.Should().Be("display");
        dto.Icon.Should().Be("icon");
        dto.BadgeClass.Should().Be("badge");
        dto.Description.Should().Be("desc");
        dto.StartYear.Should().Be(1990);
        dto.EndYear.Should().Be(1995);
    }


    [Fact]
    public async Task CreateGameAsync_WithImageId_ShouldSetImageIdAndComputeImageUrl()
    {
        // Arrange
        const string imageId = "chrono-art.webp";
        var request = new CreateGameRequest("Chrono", "SNES", "RPG", 1995, "Everyone", "Desc", imageId);

        // Act
        var result = await _service.CreateGameAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ImageId.Should().Be(imageId);
        result.Value.ImageUrl.Should().Be($"/api/images/{imageId}");
    }

    [Fact]
    public async Task UpdateGameAsync_WhenImageReplaced_ShouldDeleteOldImageFromStorage()
    {
        // Arrange
        var mockStorage = Substitute.For<IImageStoragePort>();
        var serviceWithStorage = new VideoGameService(_repository, _lookupRepository, imageStorage: mockStorage);

        var existingGame = VideoGame.Create(
            GameTitle.Create("Zelda").Value,
            Platform.Create("Nintendo Switch").Value,
            Genre.Create("Action-Adventure").Value,
            ReleaseYear.Create(2017).Value,
            Rating.Create("Everyone 10+").Value,
            "Desc",
            imageId: "old-zelda.webp").Value;

        _repository.GetByIdAsync(existingGame.Id, Arg.Any<CancellationToken>()).Returns(existingGame);

        var request = new UpdateGameRequest("Zelda", "Nintendo Switch", "Action-Adventure", 2017, "Everyone 10+", "Desc", "new-zelda.webp");

        // Act
        var result = await serviceWithStorage.UpdateGameAsync(existingGame.Id, request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ImageId.Should().Be("new-zelda.webp");
        await mockStorage.Received(1).DeleteImageAsync("old-zelda.webp", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteGameAsync_WhenGameHasImage_ShouldDeleteImageFromStorage()
    {
        // Arrange
        var mockStorage = Substitute.For<IImageStoragePort>();
        var serviceWithStorage = new VideoGameService(_repository, _lookupRepository, imageStorage: mockStorage);

        var existingGame = VideoGame.Create(
            GameTitle.Create("Zelda").Value,
            Platform.Create("Nintendo Switch").Value,
            Genre.Create("Action-Adventure").Value,
            ReleaseYear.Create(2017).Value,
            Rating.Create("Everyone 10+").Value,
            "Desc",
            imageId: "zelda-to-delete.webp").Value;

        _repository.GetByIdAsync(existingGame.Id, Arg.Any<CancellationToken>()).Returns(existingGame);

        // Act
        var result = await serviceWithStorage.DeleteGameAsync(existingGame.Id, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await mockStorage.Received(1).DeleteImageAsync("zelda-to-delete.webp", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetGameByIdAsync_WhenGameHasImageAndStorageConfigured_ShouldPopulateResolvedImageUrl()
    {
        // Arrange
        var mockStorage = Substitute.For<IImageStoragePort>();
        mockStorage.GetImageUrl("zelda.webp").Returns("https://cdn.videogamecatalogue.com/thumbnails/zelda.webp");
        var serviceWithStorage = new VideoGameService(_repository, _lookupRepository, imageStorage: mockStorage);

        var existingGame = VideoGame.Create(
            GameTitle.Create("Zelda").Value,
            Platform.Create("Nintendo Switch").Value,
            Genre.Create("Action-Adventure").Value,
            ReleaseYear.Create(2017).Value,
            Rating.Create("Everyone 10+").Value,
            "Desc",
            imageId: "zelda.webp").Value;

        _repository.GetByIdAsync(existingGame.Id, Arg.Any<CancellationToken>()).Returns(existingGame);

        // Act
        var result = await serviceWithStorage.GetGameByIdAsync(existingGame.Id, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ImageUrl.Should().Be("https://cdn.videogamecatalogue.com/thumbnails/zelda.webp");
    }
}
