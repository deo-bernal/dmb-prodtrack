using ProdTrack.Domain.Common;
using ProdTrack.Domain.Products;

namespace ProdTrack.Domain.Quality;

/// <summary>QC checklist template per product type (PT-035). The active template is used for new inspections.</summary>
public sealed class QcChecklistTemplate : AggregateRoot
{
    public const int NameMaxLength = 100;

    private readonly List<QcChecklistItem> _items = [];

    private QcChecklistTemplate()
    {
    }

    public ProductType ProductType { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public bool IsActive { get; private set; } = true;

    public IReadOnlyList<QcChecklistItem> Items => _items;

    public static Result<QcChecklistTemplate> Create(ProductType productType, string name, IReadOnlyList<QcChecklistItemDefinition> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        var errors = new List<KeyValuePair<string, string>>();
        var trimmed = Guard.Required(name, NameMaxLength, "name", errors);
        if (items.Count == 0)
        {
            errors.Add(new("items", "A checklist needs at least one item."));
        }

        foreach (var (item, index) in items.Select((item, index) => (item, index)))
        {
            Guard.Required(item.Description, QcChecklistItem.DescriptionMaxLength, $"items[{index}].description", errors);
            if (item.Kind == QcItemKind.Measured && item.MinValue is null && item.MaxValue is null)
            {
                errors.Add(new($"items[{index}]", "A measured item needs a minimum and/or maximum value."));
            }

            if (item.MinValue > item.MaxValue)
            {
                errors.Add(new($"items[{index}]", "The minimum cannot be greater than the maximum."));
            }
        }

        if (items.GroupBy(i => i.Sequence).Any(g => g.Count() > 1))
        {
            errors.Add(new("items", "Item sequences must be unique."));
        }

        if (errors.Count > 0)
        {
            return errors.ToValidationError();
        }

        var template = new QcChecklistTemplate { ProductType = productType, Name = trimmed };
        template._items.AddRange(items.OrderBy(i => i.Sequence).Select(i => new QcChecklistItem(i)));
        return template;
    }

    public void Deactivate() => IsActive = false;
}
