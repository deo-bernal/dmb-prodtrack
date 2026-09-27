namespace ProdTrack.ApiClient;

public sealed record ApiResult<T>(T? Value, ApiError? Error)
{
    public bool IsSuccess => Error is null;
}
