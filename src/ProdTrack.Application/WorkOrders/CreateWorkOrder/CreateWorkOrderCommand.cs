using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;

namespace ProdTrack.Application.WorkOrders.CreateWorkOrder;

/// <summary>Creates a Draft work order directly from a catalog product (generation from sales order lines is PT-019).</summary>
[RequiresPolicy(Policies.PlanWorkOrders)]
public sealed record CreateWorkOrderCommand(
    int ProductId,
    int Quantity,
    DateOnly DueDate,
    int Priority,
    string? CustomerName,
    string? Legend) : ICommand<WorkOrderCreatedModel>;
