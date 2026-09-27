using ProdTrack.Application.Messaging;
using ProdTrack.Application.ReasonCodes.CreateReasonCode;
using ProdTrack.Application.ReasonCodes.GetReasonCodes;
using ProdTrack.Application.ReasonCodes.UpdateReasonCode;
using ProdTrack.Application.Security;
using ProdTrack.Contracts.ReasonCodes;
using ProdTrack.Domain.ReasonCodes;
using ProdTrack.Server.Errors;

namespace ProdTrack.Server.Endpoints;

internal static class ReasonCodeEndpoints
{
    public static RouteGroupBuilder MapReasonCodeEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/reason-codes").WithTags("Reason codes");

        group.MapGet("/", async (IDispatcher dispatcher, string? category, bool? includeInactive, CancellationToken ct) =>
            {
                var parsed = EnumBinding.ParseOptional<ReasonCategory>(category, "category");
                if (parsed.IsFailure)
                {
                    return parsed.Error!.ToProblem();
                }

                return (await dispatcher.QueryAsync(new GetReasonCodesQuery(parsed.Value, includeInactive ?? false), ct))
                    .ToHttp(items => TypedResults.Ok(items.Select(r => r.ToDto()).ToList()));
            })
            .RequireAuthorization(Policies.ReadAll)
            .Produces<List<ReasonCodeDto>>();

        group.MapPost("/", async (IDispatcher dispatcher, CreateReasonCodeRequest request, CancellationToken ct) =>
            {
                var category = EnumBinding.Parse<ReasonCategory>(request.Category, "category");
                if (category.IsFailure)
                {
                    return category.Error!.ToProblem();
                }

                return (await dispatcher.SendAsync(new CreateReasonCodeCommand(request.Code, request.Description, category.Value), ct))
                    .ToHttp(r => TypedResults.Created($"/api/v1/reason-codes/{r.Id}", r.ToDto()));
            })
            .RequireAuthorization(Policies.ManageMasterData)
            .Produces<ReasonCodeDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:int}", async (IDispatcher dispatcher, int id, UpdateReasonCodeRequest request, CancellationToken ct) =>
                (await dispatcher.SendAsync(new UpdateReasonCodeCommand(id, request.Description, request.IsActive), ct))
                    .ToHttp(r => TypedResults.Ok(r.ToDto())))
            .RequireAuthorization(Policies.ManageMasterData)
            .Produces<ReasonCodeDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return api;
    }
}
