using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Application.Messaging;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.Products;
using ProdTrack.Domain.ReasonCodes;
using ProdTrack.Domain.Stations;

namespace ProdTrack.Application.Reference.GetReferenceList;

internal sealed class GetReferenceListHandler(IAppDbContext db) : IQueryHandler<GetReferenceListQuery, IReadOnlyList<ReferenceItemModel>>
{
    public async Task<Result<IReadOnlyList<ReferenceItemModel>>> HandleAsync(GetReferenceListQuery query, CancellationToken cancellationToken)
    {
        switch (query.List?.ToLowerInvariant())
        {
            case ReferenceLists.ColorSchemes:
                return await db.ColorSchemes.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.Name)
                    .Select(c => new ReferenceItemModel(c.Code, c.Name, c.TextColor, c.BackgroundColor))
                    .ToListAsync(cancellationToken);
            case ReferenceLists.SignalWords:
                return await db.SignalWords.AsNoTracking().Where(s => s.IsActive).OrderBy(s => s.Id)
                    .Select(s => new ReferenceItemModel(s.Word, s.Word, s.TextColor, s.HeaderColor))
                    .ToListAsync(cancellationToken);
            case ReferenceLists.ProductTypes:
                return FromEnum<ProductType>();
            case ReferenceLists.StationTypes:
                return FromEnum<StationType>();
            case ReferenceLists.ReasonCategories:
                return FromEnum<ReasonCategory>();
            default:
                return Error.NotFound("Reference.UnknownList", $"Unknown reference list '{query.List}'. Known lists: {string.Join(", ", ReferenceLists.All)}.");
        }
    }

    private static List<ReferenceItemModel> FromEnum<TEnum>()
        where TEnum : struct, Enum =>
        [.. Enum.GetNames<TEnum>().Select(n => new ReferenceItemModel(n, n))];
}
