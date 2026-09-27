using ProdTrack.Application.Dashboard.GetDashboardSummary;
using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;
using ProdTrack.Contracts.Dashboard;
using ProdTrack.Server.Errors;

namespace ProdTrack.Server.Endpoints;

internal static class DashboardEndpoints
{
    public static RouteGroupBuilder MapDashboardEndpoints(this RouteGroupBuilder api)
    {
        api.MapGet("/dashboard/wip", async (IDispatcher dispatcher, CancellationToken ct) =>
                (await dispatcher.QueryAsync(new GetDashboardSummaryQuery(), ct)).ToHttp(d => TypedResults.Ok(d.ToDto())))
            .RequireAuthorization(Policies.ReadAll)
            .WithTags("Dashboard")
            .WithSummary("Work orders by status, late count and WIP by station")
            .Produces<DashboardSummaryDto>();
        return api;
    }
}
