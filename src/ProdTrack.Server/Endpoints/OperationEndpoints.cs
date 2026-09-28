using ProdTrack.Application.Messaging;
using ProdTrack.Application.Operations;
using ProdTrack.Application.Quality;
using ProdTrack.Application.Scanning;
using ProdTrack.Application.Security;
using ProdTrack.Contracts.Operations;
using ProdTrack.Domain.Quality;
using ProdTrack.Server.Errors;

namespace ProdTrack.Server.Endpoints;

/// <summary>Shop-floor execution API used by the /floor PWA (docs/02 section 6, PT-023..PT-028).</summary>
internal static class OperationEndpoints
{
    public static RouteGroupBuilder MapOperationEndpoints(this RouteGroupBuilder api)
    {
        api.MapGet("/stations/{code}/queue", async (IDispatcher dispatcher, string code, CancellationToken ct) =>
                (await dispatcher.QueryAsync(new GetStationQueueQuery(code), ct)).ToHttp(q => TypedResults.Ok(q.ToDto())))
            .WithTags("Operations")
            .RequireAuthorization(Policies.ReadAll)
            .WithSummary("Actionable operations at a station, ordered by priority and due date")
            .Produces<StationQueueDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        api.MapGet("/scan/{code}", async (IDispatcher dispatcher, string code, CancellationToken ct) =>
                (await dispatcher.QueryAsync(new ResolveScanCodeQuery(code), ct)).ToHttp(r => TypedResults.Ok(r.ToDto())))
            .WithTags("Operations")
            .RequireAuthorization(Policies.ExecuteOperations)
            .WithSummary("Resolve a scanned code (WO:..., OP:number:seq, ST:..., or a bare WO number)")
            .Produces<ScanResolutionDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        api.MapGet("/qc/templates", async (IDispatcher dispatcher, CancellationToken ct) =>
                (await dispatcher.QueryAsync(new GetQcTemplatesQuery(), ct)).ToHttp(t => TypedResults.Ok(t.Select(x => x.ToDto()).ToList())))
            .WithTags("Quality")
            .RequireAuthorization(Policies.ReadAll)
            .Produces<List<QcTemplateDto>>();

        var group = api.MapGroup("/operations").WithTags("Operations");

        group.MapGet("/{id:int}", async (IDispatcher dispatcher, int id, CancellationToken ct) =>
                (await dispatcher.QueryAsync(new GetOperationQuery(id), ct)).ToHttp(o => TypedResults.Ok(o.ToDto())))
            .RequireAuthorization(Policies.ReadAll)
            .Produces<OperationDetailDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:int}/start", async (IDispatcher dispatcher, int id, StartOperationRequest? request, CancellationToken ct) =>
                (await dispatcher.SendAsync(new StartOperationCommand(id, request?.StationCode, request?.RequestId), ct)).ToHttp(_ => TypedResults.NoContent()))
            .RequireAuthorization(Policies.ExecuteOperations)
            .WithSummary("Start (clock on). Optional StationCode must match; RequestId makes retries idempotent.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:int}/pause", async (IDispatcher dispatcher, int id, PauseOperationRequest request, CancellationToken ct) =>
                (await dispatcher.SendAsync(new PauseOperationCommand(id, request.ReasonCodeId, request.RequestId), ct)).ToHttp(_ => TypedResults.NoContent()))
            .RequireAuthorization(Policies.ExecuteOperations)
            .WithSummary("Pause with a Pause-category reason code")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:int}/resume", async (IDispatcher dispatcher, int id, ResumeOperationRequest? request, CancellationToken ct) =>
                (await dispatcher.SendAsync(new ResumeOperationCommand(id, request?.RequestId), ct)).ToHttp(_ => TypedResults.NoContent()))
            .RequireAuthorization(Policies.ExecuteOperations)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:int}/complete", async (IDispatcher dispatcher, int id, CompleteOperationRequest request, CancellationToken ct) =>
                (await dispatcher.SendAsync(new CompleteOperationCommand(id, request.GoodQuantity, request.RequestId), ct)).ToHttp(_ => TypedResults.NoContent()))
            .RequireAuthorization(Policies.ExecuteOperations)
            .WithSummary("Complete with a good quantity (good + scrap must not exceed input); the next step becomes Ready")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:int}/scrap", async (IDispatcher dispatcher, int id, LogScrapRequest request, CancellationToken ct) =>
                (await dispatcher.SendAsync(new LogScrapCommand(id, request.Quantity, request.ReasonCodeId, request.Note), ct)).ToHttp(_ => TypedResults.NoContent()))
            .RequireAuthorization(Policies.ExecuteOperations)
            .WithSummary("Log scrap with a Scrap-category reason code")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:int}/inspection", async (IDispatcher dispatcher, int id, RecordInspectionRequest request, CancellationToken ct) =>
            {
                var items = (request.Items ?? []).Select(i => new QcItemResultInput(i.ChecklistItemId, i.Passed, i.MeasuredValue)).ToList();
                return (await dispatcher.SendAsync(new RecordInspectionCommand(id, request.SampleSize, items, request.Notes), ct))
                    .ToHttp(r => TypedResults.Ok(new InspectionRecordedResponse(r.InspectionId, r.Result.ToString(), r.Disposition.ToString())));
            })
            .RequireAuthorization(Policies.RecordInspections)
            .WithSummary("Record a QC inspection: pass completes the step, fail puts the work order on hold (QC-FAIL)")
            .Produces<InspectionRecordedResponse>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        return api;
    }
}
