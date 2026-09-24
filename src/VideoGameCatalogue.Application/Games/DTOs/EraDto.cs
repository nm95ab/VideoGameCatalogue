namespace VideoGameCatalogue.Application.Games.DTOs;

/// <summary>
/// Data Transfer Object representing a historical console generation and gaming era.
/// </summary>
public record EraDto(
    string Key,
    string Generation,
    string Name,
    string DisplayTitle,
    string Icon,
    string BadgeClass,
    string Description,
    int StartYear,
    int? EndYear);
