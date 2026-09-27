using ProdTrack.Domain.Common;
using ProdTrack.Domain.Products;

namespace ProdTrack.Domain.Quality;

/// <summary>QC checklist template per product type (schema only; behaviour arrives with PT-035).</summary>
public sealed class QcChecklistTemplate : AggregateRoot
{
    private readonly List<QcChecklistItem> _items = [];

    private QcChecklistTemplate()
    {
    }

    public ProductType ProductType { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public bool IsActive { get; private set; } = true;

    public IReadOnlyList<QcChecklistItem> Items => _items;
}
