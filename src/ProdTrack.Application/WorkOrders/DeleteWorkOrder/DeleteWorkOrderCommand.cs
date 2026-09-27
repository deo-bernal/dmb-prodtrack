using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;

namespace ProdTrack.Application.WorkOrders.DeleteWorkOrder;

/// <summary>Deletes a Draft work order (released work orders are cancelled instead, PT-022).</summary>
[RequiresPolicy(Policies.PlanWorkOrders)]
public sealed record DeleteWorkOrderCommand(int Id) : ICommand<Unit>;
