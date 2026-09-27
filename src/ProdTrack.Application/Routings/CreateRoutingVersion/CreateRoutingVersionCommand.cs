using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;
using ProdTrack.Domain.Products;

namespace ProdTrack.Application.Routings.CreateRoutingVersion;

/// <summary>Creates the next routing version for a product type; the previous version stops being current (PT-014).</summary>
[RequiresPolicy(Policies.ManageProductsAndRoutings)]
public sealed record CreateRoutingVersionCommand(ProductType ProductType, string Name, IReadOnlyList<RoutingStepInput> Steps) : ICommand<RoutingModel>;

public sealed record RoutingStepInput(int Sequence, string StationCode, decimal SetupMinutes, decimal StdMinutesPerUnit, bool AllowOverlap);
