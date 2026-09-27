using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;

namespace ProdTrack.Application.Dashboard.GetDashboardSummary;

/// <summary>Basic supervisor dashboard: work orders by status, late count, WIP by station (live updates arrive with PT-041).</summary>
[RequiresPolicy(Policies.ReadAll)]
public sealed record GetDashboardSummaryQuery : IQuery<DashboardSummaryModel>;
