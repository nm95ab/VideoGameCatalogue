using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using VideoGameCatalogue.Application.Games.DTOs;

namespace VideoGameCatalogue.AcceptanceTests.Drivers;

public class ProblemDetailsResponse
{
    public string? Title { get; set; }
    public string? Detail { get; set; }
    public int? Status { get; set; }
    public string? ErrorCode { get; set; }
}

public class VideoGameApiDriver(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public HttpResponseMessage? LastResponse { get; private set; }
    public HttpStatusCode LastStatusCode => LastResponse?.StatusCode ?? HttpStatusCode.InternalServerError;
    public string? LastResponseBody { get; private set; }

    public async Task GetAllGamesAsync(string? search = null, string? platform = null, string? genre = null)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(search))
            queryParams.Add($"search={Uri.EscapeDataString(search)}");
        if (!string.IsNullOrWhiteSpace(platform))
            queryParams.Add($"platform={Uri.EscapeDataString(platform)}");
        if (!string.IsNullOrWhiteSpace(genre))
            queryParams.Add($"genre={Uri.EscapeDataString(genre)}");

        var uri = "/api/games";
        if (queryParams.Count > 0)
            uri += "?" + string.Join("&", queryParams);

        LastResponse = await httpClient.GetAsync(uri);
        LastResponseBody = await LastResponse.Content.ReadAsStringAsync();
    }

    public async Task GetGameByIdAsync(string id)
    {
        LastResponse = await httpClient.GetAsync($"/api/games/{id}");
        LastResponseBody = await LastResponse.Content.ReadAsStringAsync();
    }

    public async Task CreateGameAsync(object request)
    {
        LastResponse = await httpClient.PostAsJsonAsync("/api/games", request);
        LastResponseBody = await LastResponse.Content.ReadAsStringAsync();
    }

    public async Task UpdateGameAsync(string id, object request)
    {
        LastResponse = await httpClient.PutAsJsonAsync($"/api/games/{id}", request);
        LastResponseBody = await LastResponse.Content.ReadAsStringAsync();
    }

    public async Task DeleteGameAsync(string id)
    {
        LastResponse = await httpClient.DeleteAsync($"/api/games/{id}");
        LastResponseBody = await LastResponse.Content.ReadAsStringAsync();
    }

    public async Task GetMetadataAsync()
    {
        LastResponse = await httpClient.GetAsync("/api/games/metadata");
        LastResponseBody = await LastResponse.Content.ReadAsStringAsync();
    }

    public async Task<List<GameDto>> ReadGamesListAsync()
    {
        if (string.IsNullOrWhiteSpace(LastResponseBody)) return [];
        return JsonSerializer.Deserialize<List<GameDto>>(LastResponseBody, JsonOptions) ?? [];
    }

    public async Task<GameDto?> ReadSingleGameAsync()
    {
        if (string.IsNullOrWhiteSpace(LastResponseBody)) return null;
        return JsonSerializer.Deserialize<GameDto>(LastResponseBody, JsonOptions);
    }

    public async Task UploadImageAsync(byte[] bytes, string fileName, string contentType)
    {
        using var multipart = new MultipartFormDataContent();
        var byteContent = new ByteArrayContent(bytes);
        byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        multipart.Add(byteContent, "file", fileName);

        LastResponse = await httpClient.PostAsync("/api/images", multipart);
        LastResponseBody = await LastResponse.Content.ReadAsStringAsync();
    }

    public async Task GetImageDirectAsync(string imageId)
    {
        LastResponse = await httpClient.GetAsync($"/api/images/{imageId}");
        LastResponseBody = await LastResponse.Content.ReadAsStringAsync();
    }

    public async Task DeleteImageDirectAsync(string imageId)
    {
        LastResponse = await httpClient.DeleteAsync($"/api/images/{imageId}");
        LastResponseBody = await LastResponse.Content.ReadAsStringAsync();
    }

    public async Task<VideoGameCatalogue.Application.Images.ImageUploadResponse?> ReadImageUploadResponseAsync()
    {
        if (string.IsNullOrWhiteSpace(LastResponseBody)) return null;
        return JsonSerializer.Deserialize<VideoGameCatalogue.Application.Images.ImageUploadResponse>(LastResponseBody, JsonOptions);
    }

    public async Task<CatalogueMetadataDto?> ReadMetadataAsync()
    {
        if (string.IsNullOrWhiteSpace(LastResponseBody)) return null;
        return JsonSerializer.Deserialize<CatalogueMetadataDto>(LastResponseBody, JsonOptions);
    }

    public async Task<ProblemDetailsResponse?> ReadProblemDetailsAsync()
    {
        if (string.IsNullOrWhiteSpace(LastResponseBody)) return null;
        return JsonSerializer.Deserialize<ProblemDetailsResponse>(LastResponseBody, JsonOptions);
    }
}
