using System.Diagnostics;
using ProdTrack.Application.Abstractions;

namespace ProdTrack.Server.Logging;

internal sealed class CorrelationIdAccessor(IHttpContextAccessor httpContextAccessor) : ICorrelationIdAccessor
{
    public string? CorrelationId =>
        httpContextAccessor.HttpContext?.Items[CorrelationIdMiddleware.ItemKey] as string
        ?? Activity.Current?.TraceId.ToString();
}
