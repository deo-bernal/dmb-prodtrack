using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;

namespace ProdTrack.Application.Routings.GetRouting;

[RequiresPolicy(Policies.ReadAll)]
public sealed record GetRoutingQuery(int Id) : IQuery<RoutingModel>;
