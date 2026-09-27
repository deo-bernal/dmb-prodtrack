using ProdTrack.Application.Messaging;
using ProdTrack.Application.Reference.GetReferenceList;
using ProdTrack.Application.Security;
using ProdTrack.Contracts.Reference;
using ProdTrack.Server.Errors;

namespace ProdTrack.Server.Endpoints;

internal static class ReferenceEndpoints
{
    public static RouteGroupBuilder MapReferenceEndpoints(this RouteGroupBuilder api)
    {
        api.MapGet("/reference/{list}", async (IDispatcher dispatcher, string list, CancellationToken ct) =>
                (await dispatcher.QueryAsync(new GetReferenceListQuery(list), ct))
                    .ToHttp(items => TypedResults.Ok(items.Select(i => i.ToDto()).ToList())))
            .RequireAuthorization(Policies.ReadAll)
            .WithTags("Reference data")
            .WithSummary("color-schemes, signal-words, product-types, station-types, reason-categories")
            .Produces<List<ReferenceItemDto>>()
            .ProducesProblem(StatusCodes.Status404NotFound);
        return api;
    }
}
