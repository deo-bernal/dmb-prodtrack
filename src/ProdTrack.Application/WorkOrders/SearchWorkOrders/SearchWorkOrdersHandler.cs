using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Common;
using ProdTrack.Application.Messaging;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Application.WorkOrders.SearchWorkOrders;

internal sealed class SearchWorkOrdersHandler(IAppDbContext db, IPlantClock clock)
    : IQueryHandler<SearchWorkOrdersQuery, PagedResult<WorkOrderSummaryModel>>
{
    public async Task<Result<PagedResult<WorkOrderSummaryModel>>> HandleAsync(SearchWorkOrdersQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize) = Paging.Normalize(query.Page, query.PageSize);
        var today = clock.Today;

        var workOrders = db.WorkOrders.AsNoTracking();
        if (query.Status is not null)
        {
            workOrders = workOrders.Where(w => w.Status == query.Status);
        }

        if (query.DueBefore is not null)
        {
            workOrders = workOrders.Where(w => w.DueDate < query.DueBefore);
        }

        if (query.LateOnly)
        {
            workOrders = workOrders.Where(w => w.DueDate < today
                && w.Status != WorkOrderStatus.Completed && w.Status != WorkOrderStatus.Cancelled);
        }

        var rows = workOrders.Join(
            db.Products.AsNoTracking(),
            w => w.ProductId,
            p => p.Id,
            (w, p) => new { WorkOrder = w, p.Sku, ProductName = p.Name });

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            rows = rows.Where(r => r.WorkOrder.Number.Contains(term)
                || (r.WorkOrder.CustomerName != null && r.WorkOrder.CustomerName.Contains(term))
                || r.Sku.Contains(term));
        }

        var total = await rows.CountAsync(cancellationToken);
        var items = await rows
            .OrderBy(r => r.WorkOrder.DueDate).ThenBy(r => r.WorkOrder.Priority).ThenBy(r => r.WorkOrder.Number)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new WorkOrderSummaryModel(
                r.WorkOrder.Id,
                r.WorkOrder.Number,
                r.Sku,
                r.ProductName,
                r.WorkOrder.CustomerName,
                r.WorkOrder.Status,
                r.WorkOrder.Quantity,
                r.WorkOrder.CompletedQuantity,
                r.WorkOrder.Priority,
                r.WorkOrder.DueDate,
                r.WorkOrder.DueDate < today && r.WorkOrder.Status != WorkOrderStatus.Completed && r.WorkOrder.Status != WorkOrderStatus.Cancelled))
            .ToListAsync(cancellationToken);

        return new PagedResult<WorkOrderSummaryModel>(items, page, pageSize, total);
    }
}
