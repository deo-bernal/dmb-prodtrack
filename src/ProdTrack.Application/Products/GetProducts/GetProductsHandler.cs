using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Domain.Common;

namespace ProdTrack.Application.Products.GetProducts;

internal sealed class GetProductsHandler(IAppDbContext db) : IQueryHandler<GetProductsQuery, IReadOnlyList<ProductModel>>
{
    public async Task<Result<IReadOnlyList<ProductModel>>> HandleAsync(GetProductsQuery query, CancellationToken cancellationToken)
    {
        var products = db.Products.AsNoTracking()
            .Where(p => query.IncludeInactive || p.IsActive)
            .Where(p => query.ProductType == null || p.ProductType == query.ProductType);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            products = products.Where(p => p.Sku.Contains(term) || p.Name.Contains(term));
        }

        var items = await products.OrderBy(p => p.Sku)
            .Select(p => new ProductModel(p.Id, p.Sku, p.Name, p.ProductType, p.RequiresArtworkApproval, p.IsActive, p.DefaultSpec))
            .ToListAsync(cancellationToken);
        return items;
    }
}
