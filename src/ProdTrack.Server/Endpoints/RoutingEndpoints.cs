using ProdTrack.Application.Messaging;
using ProdTrack.Application.Routings.CreateRoutingVersion;
using ProdTrack.Application.Routings.GetRouting;
using ProdTrack.Application.Routings.GetRoutings;
using ProdTrack.Application.Security;
using ProdTrack.Contracts.Routings;
using ProdTrack.Domain.Products;
using ProdTrack.Server.Errors;

namespace ProdTrack.Server.Endpoints;

internal static class RoutingEndpoints
{
    public static RouteGroupBuilder MapRoutingEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/routings").WithTags("Routings");

        group.MapGet("/", async (IDispatcher dispatcher, string? productType, bool? currentOnly, CancellationToken ct) =>
            {
                var type = EnumBinding.ParseOptional<ProductType>(productType, "productType");
                if (type.IsFailure)
                {
                    return type.Error!.ToProblem();
                }

                return (await dispatcher.QueryAsync(new GetRoutingsQuery(type.Value, currentOnly ?? false), ct))
                    .ToHttp(items => TypedResults.Ok(items.Select(r => r.ToDto()).ToList()));
            })
            .RequireAuthorization(Policies.ReadAll)
            .Produces<List<RoutingDto>>();

        group.MapGet("/{id:int}", async (IDispatcher dispatcher, int id, CancellationToken ct) =>
                (await dispatcher.QueryAsync(new GetRoutingQuery(id), ct)).ToHttp(r => TypedResults.Ok(r.ToDto())))
            .RequireAuthorization(Policies.ReadAll)
            .Produces<RoutingDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (IDispatcher dispatcher, CreateRoutingRequest request, CancellationToken ct) =>
            {
                var type = EnumBinding.Parse<ProductType>(request.ProductType, "productType");
                if (type.IsFailure)
                {
                    return type.Error!.ToProblem();
                }

                var steps = (request.Steps ?? []).Select(s => new RoutingStepInput(s.Sequence, s.StationCode, s.SetupMinutes, s.StdMinutesPerUnit, s.AllowOverlap)).ToList();
                return (await dispatcher.SendAsync(new CreateRoutingVersionCommand(type.Value, request.Name, steps), ct))
                    .ToHttp(r => TypedResults.Created($"/api/v1/routings/{r.Id}", r.ToDto()));
            })
            .RequireAuthorization(Policies.ManageProductsAndRoutings)
            .WithSummary("Create the next routing version for a product type")
            .Produces<RoutingDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        return api;
    }
}
