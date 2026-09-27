using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Common;
using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.SalesOrders;

namespace ProdTrack.Application.SalesOrders;

[RequiresPolicy(Policies.ReadAll)]
public sealed record GetSalesOrderQuery(int Id) : IQuery<SalesOrderDetailModel>;

[RequiresPolicy(Policies.ReadAll)]
public sealed record SearchSalesOrdersQuery(SalesOrderStatus? Status = null, string? Search = null, int? Page = null, int? PageSize = null)
    : IQuery<PagedResult<SalesOrderSummaryModel>>;

internal sealed class GetSalesOrdersHandler(IAppDbContext db)
    : IQueryHandler<GetSalesOrderQuery, SalesOrderDetailModel>,
      IQueryHandler<SearchSalesOrdersQuery, PagedResult<SalesOrderSummaryModel>>
{
    public async Task<Result<SalesOrderDetailModel>> HandleAsync(GetSalesOrderQuery query, CancellationToken cancellationToken)
    {
        var model = await SalesOrderLoader.LoadAsync(db, query.Id, cancellationToken);
        return model is null ? SalesOrderErrors.NotFound(query.Id) : model;
    }

    public async Task<Result<PagedResult<SalesOrderSummaryModel>>> HandleAsync(SearchSalesOrdersQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize) = Paging.Normalize(query.Page, query.PageSize);
        var orders = db.SalesOrders.AsNoTracking();
        if (query.Status is not null)
        {
            orders = orders.Where(s => s.Status == query.Status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            orders = orders.Where(s => s.Number.Contains(term) || s.CustomerName.Contains(term) || (s.PoNumber != null && s.PoNumber.Contains(term)));
        }

        var total = await orders.CountAsync(cancellationToken);
        var items = await orders
            .OrderBy(s => s.DueDate).ThenBy(s => s.Number)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SalesOrderSummaryModel(
                s.Id,
                s.Number,
                s.CustomerName,
                s.PoNumber,
                s.DueDate,
                s.Status,
                s.Lines.Count,
                db.WorkOrders.Count(w => w.SalesOrderLineId != null && s.Lines.Select(l => (int?)l.Id).Contains(w.SalesOrderLineId))))
            .ToListAsync(cancellationToken);
        return new PagedResult<SalesOrderSummaryModel>(items, page, pageSize, total);
    }
}
