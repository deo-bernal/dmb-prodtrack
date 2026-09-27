using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.Products;

namespace ProdTrack.Application.Products.GetProduct;

internal sealed class GetProductHandler(IAppDbContext db) : IQueryHandler<GetProductQuery, ProductModel>
{
    public async Task<Result<ProductModel>> HandleAsync(GetProductQuery query, CancellationToken cancellationToken)
    {
        var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == query.Id, cancellationToken);
        return product is null ? ProductErrors.NotFound(query.Id) : ProductModel.From(product);
    }
}
