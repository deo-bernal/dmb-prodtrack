using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ProdTrack.Contracts;
using ProdTrack.Contracts.Common;
using ProdTrack.Contracts.Operations;
using ProdTrack.Contracts.ReasonCodes;
using ProdTrack.Contracts.Stations;
using ProdTrack.Contracts.Users;

namespace ProdTrack.ApiClient;

/// <summary>
/// Typed client for the ProdTrack REST API (same-origin cookie session, no tokens in the browser). Unsafe requests
/// carry the antiforgery header obtained from <c>GET /api/v1/antiforgery</c> (refreshed once on rejection).
/// </summary>
public sealed class ProdTrackApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private AntiforgeryTokenResponse? antiforgery;

    public Task<ApiResult<MeResponse>> GetMeAsync(CancellationToken cancellationToken = default) =>
        GetAsync<MeResponse>(ApiRoutes.Me, cancellationToken);

    public Task<ApiResult<List<StationDto>>> GetStationsAsync(CancellationToken cancellationToken = default) =>
        GetAsync<List<StationDto>>(ApiRoutes.Stations, cancellationToken);

    public Task<ApiResult<List<ReasonCodeDto>>> GetReasonCodesAsync(string category, CancellationToken cancellationToken = default) =>
        GetAsync<List<ReasonCodeDto>>($"{ApiRoutes.ReasonCodes}?category={Uri.EscapeDataString(category)}", cancellationToken);

    public Task<ApiResult<StationQueueDto>> GetStationQueueAsync(string stationCode, CancellationToken cancellationToken = default) =>
        GetAsync<StationQueueDto>(ApiRoutes.StationQueue(stationCode), cancellationToken);

    public Task<ApiResult<ScanResolutionDto>> ResolveScanAsync(string code, CancellationToken cancellationToken = default) =>
        GetAsync<ScanResolutionDto>($"{ApiRoutes.Scan}/{Uri.EscapeDataString(code.Trim())}", cancellationToken);

    public Task<ApiResult<OperationDetailDto>> GetOperationAsync(int operationId, CancellationToken cancellationToken = default) =>
        GetAsync<OperationDetailDto>(ApiRoutes.Operation(operationId), cancellationToken);

    public Task<ApiResult<bool>> StartOperationAsync(int operationId, StartOperationRequest request, CancellationToken cancellationToken = default) =>
        PostAsync(ApiRoutes.Operation(operationId) + "/start", request, cancellationToken);

    public Task<ApiResult<bool>> PauseOperationAsync(int operationId, PauseOperationRequest request, CancellationToken cancellationToken = default) =>
        PostAsync(ApiRoutes.Operation(operationId) + "/pause", request, cancellationToken);

    public Task<ApiResult<bool>> ResumeOperationAsync(int operationId, ResumeOperationRequest request, CancellationToken cancellationToken = default) =>
        PostAsync(ApiRoutes.Operation(operationId) + "/resume", request, cancellationToken);

    public Task<ApiResult<bool>> CompleteOperationAsync(int operationId, CompleteOperationRequest request, CancellationToken cancellationToken = default) =>
        PostAsync(ApiRoutes.Operation(operationId) + "/complete", request, cancellationToken);

    public Task<ApiResult<bool>> LogScrapAsync(int operationId, LogScrapRequest request, CancellationToken cancellationToken = default) =>
        PostAsync(ApiRoutes.Operation(operationId) + "/scrap", request, cancellationToken);

    public async Task<ApiResult<InspectionRecordedResponse>> RecordInspectionAsync(int operationId, RecordInspectionRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await SendUnsafeAsync(ApiRoutes.Operation(operationId) + "/inspection", request, cancellationToken);
        return response.IsSuccessStatusCode
            ? new ApiResult<InspectionRecordedResponse>(await response.Content.ReadFromJsonAsync<InspectionRecordedResponse>(JsonOptions, cancellationToken), null)
            : new ApiResult<InspectionRecordedResponse>(null, await ReadErrorAsync(response, cancellationToken));
    }

    private async Task<ApiResult<T>> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(Relative(path), cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return new ApiResult<T>(await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken), null);
        }

        return new ApiResult<T>(default, await ReadErrorAsync(response, cancellationToken));
    }

    private async Task<ApiResult<bool>> PostAsync<TBody>(string path, TBody body, CancellationToken cancellationToken)
    {
        using var response = await SendUnsafeAsync(path, body, cancellationToken);
        return response.IsSuccessStatusCode
            ? new ApiResult<bool>(true, null)
            : new ApiResult<bool>(false, await ReadErrorAsync(response, cancellationToken));
    }

    private async Task<HttpResponseMessage> SendUnsafeAsync<TBody>(string path, TBody body, CancellationToken cancellationToken)
    {
        var response = await PostOnceAsync(path, body, refreshToken: false, cancellationToken);
        if (response.StatusCode == HttpStatusCode.BadRequest && await IsAntiforgeryRejectionAsync(response, cancellationToken))
        {
            response.Dispose();
            response = await PostOnceAsync(path, body, refreshToken: true, cancellationToken);
        }

        return response;
    }

    private async Task<HttpResponseMessage> PostOnceAsync<TBody>(string path, TBody body, bool refreshToken, CancellationToken cancellationToken)
    {
        if (antiforgery is null || refreshToken)
        {
            antiforgery = await httpClient.GetFromJsonAsync<AntiforgeryTokenResponse>(Relative(ApiRoutes.Antiforgery), JsonOptions, cancellationToken);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, Relative(path)) { Content = JsonContent.Create(body, options: JsonOptions) };
        if (antiforgery is not null)
        {
            request.Headers.TryAddWithoutValidation(antiforgery.HeaderName, antiforgery.Token);
        }

        return await httpClient.SendAsync(request, cancellationToken);
    }

    private static async Task<bool> IsAntiforgeryRejectionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var text = await response.Content.ReadAsStringAsync(cancellationToken);
        return text.Contains("Antiforgery.Invalid", StringComparison.Ordinal);
    }

    private static Uri Relative(string path) => new(path.TrimStart('/'), UriKind.Relative);

    private static async Task<ApiError> ReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return new ApiError(401, "Please sign in.", null);
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            return new ApiError(403, "Your role is not allowed to do this.", null);
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
