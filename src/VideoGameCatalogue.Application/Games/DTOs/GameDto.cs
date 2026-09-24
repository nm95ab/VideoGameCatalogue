using VideoGameCatalogue.Domain.Games;

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
    DateTime? UpdatedAtUtc)
{
    public static GameDto FromDomain(VideoGame game) => new(
        game.Id,
        game.Title.Value,
        game.Platform.Value,
        game.Genre.Value,
        game.ReleaseYear.Value,
        game.Rating.Value,
        game.Description,
        game.CreatedAtUtc,
        game.UpdatedAtUtc);
}
