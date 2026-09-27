using ProdTrack.Domain.Common;

namespace ProdTrack.Server.Errors;

/// <summary>Maps Result errors to ProblemDetails: 400/403/404/409/422 with a stable <c>code</c> (docs/02 section 11).</summary>
internal static class ResultHttpExtensions
{
    public static IResult ToProblem(this Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        var extensions = new Dictionary<string, object?> { ["code"] = error.Code };
        if (error is ValidationError validation)
        {
            return TypedResults.ValidationProblem(
                validation.Errors.ToDictionary(e => e.Key, e => e.Value, StringComparer.Ordinal),
                detail: error.Message,
                extensions: extensions);
        }

        var status = error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.BusinessRule => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status400BadRequest,
        };
        return TypedResults.Problem(statusCode: status, title: error.Message, detail: error.Message, extensions: extensions);
    }

    public static IResult ToHttp<T>(this Result<T> result, Func<T, IResult> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(onSuccess);
        return result.IsSuccess ? onSuccess(result.Value) : result.Error!.ToProblem();
    }
}
