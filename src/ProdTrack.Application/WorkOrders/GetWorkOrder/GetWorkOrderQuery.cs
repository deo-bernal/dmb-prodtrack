using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;

namespace ProdTrack.Application.WorkOrders.GetWorkOrder;

[RequiresPolicy(Policies.ReadAll)]
public sealed record GetWorkOrderQuery(int Id) : IQuery<WorkOrderDetailModel>;

[RequiresPolicy(Policies.ReadAll)]
public sealed record GetWorkOrderByNumberQuery(string Number) : IQuery<WorkOrderDetailModel>;
