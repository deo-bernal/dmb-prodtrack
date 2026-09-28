using ProdTrack.Domain.Products;
using ProdTrack.Domain.Quality;

namespace ProdTrack.Application.Quality;

public sealed record QcChecklistItemModel(int Id, int Sequence, string Description, QcItemKind Kind, decimal? MinValue, decimal? MaxValue, string? Unit);

public sealed record QcTemplateModel(int Id, ProductType ProductType, string Name, bool IsActive, IReadOnlyList<QcChecklistItemModel> Items)
{
    public static QcTemplateModel From(QcChecklistTemplate t) => new(
        t.Id,
        t.ProductType,
        t.Name,
        t.IsActive,
        [.. t.Items.OrderBy(i => i.Sequence).Select(i => new QcChecklistItemModel(i.Id, i.Sequence, i.Description, i.Kind, i.MinValue, i.MaxValue, i.Unit))]);
}

public sealed record InspectionRecordedModel(int InspectionId, QcResult Result, QcDisposition Disposition);
