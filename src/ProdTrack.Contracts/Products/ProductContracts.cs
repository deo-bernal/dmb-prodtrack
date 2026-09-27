namespace ProdTrack.Contracts.Products;

/// <summary>Product specification; required fields depend on the product type.</summary>
public sealed record ProductSpecDto
{
    public string? Material { get; init; }

    public decimal? WidthMm { get; init; }

    public decimal? HeightMm { get; init; }

    public string? Mounting { get; init; }

    public string? Standard { get; init; }

    public string? ColorSchemeCode { get; init; }

    public string? PipeOdRange { get; init; }

    public decimal? LetterHeightMm { get; init; }

    public string? TagShape { get; init; }

    public decimal? DiameterMm { get; init; }

    public decimal? ThicknessMm { get; init; }

    public decimal? HoleSizeMm { get; init; }

    public string? SignalWord { get; init; }

    public string? Adhesive { get; init; }

    public string? Finish { get; init; }
}

/// <summary>Catalog product. ProductType: PipeMarker|ValveTag|SafetySign|Label.</summary>
public sealed record ProductDto(int Id, string Sku, string Name, string ProductType, bool RequiresArtworkApproval, bool IsActive, ProductSpecDto Spec, string? Version = null);

public sealed record CreateProductRequest(string Sku, string Name, string ProductType, bool RequiresArtworkApproval, ProductSpecDto Spec);

public sealed record UpdateProductRequest(string Name, bool RequiresArtworkApproval, ProductSpecDto Spec, bool IsActive);
