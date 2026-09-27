using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;
using ProdTrack.Domain.Products;

namespace ProdTrack.Application.Routings.GetRoutings;

[RequiresPolicy(Policies.ReadAll)]
public sealed record GetRoutingsQuery(ProductType? ProductType = null, bool CurrentOnly = false) : IQuery<IReadOnlyList<RoutingModel>>;
