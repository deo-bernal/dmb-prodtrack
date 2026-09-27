using ProdTrack.Domain.Common;

namespace ProdTrack.Domain.Products;

public static class ProductErrors
{
    public static Error NotFound(int id) => Error.NotFound("Product.NotFound", $"Product {id} was not found.");

    public static ValidationError DuplicateSku(string sku) => Error.Validation("sku", $"SKU '{sku}' already exists.");

    public static ValidationError UnknownColorScheme(string code) =>
        Error.Validation("spec.colorSchemeCode", $"Color scheme '{code}' is not in the reference list (BR-15).");

    public static ValidationError UnknownSignalWord(string word) =>
        Error.Validation("spec.signalWord", $"Signal word '{word}' is not in the reference list.");

    public static Error Inactive(string sku) => Error.BusinessRule("Product.Inactive", $"Product '{sku}' is inactive.");
}
