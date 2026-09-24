using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VideoGameCatalogue.Application.Games;
using VideoGameCatalogue.Application.Games.DTOs;
using VideoGameCatalogue.Domain.Common;

namespace VideoGameCatalogue.Api.Endpoints;

/// <summary>
/// Inbound (Driving) Adapter exposing RESTful HTTP endpoints for video game catalogue management.
/// <para>
/// Translates incoming HTTP requests and route/query parameters to the <see cref="IVideoGameService"/>
/// driving port, and maps application results to RFC 7807 ProblemDetails or HTTP 200/201/204 responses.
/// </para>
/// </summary>
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

    private static async Task<Ok<PagedResult<GameDto>>> GetGames(
        [FromServices] IVideoGameService service,
        [FromQuery] string? search,
        [FromQuery] string? platform,
        [FromQuery] string? genre,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken ct)
    {
        var result = await service.GetGamesAsync(
            search,
            platform,
            genre,
            page ?? 1,
            pageSize ?? 10,
            ct);
        return TypedResults.Ok(result);
    }

    private static async Task<Ok<CatalogueMetadataDto>> GetMetadata(
        [FromServices] IVideoGameService service,
        HttpContext httpContext,
        CancellationToken ct)
    {
        httpContext.Response.Headers.CacheControl = "public, max-age=3600";
        var metadata = await service.GetMetadataAsync(ct);
        return TypedResults.Ok(metadata);
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

    /// <summary>
    /// Translates Domain <see cref="Error"/> objects into RFC 7807 compliant <see cref="ProblemDetails"/> HTTP responses.
    /// Maps "NotFound" error codes to HTTP 404, and validation/invariants failures to HTTP 400 with machine-readable extensions.
    /// </summary>
    private static IResult ToProblemResult(Error error)
    {
        if (error.Code.EndsWith(".NotFound", StringComparison.OrdinalIgnoreCase))
        {
            return TypedResults.NotFound(new ProblemDetails
            {
                Title = "Resource Not Found",
                Detail = error.Description,
                Status = StatusCodes.Status404NotFound,
                Extensions = { ["errorCode"] = error.Code }
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
