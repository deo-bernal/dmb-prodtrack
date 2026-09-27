using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;
using ProdTrack.Domain.Products;

namespace ProdTrack.Application.Products.UpdateProduct;

[RequiresPolicy(Policies.ManageProductsAndRoutings)]
public sealed record UpdateProductCommand(int Id, string Name, bool RequiresArtworkApproval, ProductSpec Spec, bool IsActive) : ICommand<ProductModel>;
