using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;

namespace ProdTrack.Application.WorkOrders.ReleaseWorkOrder;

/// <summary>Releases a Draft work order to the floor using the current routing of its product type (PT-020).</summary>
[RequiresPolicy(Policies.PlanWorkOrders)]
public sealed record ReleaseWorkOrderCommand(int Id) : ICommand<Unit>;
