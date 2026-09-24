using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VideoGameCatalogue.Application.Games;
using VideoGameCatalogue.Application.Games.DTOs;
using VideoGameCatalogue.Domain.Common;

namespace VideoGameCatalogue.Api.Endpoints;

public static class VideoGameEndpoints
{
    public static IEndpointRouteBuilder MapVideoGameEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/games")
            .WithTags("VideoGames");

        group.MapGet("/", GetGames);
        group.MapGet("/metadata", GetMetadata);
        group.MapGet("/{id:guid}", GetGameById);
        group.MapPost("/", CreateGame);
        group.MapPut("/{id:guid}", UpdateGame);
        group.MapDelete("/{id:guid}", DeleteGame);

        return app;
    }

    private static async Task<Ok<IReadOnlyList<GameDto>>> GetGames(
        [FromServices] IVideoGameService service,
        [FromQuery] string? search,
        [FromQuery] string? platform,
        [FromQuery] string? genre,
        CancellationToken ct)
    {
        var games = await service.GetAllGamesAsync(search, platform, genre, ct);
        return TypedResults.Ok(games);
    }

    private static Ok<CatalogueMetadataDto> GetMetadata(
        [FromServices] IVideoGameService service)
    {
        return TypedResults.Ok(service.GetMetadata());
    }

    private static async Task<IResult> GetGameById(
        [FromServices] IVideoGameService service,
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var result = await service.GetGameByIdAsync(id, ct);
        if (result.IsFailure)
            return ToProblemResult(result.Error);

        return TypedResults.Ok(result.Value);
    }

    private static async Task<IResult> CreateGame(
        [FromServices] IVideoGameService service,
        [FromBody] CreateGameRequest request,
        CancellationToken ct)
    {
        var result = await service.CreateGameAsync(request, ct);
        if (result.IsFailure)
            return ToProblemResult(result.Error);

        return TypedResults.Created($"/api/games/{result.Value.Id}", result.Value);
    }

    private static async Task<IResult> UpdateGame(
        [FromServices] IVideoGameService service,
        [FromRoute] Guid id,
        [FromBody] UpdateGameRequest request,
        CancellationToken ct)
    {
        var result = await service.UpdateGameAsync(id, request, ct);
        if (result.IsFailure)
            return ToProblemResult(result.Error);

        return TypedResults.Ok(result.Value);
    }

    private static async Task<IResult> DeleteGame(
        [FromServices] IVideoGameService service,
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var result = await service.DeleteGameAsync(id, ct);
        if (result.IsFailure)
            return ToProblemResult(result.Error);

        return TypedResults.NoContent();
    }

    private static IResult ToProblemResult(Error error)
    {
        if (error.Code.EndsWith(".NotFound", StringComparison.OrdinalIgnoreCase))
        {
            return TypedResults.NotFound(new ProblemDetails
            {
                Title = "Resource Not Found",
                Detail = error.Description,
                Status = StatusCodes.Status404NotFound
            });
        }

        return TypedResults.BadRequest(new ProblemDetails
        {
            Title = "Validation Error",
            Detail = error.Description,
            Status = StatusCodes.Status400BadRequest,
            Extensions = { ["errorCode"] = error.Code }
        });
    }
}
