using ProdTrack.Domain.Common;

namespace ProdTrack.Domain.Products;

/// <summary>Catalog product (pipe marker, valve tag, safety sign, label) with a default specification.</summary>
public sealed class Product : AggregateRoot
{
    public const int SkuMaxLength = 40;
    public const int NameMaxLength = 200;

    private Product()
    {
    }

    public string Sku { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public ProductType ProductType { get; private set; }

    public bool RequiresArtworkApproval { get; private set; }

    public ProductSpec DefaultSpec { get; private set; } = new();

    public bool IsActive { get; private set; } = true;

    public static Result<Product> Create(string sku, string name, ProductType type, bool requiresArtworkApproval, ProductSpec spec)
    {
        var errors = new List<KeyValuePair<string, string>>();
        var normalizedSku = Guard.Required(sku, SkuMaxLength, "sku", errors).ToUpperInvariant();
        if (!Enum.IsDefined(type))
        {
            errors.Add(new("productType", "Unknown product type."));
            return errors.ToValidationError();
        }

        var product = new Product { Sku = normalizedSku, ProductType = type };
        errors.AddRange(product.Apply(name, requiresArtworkApproval, spec, isActive: true));
        return errors.Count > 0 ? errors.ToValidationError() : product;
    }

    public Result Update(string name, bool requiresArtworkApproval, ProductSpec spec, bool isActive)
    {
        var errors = Apply(name, requiresArtworkApproval, spec, isActive);
        return errors.Count > 0 ? errors.ToValidationError() : Result.Success();
    }

    private List<KeyValuePair<string, string>> Apply(string name, bool requiresArtworkApproval, ProductSpec spec, bool isActive)
    {
        ArgumentNullException.ThrowIfNull(spec);
        var errors = new List<KeyValuePair<string, string>>();
        var trimmedName = Guard.Required(name, NameMaxLength, "name", errors);
        errors.AddRange(spec.Validate(ProductType));
        if (errors.Count == 0)
        {
            Name = trimmedName;
            RequiresArtworkApproval = requiresArtworkApproval;
            DefaultSpec = spec;
            IsActive = isActive;
        }

        return errors;
    }
}
