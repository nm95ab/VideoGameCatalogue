using FluentAssertions;
using VideoGameCatalogue.Domain.Common;
using VideoGameCatalogue.Domain.Games.ValueObjects;
using Xunit;

namespace VideoGameCatalogue.UnitTests.Domain;

public class ValueObjectTests
{
    [Theory]
    [InlineData("Super Mario 64")]
    [InlineData("The Legend of Zelda: Ocarina of Time")]
    public void GameTitle_Create_WithValidTitle_ShouldSucceed(string title)
    {
        // Act
        var result = GameTitle.Create(title);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(title.Trim());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GameTitle_Create_WithEmptyTitle_ShouldFail(string? title)
    {
        // Act
        var result = GameTitle.Create(title!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("GameTitle.Empty");
    }

    [Fact]
    public void GameTitle_Create_WhenExceeds150Chars_ShouldFail()
    {
        // Arrange
        var longTitle = new string('A', 151);

        // Act
        var result = GameTitle.Create(longTitle);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("GameTitle.TooLong");
    }

    [Theory]
    [InlineData(1950)]
    [InlineData(1995)]
    [InlineData(2025)]
    public void ReleaseYear_Create_WithValidYear_ShouldSucceed(int year)
    {
        // Act
        var result = ReleaseYear.Create(year);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(year);
    }

    [Theory]
    [InlineData(1949)]
    [InlineData(2050)]
    public void ReleaseYear_Create_WithInvalidYear_ShouldFail(int year)
    {
        // Act
        var result = ReleaseYear.Create(year);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ReleaseYear.Invalid");
    }

    [Fact]
    public void ReleaseYear_Create_WithExplicitCurrentYear_ShouldUseSpecifiedBase()
    {
        // Act: base year 2020 allows up to 2020; 2021 should fail
        var validResult = ReleaseYear.Create(2020, 2020);
        var invalidResult = ReleaseYear.Create(2021, 2020);

        // Assert
        validResult.IsSuccess.Should().BeTrue();
        invalidResult.IsFailure.Should().BeTrue();
        invalidResult.Error.Code.Should().Be("ReleaseYear.Invalid");
    }

    [Fact]
    public void ReleaseYear_Create_WithFutureYearBeyondCurrentYear_ShouldFail()
    {
        // Arrange
        var currentYear = TimeProvider.System.GetUtcNow().Year;

        // Act
        var result = ReleaseYear.Create(currentYear + 1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ReleaseYear.Invalid");
        result.Error.Description.Should().Contain(currentYear.ToString());
    }

    [Theory]
    [InlineData("PC")]
    [InlineData("PlayStation 5")]
    [InlineData("Xbox Series X")]
    [InlineData("Nintendo Switch")]
    public void Platform_Create_WithValidPlatform_ShouldSucceed(string platform)
    {
        // Act
        var result = Platform.Create(platform);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(platform.Trim());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Platform_Create_WithEmptyPlatform_ShouldFail(string? platform)
    {
        // Act
        var result = Platform.Create(platform!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Platform.Empty");
    }

    [Theory]
    [InlineData("Action")]
    [InlineData("Role-Playing (RPG)")]
    [InlineData("Adventure")]
    public void Genre_Create_WithValidGenre_ShouldSucceed(string genre)
    {
        // Act
        var result = Genre.Create(genre);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(genre.Trim());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Genre_Create_WithEmptyGenre_ShouldFail(string? genre)
    {
        // Act
        var result = Genre.Create(genre!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Genre.Empty");
    }

    [Theory]
    [InlineData("Everyone")]
    [InlineData("Teen")]
    [InlineData("Mature 17+")]
    public void Rating_Create_WithValidRating_ShouldSucceed(string rating)
    {
        // Act
        var result = Rating.Create(rating);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(rating.Trim());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Rating_Create_WithEmptyRating_ShouldFail(string? rating)
    {
        var result = Rating.Create(rating!);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Rating.Empty");
    }

    [Fact]
    public void Platform_Create_WhenExceeds50Chars_ShouldFail()
    {
        var longPlatform = new string('P', 51);
        var result = Platform.Create(longPlatform);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Platform.TooLong");
    }

    [Fact]
    public void Genre_Create_WhenExceeds50Chars_ShouldFail()
    {
        var longGenre = new string('G', 51);
        var result = Genre.Create(longGenre);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Genre.TooLong");
    }

    [Fact]
    public void Rating_Create_WhenExceeds30Chars_ShouldFail()
    {
        var longRating = new string('R', 31);
        var result = Rating.Create(longRating);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Rating.TooLong");
    }

    [Fact]
    public void ValueObjects_ToString_ShouldReturnUnderlyingStringValue()
    {
        GameTitle.Create("Chrono Trigger").Value.ToString().Should().Be("Chrono Trigger");
        Platform.Create("SNES").Value.ToString().Should().Be("SNES");
        Genre.Create("RPG").Value.ToString().Should().Be("RPG");
        ReleaseYear.Create(1995).Value.ToString().Should().Be("1995");
        Rating.Create("Everyone").Value.ToString().Should().Be("Everyone");
    }
}
