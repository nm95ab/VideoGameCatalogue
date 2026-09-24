using VideoGameCatalogue.Domain.Games;
using VideoGameCatalogue.Domain.Ports;

namespace VideoGameCatalogue.Application.Games.DTOs;

public record GameDto(
    Guid Id,
    string Title,
    string Platform,
    string Genre,
    int ReleaseYear,
    string Rating,
    string Description,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    string? ImageId = null,
    string? ImageUrl = null,
    EraDto? Era = null,
    int AgeInYears = 0,
    string Decade = "")
{
    public static GameDto FromDomain(VideoGame game, IImageStoragePort? imageStorage = null)
    {
        string? imageUrl = null;
        if (!string.IsNullOrWhiteSpace(game.ImageId))
        {
            imageUrl = imageStorage is not null
                ? imageStorage.GetImageUrl(game.ImageId)
                : $"/api/images/{game.ImageId}";
        }

        var era = game.ReleaseYear.Era;
        var eraDto = new EraDto(
            era.Key,
            era.Generation,
            era.Name,
            era.DisplayTitle,
            era.Icon,
            era.BadgeClass,
            era.Description,
            era.StartYear,
            era.EndYear);

        return new(
            game.Id,
            game.Title.Value,
            game.Platform.Value,
            game.Genre.Value,
            game.ReleaseYear.Value,
            game.Rating.Value,
            game.Description,
            game.CreatedAtUtc,
            game.UpdatedAtUtc,
            game.ImageId,
            imageUrl,
            eraDto,
            game.ReleaseYear.AgeInYears(),
            game.ReleaseYear.Decade);
    }
}
