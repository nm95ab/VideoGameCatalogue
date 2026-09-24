using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VideoGameCatalogue.Application.Images;
using VideoGameCatalogue.Domain.Common;

namespace VideoGameCatalogue.Api.Endpoints;

/// <summary>
/// Inbound (Driving) Adapter exposing RESTful endpoints for image upload and deletion.
/// <para>
/// Note: Image retrieval is served directly via kernel-level zero-copy static file middleware
/// (<c>app.UseStaticFiles()</c>) in local development, or via direct CDN/Blob URLs in cloud deployments,
/// eliminating reverse-proxy overhead and thread pool saturation on the API server.
/// </para>
/// </summary>
public static class ImageEndpoints
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public static IEndpointRouteBuilder MapImageEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/images")
            .WithTags("Images");

        group.MapPost("/", UploadImage)
            .DisableAntiforgery()
            .RequireRateLimiting("UploadsPolicy");

        // Existing images are served by UseStaticFiles() earlier in the middleware pipeline.
        // If an image is missing or deleted, requests fall through to this endpoint returning 404 Not Found.
        group.MapGet("/{imageId}", () => TypedResults.NotFound());

        group.MapDelete("/{imageId}", DeleteImage);

        return app;
    }

    private static async Task<IResult> UploadImage(
        [FromServices] IImageManagementService service,
        IFormFile? file,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            return ToProblemResult(Error.Validation("Image.Empty", "An image file must be provided."));
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return ToProblemResult(Error.Validation("Image.TooLarge", "The image file exceeds the 10MB limit."));
        }

        await using var stream = file.OpenReadStream();
        var result = await service.UploadThumbnailAsync(stream, file.FileName, file.ContentType, ct);
        if (result.IsFailure)
        {
            return ToProblemResult(result.Error);
        }

        return TypedResults.Created(result.Value.Url, result.Value);
    }

    private static async Task<IResult> DeleteImage(
        [FromServices] IImageManagementService service,
        [FromRoute] string imageId,
        CancellationToken ct)
    {
        var result = await service.DeleteImageAsync(imageId, ct);
        if (result.IsFailure)
        {
            return ToProblemResult(result.Error);
        }

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
