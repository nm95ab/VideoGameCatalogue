namespace VideoGameCatalogue.Application.Games.DTOs;

public record CreateGameRequest(
    string Title,
    string Platform,
    string Genre,
    int ReleaseYear,
    string Rating,
    string? Description);

public record UpdateGameRequest(
    string Title,
    string Platform,
    string Genre,
    int ReleaseYear,
    string Rating,
    string? Description);

public record CatalogueMetadataDto(
    IReadOnlyList<string> Platforms,
    IReadOnlyList<string> Genres,
    IReadOnlyList<string> Ratings);
