using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;

namespace ProdTrack.Application.WorkOrders.UpdateWorkOrder;

[RequiresPolicy(Policies.PlanWorkOrders)]
public sealed record UpdateWorkOrderCommand(
    int Id,
    int Quantity,
    DateOnly DueDate,
    int Priority,
    string? CustomerName,
    string? Legend,
    byte[]? ExpectedVersion = null) : ICommand<Unit>;
