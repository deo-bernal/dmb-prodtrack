using Microsoft.AspNetCore.Mvc;
using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;
using ProdTrack.Application.WorkOrders.Artwork;
using ProdTrack.Application.WorkOrders.CreateWorkOrder;
using ProdTrack.Application.WorkOrders.DeleteWorkOrder;
using ProdTrack.Application.WorkOrders.GetWorkOrder;
using ProdTrack.Application.WorkOrders.ReleaseWorkOrder;
using ProdTrack.Application.WorkOrders.SearchWorkOrders;
using ProdTrack.Application.WorkOrders.UpdateWorkOrder;
using ProdTrack.Contracts.Common;
using ProdTrack.Contracts.WorkOrders;
using ProdTrack.Domain.WorkOrders;
using ProdTrack.Server.Errors;

namespace ProdTrack.Server.Endpoints;

internal static class WorkOrderEndpoints
{
    public static RouteGroupBuilder MapWorkOrderEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/work-orders").WithTags("Work orders");

        group.MapGet("/", async (IDispatcher dispatcher, string? status, string? search, DateOnly? dueBefore, bool? late, int? page, int? pageSize, CancellationToken ct) =>
            {
                var parsed = EnumBinding.ParseOptional<WorkOrderStatus>(status, "status");
                if (parsed.IsFailure)
                {
                    return parsed.Error!.ToProblem();
                }

                var query = new SearchWorkOrdersQuery(parsed.Value, search, dueBefore, late ?? false, page, pageSize);
                return (await dispatcher.QueryAsync(query, ct)).ToHttp(r =>
                    TypedResults.Ok(new PagedResponse<WorkOrderSummaryDto>([.. r.Items.Select(i => i.ToDto())], r.Page, r.PageSize, r.TotalCount)));
            })
            .RequireAuthorization(Policies.ReadAll)
            .WithSummary("Search work orders (status, text, due before, late), paged 25 by default")
            .Produces<PagedResponse<WorkOrderSummaryDto>>();

        group.MapGet("/{id:int}", async (IDispatcher dispatcher, int id, CancellationToken ct) =>
                (await dispatcher.QueryAsync(new GetWorkOrderQuery(id), ct)).ToHttp(w => TypedResults.Ok(w.ToDto())))
            .RequireAuthorization(Policies.ReadAll)
            .Produces<WorkOrderDetailDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/by-number/{number}", async (IDispatcher dispatcher, string number, CancellationToken ct) =>
                (await dispatcher.QueryAsync(new GetWorkOrderByNumberQuery(number), ct)).ToHttp(w => TypedResults.Ok(w.ToDto())))
            .RequireAuthorization(Policies.ReadAll)
            .Produces<WorkOrderDetailDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (IDispatcher dispatcher, CreateWorkOrderRequest request, CancellationToken ct) =>
                (await dispatcher.SendAsync(new CreateWorkOrderCommand(request.ProductId, request.Quantity, request.DueDate, request.Priority, request.CustomerName, request.Legend), ct))
                    .ToHttp(w => TypedResults.Created($"/api/v1/work-orders/{w.Id}", new WorkOrderCreatedResponse(w.Id, w.Number))))
            .RequireAuthorization(Policies.PlanWorkOrders)
            .Produces<WorkOrderCreatedResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:int}", async (IDispatcher dispatcher, int id, UpdateWorkOrderRequest request, CancellationToken ct) =>
                (await dispatcher.SendAsync(new UpdateWorkOrderCommand(id, request.Quantity, request.DueDate, request.Priority, request.CustomerName, request.Legend), ct))
                    .ToHttp(_ => TypedResults.NoContent()))
            .RequireAuthorization(Policies.PlanWorkOrders)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapDelete("/{id:int}", async (IDispatcher dispatcher, int id, CancellationToken ct) =>
                (await dispatcher.SendAsync(new DeleteWorkOrderCommand(id), ct)).ToHttp(_ => TypedResults.NoContent()))
            .RequireAuthorization(Policies.PlanWorkOrders)
            .WithSummary("Delete a Draft work order")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:int}/release", async (IDispatcher dispatcher, int id, CancellationToken ct) =>
                (await dispatcher.SendAsync(new ReleaseWorkOrderCommand(id), ct)).ToHttp(_ => TypedResults.NoContent()))
            .RequireAuthorization(Policies.PlanWorkOrders)
            .WithSummary("Release a Draft work order (creates operations from the current routing)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        MapArtwork(group);
        return api;
    }

    private static void MapArtwork(RouteGroupBuilder group)
    {
        group.MapPost("/{id:int}/artwork", async (IDispatcher dispatcher, int id, IFormFile file, CancellationToken ct) =>
            {
                await using var stream = file.OpenReadStream();
                return (await dispatcher.SendAsync(new UploadArtworkProofCommand(id, file.FileName, file.Length, stream), ct))
                    .ToHttp(p => TypedResults.Created($"/api/v1/work-orders/{id}/artwork/{p.Version}", p.ToDto()));
            })
            .RequireAuthorization(Policies.UploadArtwork)
            .DisableAntiforgery() // validated by the group filter for cookie-authenticated requests
            .WithMetadata(new RequestSizeLimitAttribute(ArtworkFileRules.MaxBytes + (1024 * 1024)))
            .WithSummary("Upload an artwork proof (PDF, PNG, JPG, SVG; max 20 MB)")
            .Produces<ArtworkProofDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/{id:int}/artwork/{version:int}", async (IDispatcher dispatcher, int id, int version, CancellationToken ct) =>
                (await dispatcher.QueryAsync(new GetArtworkProofFileQuery(id, version), ct))
                    .ToHttp(f => TypedResults.File(f.Content, f.ContentType, f.FileName)))
            .RequireAuthorization(Policies.ReadAll)
            .WithSummary("Download an artwork proof (streamed)");

        group.MapPost("/{id:int}/artwork/{version:int}/approve", async (IDispatcher dispatcher, int id, int version, ApproveArtworkRequest? request, CancellationToken ct) =>
                (await dispatcher.SendAsync(new ApproveArtworkProofCommand(id, version, request?.Note), ct)).ToHttp(_ => TypedResults.NoContent()))
            .RequireAuthorization(Policies.ApproveArtwork)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:int}/artwork/{version:int}/reject", async (IDispatcher dispatcher, int id, int version, RejectArtworkRequest request, CancellationToken ct) =>
                (await dispatcher.SendAsync(new RejectArtworkProofCommand(id, version, request.Reason), ct)).ToHttp(_ => TypedResults.NoContent()))
            .RequireAuthorization(Policies.ApproveArtwork)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }
}
