using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Application.Security;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.Products;

namespace ProdTrack.Application.Quality;

/// <summary>QC checklist templates per product type (PT-035).</summary>
[RequiresPolicy(Policies.ReadAll)]
public sealed record GetQcTemplatesQuery : IQuery<IReadOnlyList<QcTemplateModel>>;

internal sealed class GetQcTemplatesHandler(IAppDbContext db) : IQueryHandler<GetQcTemplatesQuery, IReadOnlyList<QcTemplateModel>>
{
    public async Task<Result<IReadOnlyList<QcTemplateModel>>> HandleAsync(GetQcTemplatesQuery query, CancellationToken cancellationToken)
    {
        var templates = await db.QcChecklistTemplates.AsNoTracking().Include(t => t.Items).ToListAsync(cancellationToken);
        return templates.OrderBy(t => t.ProductType).ThenByDescending(t => t.IsActive).Select(QcTemplateModel.From).ToList();
    }
}

internal static class QcTemplateQueries
{
    public static async Task<QcTemplateModel?> ActiveForAsync(IAppDbContext db, ProductType productType, CancellationToken cancellationToken)
    {
        var template = await db.QcChecklistTemplates.AsNoTracking()
            .Include(t => t.Items)
            .Where(t => t.ProductType == productType && t.IsActive)
            .OrderByDescending(t => t.Id)
            .FirstOrDefaultAsync(cancellationToken);
        return template is null ? null : QcTemplateModel.From(template);
    }
}
