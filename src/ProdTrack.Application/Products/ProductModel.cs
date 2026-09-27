using ProdTrack.Domain.Products;

namespace ProdTrack.Application.Products;

public sealed record ProductModel(int Id, string Sku, string Name, ProductType ProductType, bool RequiresArtworkApproval, bool IsActive, ProductSpec Spec)
{
    public static ProductModel From(Product p) => new(p.Id, p.Sku, p.Name, p.ProductType, p.RequiresArtworkApproval, p.IsActive, p.DefaultSpec);
}
