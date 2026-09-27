using ProdTrack.Domain.Common;
using ProdTrack.Domain.Products;

namespace ProdTrack.Domain.SalesOrders;

/// <summary>A line of a sales order: product, quantity, legend text and the line's own spec (copied or overridden).</summary>
public sealed class SalesOrderLine : Entity
{
    public const int LegendMaxLength = 500;

    private SalesOrderLine()
    {
    }

    public int SalesOrderId { get; private set; }

    public int LineNumber { get; private set; }

    public int ProductId { get; private set; }

    public int Quantity { get; private set; }

    public string? Legend { get; private set; }

    public ProductSpec Spec { get; private set; } = new();

    internal static SalesOrderLine? Create(int lineNumber, SalesOrderLineDefinition definition, string field, List<KeyValuePair<string, string>> errors)
    {
        var line = new SalesOrderLine { LineNumber = lineNumber };
        var before = errors.Count;
        line.Apply(definition, field, errors);
        return errors.Count == before ? line : null;
    }

    internal void Apply(SalesOrderLineDefinition definition, string field, List<KeyValuePair<string, string>> errors)
    {
        ArgumentNullException.ThrowIfNull(definition);
        var product = definition.Product;
        var before = errors.Count;
        if (!product.IsActive && product.Id != ProductId)
        {
            errors.Add(new($"{field}.productId", $"Product {product.Sku} is inactive."));
        }

        if (definition.Quantity <= 0)
        {
            errors.Add(new($"{field}.quantity", "Quantity must be greater than 0."));
        }

        var legend = string.IsNullOrWhiteSpace(definition.Legend) ? null : definition.Legend.Trim();
        if (product.ProductType == ProductType.PipeMarker && legend is null)
        {
            errors.Add(new($"{field}.legend", "Legend text is required for pipe markers."));
        }
        else if (legend?.Length > LegendMaxLength)
        {
            errors.Add(new($"{field}.legend", $"Legend must be at most {LegendMaxLength} characters."));
        }

        var spec = definition.SpecOverride ?? product.DefaultSpec;
        errors.AddRange(spec.Validate(product.ProductType).Select(e => new KeyValuePair<string, string>($"{field}.{e.Key}", e.Value)));
        if (errors.Count > before)
        {
            return;
        }

        ProductId = product.Id;
        Quantity = definition.Quantity;
        Legend = legend;
        Spec = spec;
    }

    internal bool IsSameAs(SalesOrderLineDefinition definition) =>
        definition.Product.Id == ProductId
        && definition.Quantity == Quantity
        && string.Equals(string.IsNullOrWhiteSpace(definition.Legend) ? null : definition.Legend.Trim(), Legend, StringComparison.Ordinal)
        && (definition.SpecOverride is null || definition.SpecOverride == Spec);
}
