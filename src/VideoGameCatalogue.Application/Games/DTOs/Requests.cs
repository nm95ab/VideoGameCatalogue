namespace VideoGameCatalogue.Application.Games.DTOs;

public record CreateGameRequest(
    string Title,
    string Platform,
    string Genre,
    int ReleaseYear,
    string Rating,
    string? Description,
    string? ImageId = null);

public record UpdateGameRequest(
    string Title,
    string Platform,
    string Genre,
    int ReleaseYear,
    string Rating,
    string? Description,
    string? ImageId = null);

public record CatalogueMetadataDto(
    IReadOnlyList<string> Platforms,
    IReadOnlyList<string> Genres,
    IReadOnlyList<string> Ratings,
    IReadOnlyList<EraDto>? Eras = null);
