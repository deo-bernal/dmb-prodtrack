using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;
using ProdTrack.Domain.Products;

namespace ProdTrack.Application.Products.CreateProduct;

[RequiresPolicy(Policies.ManageProductsAndRoutings)]
public sealed record CreateProductCommand(string Sku, string Name, ProductType ProductType, bool RequiresArtworkApproval, ProductSpec Spec) : ICommand<ProductModel>;
