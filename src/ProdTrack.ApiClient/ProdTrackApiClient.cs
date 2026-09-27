using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ProdTrack.Contracts;
using ProdTrack.Contracts.Stations;
using ProdTrack.Contracts.Users;

namespace ProdTrack.ApiClient;

/// <summary>Typed client for the ProdTrack REST API (same-origin cookie session, no tokens in the browser).</summary>
public sealed class ProdTrackApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public Task<ApiResult<MeResponse>> GetMeAsync(CancellationToken cancellationToken = default) =>
        GetAsync<MeResponse>(ApiRoutes.Me, cancellationToken);

    public Task<ApiResult<List<StationDto>>> GetStationsAsync(CancellationToken cancellationToken = default) =>
        GetAsync<List<StationDto>>(ApiRoutes.Stations, cancellationToken);

    private async Task<ApiResult<T>> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(new Uri(path.TrimStart('/'), UriKind.Relative), cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return new ApiResult<T>(await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken), null);
        }

        return new ApiResult<T>(default, await ReadErrorAsync(response, cancellationToken));
    }

    private static async Task<ApiError> ReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return new ApiError(401, "Please sign in.", null);
        }

        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemBody>(JsonOptions, cancellationToken);
            return new ApiError((int)response.StatusCode, problem?.Title ?? response.ReasonPhrase ?? "Error", problem?.Detail);
        }
        catch (JsonException)
        {
            return new ApiError((int)response.StatusCode, response.ReasonPhrase ?? "Error", null);
        }
    }

    private sealed record ProblemBody(string? Title, string? Detail);
}
