using ProdTrack.Domain.Common;

namespace ProdTrack.Domain.Products;

/// <summary>
/// Product specification. Stored as JSON on products (defaults) and snapshotted onto work orders (BR-02).
/// Which fields are required depends on the <see cref="ProductType"/>.
/// </summary>
public sealed record ProductSpec
{
    public string? Material { get; init; }

    public decimal? WidthMm { get; init; }

    public decimal? HeightMm { get; init; }

    public string? Mounting { get; init; }

    public string? Standard { get; init; }

    // Pipe markers
    public string? ColorSchemeCode { get; init; }

    public string? PipeOdRange { get; init; }

    public decimal? LetterHeightMm { get; init; }

    // Valve tags
    public string? TagShape { get; init; }

    public decimal? DiameterMm { get; init; }

    public decimal? ThicknessMm { get; init; }

    public decimal? HoleSizeMm { get; init; }

    // Safety signs
    public string? SignalWord { get; init; }

    // Labels
    public string? Adhesive { get; init; }

    public string? Finish { get; init; }

    /// <summary>Validates type-specific fields (PT-015). Returns field errors keyed by camelCase property name.</summary>
    public IReadOnlyList<KeyValuePair<string, string>> Validate(ProductType type)
    {
        var errors = new List<KeyValuePair<string, string>>();
        Require(errors, "spec.material", Material);
        switch (type)
        {
            case ProductType.PipeMarker:
                Require(errors, "spec.colorSchemeCode", ColorSchemeCode);
                Require(errors, "spec.pipeOdRange", PipeOdRange);
                break;
            case ProductType.ValveTag:
                Require(errors, "spec.tagShape", TagShape);
                RequirePositive(errors, "spec.diameterMm", DiameterMm);
                RequirePositive(errors, "spec.thicknessMm", ThicknessMm);
                RequirePositive(errors, "spec.holeSizeMm", HoleSizeMm);
                break;
            case ProductType.SafetySign:
                Require(errors, "spec.signalWord", SignalWord);
                RequirePositive(errors, "spec.widthMm", WidthMm);
                RequirePositive(errors, "spec.heightMm", HeightMm);
                break;
            case ProductType.Label:
                RequirePositive(errors, "spec.widthMm", WidthMm);
                RequirePositive(errors, "spec.heightMm", HeightMm);
                break;
            default:
                errors.Add(new("productType", "Unknown product type."));
                break;
        }

        return errors;
    }

    private static void Require(List<KeyValuePair<string, string>> errors, string field, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(new(field, $"{field} is required for this product type."));
        }
    }

    private static void RequirePositive(List<KeyValuePair<string, string>> errors, string field, decimal? value)
    {
        if (value is null or <= 0)
        {
            errors.Add(new(field, $"{field} must be greater than 0 for this product type."));
        }
    }
}
