using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Domain.Products;
using ProdTrack.Domain.SalesOrders;

namespace ProdTrack.Application.SalesOrders;

internal static class SalesOrderLoader
{
    public static async Task<SalesOrderDetailModel?> LoadAsync(IAppDbContext db, int id, CancellationToken cancellationToken)
    {
        var order = await db.SalesOrders.AsNoTracking().Include(s => s.Lines).FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (order is null)
        {
            return null;
        }

        var productIds = order.Lines.Select(l => l.ProductId).Distinct().ToList();
        var products = await db.Products.AsNoTracking().Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, cancellationToken);
        var lineIds = order.Lines.Select(l => (int?)l.Id).ToList();
        var workOrders = await db.WorkOrders.AsNoTracking()
            .Where(w => lineIds.Contains(w.SalesOrderLineId))
            .Select(w => new { w.Id, w.Number, w.Status, w.SalesOrderLineId })
            .ToListAsync(cancellationToken);

        return new SalesOrderDetailModel(
            order.Id,
            order.Number,
            order.CustomerName,
            order.PoNumber,
            order.DueDate,
            order.Status,
            order.CreatedAtUtc,
            [.. order.Lines.OrderBy(l => l.LineNumber).Select(l =>
            {
                var product = products[l.ProductId];
                var wo = workOrders.FirstOrDefault(w => w.SalesOrderLineId == l.Id);
                return new SalesOrderLineModel(
                    l.Id, l.LineNumber, l.ProductId, product.Sku, product.Name, product.ProductType, l.Quantity, l.Legend, l.Spec, wo?.Id, wo?.Number, wo?.Status.ToString());
            })],
            order.RowVersion);
    }

    /// <summary>Loads the products referenced by the lines and converts inputs to domain definitions.</summary>
    public static async Task<(List<SalesOrderLineDefinition>? Definitions, Domain.Common.Error? Error)> ToDefinitionsAsync(
        IAppDbContext db,
        IReadOnlyList<SalesOrderLineInput> lines,
        CancellationToken cancellationToken)
    {
        var ids = lines.Select(l => l.ProductId).Distinct().ToList();
        var products = await db.Products.Where(p => ids.Contains(p.Id)).ToDictionaryAsync(p => p.Id, cancellationToken);
        var definitions = new List<SalesOrderLineDefinition>();
        foreach (var (line, index) in lines.Select((l, i) => (l, i)))
        {
            if (!products.TryGetValue(line.ProductId, out var product))
            {
                return (null, Domain.Common.Error.Validation($"lines[{index}].productId", ProductErrors.NotFound(line.ProductId).Message));
            }

            definitions.Add(new SalesOrderLineDefinition(line.LineId, product, line.Quantity, line.Legend, line.SpecOverride));
        }

        return (definitions, null);
    }
}
