using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;
using ProdTrack.Domain.Products;

namespace ProdTrack.Application.Products.GetProducts;

[RequiresPolicy(Policies.ReadAll)]
public sealed record GetProductsQuery(ProductType? ProductType = null, string? Search = null, bool IncludeInactive = false) : IQuery<IReadOnlyList<ProductModel>>;
