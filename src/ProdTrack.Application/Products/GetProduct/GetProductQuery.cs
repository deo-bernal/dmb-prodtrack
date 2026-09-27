using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;

namespace ProdTrack.Application.Products.GetProduct;

[RequiresPolicy(Policies.ReadAll)]
public sealed record GetProductQuery(int Id) : IQuery<ProductModel>;
