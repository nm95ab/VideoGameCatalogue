using FluentAssertions;
using VideoGameCatalogue.Domain.Games;
using VideoGameCatalogue.Domain.Games.ValueObjects;
using Xunit;

namespace VideoGameCatalogue.UnitTests.Domain;

public class VideoGameTests
{
    private static GameTitle ValidTitle => GameTitle.Create("Super Mario 64").Value;
    private static Platform ValidPlatform => Platform.Create("Nintendo 64").Value;
    private static Genre ValidGenre => Genre.Create("Platformer").Value;
    private static ReleaseYear ValidReleaseYear => ReleaseYear.Create(1996).Value;
    private static Rating ValidRating => Rating.Create("Everyone").Value;

    [Fact]
    public void Create_WithValidParameters_ShouldSucceedAndInitializeState()
    {
        // Act
        var result = VideoGame.Create(
            ValidTitle,
            ValidPlatform,
            ValidGenre,
            ValidReleaseYear,
            ValidRating,
            "Seminal 3D platformer featuring Mario.");

        // Assert
        result.IsSuccess.Should().BeTrue();
        var game = result.Value;
        game.Id.Should().NotBeEmpty();
        game.Title.Value.Should().Be("Super Mario 64");
        game.Platform.Value.Should().Be("Nintendo 64");
        game.Genre.Value.Should().Be("Platformer");
        game.ReleaseYear.Value.Should().Be(1996);
        game.Rating.Value.Should().Be("Everyone");
        game.Description.Should().Be("Seminal 3D platformer featuring Mario.");
        game.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        game.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Create_WithSpecificId_ShouldRetainId()
    {
        // Arrange
        var explicitId = Guid.NewGuid();

        // Act
        var result = VideoGame.Create(
            explicitId,
            ValidTitle,
            ValidPlatform,
            ValidGenre,
            ValidReleaseYear,
            ValidRating,
            "Description");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(explicitId);
    }

    [Fact]
    public void Create_WithEmptyId_ShouldFail()
    {
        // Act
        var result = VideoGame.Create(
            Guid.Empty,
            ValidTitle,
            ValidPlatform,
            ValidGenre,
            ValidReleaseYear,
            ValidRating,
            "Description");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("VideoGame.InvalidId");
    }

    [Fact]
    public void UpdateDetails_WithValidValues_ShouldUpdateAndSetUpdatedAt()
    {
        // Arrange
        var game = VideoGame.Create(
            ValidTitle,
            ValidPlatform,
            ValidGenre,
            ValidReleaseYear,
            ValidRating,
            "Old description").Value;

        var newTitle = GameTitle.Create("Super Mario 64 Remastered").Value;
        var newPlatform = Platform.Create("Nintendo Switch").Value;
        var newGenre = Genre.Create("Action-Adventure").Value;
        var newYear = ReleaseYear.Create(2020).Value;
        var newRating = Rating.Create("Everyone 10+").Value;

        // Act
        var result = game.UpdateDetails(newTitle, newPlatform, newGenre, newYear, newRating, "Updated description");

        // Assert
        result.IsSuccess.Should().BeTrue();
        game.Title.Should().Be(newTitle);
        game.Platform.Should().Be(newPlatform);
        game.Genre.Should().Be(newGenre);
        game.ReleaseYear.Should().Be(newYear);
        game.Rating.Should().Be(newRating);
        game.Description.Should().Be("Updated description");
        game.UpdatedAtUtc.Should().NotBeNull();
        game.UpdatedAtUtc!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
