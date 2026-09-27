using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;
using ProdTrack.Application.Stations.CreateStation;
using ProdTrack.Application.Stations.DeactivateStation;
using ProdTrack.Application.Stations.GetStations;
using ProdTrack.Application.Stations.UpdateStation;
using ProdTrack.Contracts.Stations;
using ProdTrack.Domain.Stations;
using ProdTrack.Server.Errors;

namespace ProdTrack.Server.Endpoints;

internal static class StationEndpoints
{
    public static RouteGroupBuilder MapStationEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/stations").WithTags("Stations");

        group.MapGet("/", async (IDispatcher dispatcher, bool? includeInactive, CancellationToken ct) =>
                (await dispatcher.QueryAsync(new GetStationsQuery(includeInactive ?? false), ct))
                    .ToHttp(items => TypedResults.Ok(items.Select(s => s.ToDto()).ToList())))
            .RequireAuthorization(Policies.ReadAll)
            .Produces<List<StationDto>>();

        group.MapPost("/", async (IDispatcher dispatcher, CreateStationRequest request, CancellationToken ct) =>
            {
                var type = EnumBinding.Parse<StationType>(request.Type, "type");
                if (type.IsFailure)
                {
                    return type.Error!.ToProblem();
                }

                var result = await dispatcher.SendAsync(new CreateStationCommand(request.Code, request.Name, type.Value, request.WorkCenter), ct);
                return result.ToHttp(s => TypedResults.Created($"/api/v1/stations/{s.Id}", s.ToDto()));
            })
            .RequireAuthorization(Policies.ManageMasterData)
            .Produces<StationDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:int}", async (IDispatcher dispatcher, int id, UpdateStationRequest request, CancellationToken ct) =>
            {
                var type = EnumBinding.Parse<StationType>(request.Type, "type");
                if (type.IsFailure)
                {
                    return type.Error!.ToProblem();
                }

                var result = await dispatcher.SendAsync(new UpdateStationCommand(id, request.Name, type.Value, request.WorkCenter, request.IsActive), ct);
                return result.ToHttp(s => TypedResults.Ok(s.ToDto()));
            })
            .RequireAuthorization(Policies.ManageMasterData)
            .Produces<StationDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", async (IDispatcher dispatcher, int id, CancellationToken ct) =>
                (await dispatcher.SendAsync(new DeactivateStationCommand(id), ct))
                    .ToHttp(r => TypedResults.Ok(new DeactivateStationResponse(r.Station.ToDto(), r.OpenOperationCount))))
            .RequireAuthorization(Policies.ManageMasterData)
            .WithSummary("Deactivate a station (open work is unaffected; the response reports open operations)")
            .Produces<DeactivateStationResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return api;
    }
}
