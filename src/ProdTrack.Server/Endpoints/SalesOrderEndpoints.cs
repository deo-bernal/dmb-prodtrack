using ProdTrack.Application.Messaging;
using ProdTrack.Application.SalesOrders;
using ProdTrack.Application.Security;
using ProdTrack.Contracts.Common;
using ProdTrack.Contracts.SalesOrders;
using ProdTrack.Domain.SalesOrders;
using ProdTrack.Server.Errors;

namespace ProdTrack.Server.Endpoints;

internal static class SalesOrderEndpoints
{
    public static RouteGroupBuilder MapSalesOrderEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/sales-orders").WithTags("Sales orders");

        group.MapGet("/", async (IDispatcher dispatcher, string? status, string? search, int? page, int? pageSize, CancellationToken ct) =>
            {
                var parsed = EnumBinding.ParseOptional<SalesOrderStatus>(status, "status");
                if (parsed.IsFailure)
                {
                    return parsed.Error!.ToProblem();
                }

                return (await dispatcher.QueryAsync(new SearchSalesOrdersQuery(parsed.Value, search, page, pageSize), ct)).ToHttp(r =>
                    TypedResults.Ok(new PagedResponse<SalesOrderSummaryDto>([.. r.Items.Select(i => i.ToDto())], r.Page, r.PageSize, r.TotalCount)));
            })
            .RequireAuthorization(Policies.ReadAll)
            .WithSummary("Search sales orders (status, customer/PO/number text), paged")
            .Produces<PagedResponse<SalesOrderSummaryDto>>();

        group.MapGet("/{id:int}", async (HttpContext http, IDispatcher dispatcher, int id, CancellationToken ct) =>
                (await dispatcher.QueryAsync(new GetSalesOrderQuery(id), ct)).ToHttp(s =>
                {
                    ETags.Set(http.Response, s.Version);
                    return TypedResults.Ok(s.ToDto());
                }))
            .RequireAuthorization(Policies.ReadAll)
            .WithSummary("Get a sales order with lines (ETag = rowversion)")
            .Produces<SalesOrderDetailDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (IDispatcher dispatcher, CreateSalesOrderRequest request, CancellationToken ct) =>
                (await dispatcher.SendAsync(new CreateSalesOrderCommand(request.CustomerName, request.PoNumber, request.DueDate, ToInputs(request.Lines)), ct))
                    .ToHttp(s => TypedResults.Created($"/api/v1/sales-orders/{s.Id}", new SalesOrderCreatedResponse(s.Id, s.Number))))
            .RequireAuthorization(Policies.PlanWorkOrders)
            .Produces<SalesOrderCreatedResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:int}", async (HttpContext http, IDispatcher dispatcher, int id, UpdateSalesOrderRequest request, CancellationToken ct) =>
            {
                var version = ETags.ReadVersion(http.Request);
                if (version.IsFailure)
                {
                    return version.Error!.ToProblem();
                }

                var command = new UpdateSalesOrderCommand(id, request.CustomerName, request.PoNumber, request.DueDate, ToInputs(request.Lines), version.Value);
                return (await dispatcher.SendAsync(command, ct)).ToHttp(s =>
                {
                    ETags.Set(http.Response, s.Version);
                    return TypedResults.Ok(s.ToDto());
                });
            })
            .RequireAuthorization(Policies.PlanWorkOrders)
            .WithSummary("Update header and lines (lines with work orders are locked). Optional If-Match.")
            .Produces<SalesOrderDetailDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status412PreconditionFailed)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:int}/cancel", async (HttpContext http, IDispatcher dispatcher, int id, CancellationToken ct) =>
            {
                var version = ETags.ReadVersion(http.Request);
                if (version.IsFailure)
                {
                    return version.Error!.ToProblem();
                }

                return (await dispatcher.SendAsync(new CancelSalesOrderCommand(id, version.Value), ct)).ToHttp(_ => TypedResults.NoContent());
            })
            .RequireAuthorization(Policies.PlanWorkOrders)
            .WithSummary("Cancel a sales order (rejected while it has open work orders)")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{id:int}/work-orders", async (IDispatcher dispatcher, int id, GenerateWorkOrdersRequest? request, CancellationToken ct) =>
                (await dispatcher.SendAsync(new CreateWorkOrdersFromSalesOrderCommand(id, request?.Priority ?? 3, request?.LineIds), ct))
                    .ToHttp(r => TypedResults.Ok(new GenerateWorkOrdersResponse(
                        [.. r.Created.Select(c => new GeneratedWorkOrderDto(c.LineNumber, c.WorkOrderId, c.WorkOrderNumber))],
                        [.. r.Skipped.Select(s => new SkippedLineDto(s.LineNumber, s.WorkOrderNumber))]))))
            .RequireAuthorization(Policies.PlanWorkOrders)
            .WithSummary("Create Draft work orders from sales order lines (lines that already have one are skipped)")
            .Produces<GenerateWorkOrdersResponse>()
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        return api;
    }

    private static List<SalesOrderLineInput> ToInputs(IReadOnlyList<SalesOrderLineRequest>? lines) =>
        [.. (lines ?? []).Select(l => new SalesOrderLineInput(l.LineId, l.ProductId, l.Quantity, l.Legend, l.Spec is null ? null : l.Spec.ToDomain()))];
}
