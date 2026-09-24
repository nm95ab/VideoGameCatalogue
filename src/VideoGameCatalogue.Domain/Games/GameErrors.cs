using VideoGameCatalogue.Domain.Common;

namespace VideoGameCatalogue.Domain.Games;

public static class GameErrors
{
    public static readonly Error NotFound = Error.NotFound("VideoGame.NotFound", "The specified video game was not found.");
    public static readonly Error InvalidId = Error.Validation("VideoGame.InvalidId", "The video game ID cannot be empty.");
    public static readonly Error InvalidImageId = Error.Validation("VideoGame.InvalidImageId", "The image ID cannot contain path traversal or directory characters.");
}
