using ProdTrack.Application.Common;
using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Application.WorkOrders.SearchWorkOrders;

[RequiresPolicy(Policies.ReadAll)]
public sealed record SearchWorkOrdersQuery(
    WorkOrderStatus? Status = null,
    string? Search = null,
    DateOnly? DueBefore = null,
    bool LateOnly = false,
    int? Page = null,
    int? PageSize = null) : IQuery<PagedResult<WorkOrderSummaryModel>>;
