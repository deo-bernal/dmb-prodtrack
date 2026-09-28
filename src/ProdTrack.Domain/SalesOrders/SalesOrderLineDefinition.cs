using ProdTrack.Domain.Products;

namespace ProdTrack.Domain.SalesOrders;

/// <summary>
/// Input for a sales order line. <paramref name="LineId"/> is null for a new line. <paramref name="SpecOverride"/>
/// replaces the catalog default spec for this line only (PT-018 "Spec override").
/// </summary>
public sealed record SalesOrderLineDefinition(int? LineId, Product Product, int Quantity, string? Legend, ProductSpec? SpecOverride);
