using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Domain.Common;

namespace ProdTrack.Application.ReasonCodes.GetReasonCodes;

internal sealed class GetReasonCodesHandler(IAppDbContext db) : IQueryHandler<GetReasonCodesQuery, IReadOnlyList<ReasonCodeModel>>
{
    public async Task<Result<IReadOnlyList<ReasonCodeModel>>> HandleAsync(GetReasonCodesQuery query, CancellationToken cancellationToken)
    {
        var items = await db.ReasonCodes.AsNoTracking()
            .Where(r => query.IncludeInactive || r.IsActive)
            .Where(r => query.Category == null || r.Category == query.Category)
            .OrderBy(r => r.Category).ThenBy(r => r.Code)
            .Select(r => new ReasonCodeModel(r.Id, r.Code, r.Description, r.Category, r.IsActive))
            .ToListAsync(cancellationToken);
        return items;
    }
}
