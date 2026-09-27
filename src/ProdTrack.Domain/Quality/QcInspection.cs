using ProdTrack.Domain.Common;

namespace ProdTrack.Domain.Quality;

/// <summary>
/// QC inspection of an operation (PT-036). Measured items outside tolerance fail automatically; any failed item fails
/// the inspection, which puts the work order on hold (PT-037, rework routing is a later story).
/// </summary>
public sealed class QcInspection : AggregateRoot
{
    public const int NotesMaxLength = 1000;

    private readonly List<QcResultItem> _resultItems = [];

    private QcInspection()
    {
    }

    public int OperationId { get; private set; }

    public int SampleSize { get; private set; }

    public QcResult Result { get; private set; }

    public QcDisposition Disposition { get; private set; }

    public string InspectorId { get; private set; } = string.Empty;

    public string? Notes { get; private set; }

    public DateTimeOffset InspectedAtUtc { get; private set; }

    public IReadOnlyList<QcResultItem> ResultItems => _resultItems;

    public static Result<QcInspection> Record(
        int operationId,
        QcChecklistTemplate template,
        IReadOnlyList<QcItemResultInput> results,
        int sampleSize,
        string inspectorId,
        string? notes,
        DateTimeOffset nowUtc)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(results);
        var errors = new List<KeyValuePair<string, string>>();
        if (sampleSize <= 0)
        {
            errors.Add(new("sampleSize", "Sample size must be greater than 0."));
        }

        var trimmedNotes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        if (trimmedNotes?.Length > NotesMaxLength)
        {
            errors.Add(new("notes", $"Notes must be at most {NotesMaxLength} characters."));
        }

        var inspection = new QcInspection
        {
            OperationId = operationId,
            SampleSize = sampleSize,
            InspectorId = inspectorId,
            Notes = trimmedNotes,
            InspectedAtUtc = nowUtc,
        };

        foreach (var item in template.Items)
        {
            var input = results.FirstOrDefault(r => r.ChecklistItemId == item.Id);
            var field = $"items[{item.Sequence}]";
            if (item.Kind == QcItemKind.Measured)
            {
                if (input?.MeasuredValue is not { } value)
                {
                    errors.Add(new(field, $"Enter a measured value for '{item.Description}'."));
                    continue;
                }

                inspection._resultItems.Add(new QcResultItem(item.Id, item.IsWithinTolerance(value), value));
            }
            else
            {
                if (input?.Passed is not { } passed)
                {
                    errors.Add(new(field, $"Mark '{item.Description}' as pass or fail."));
                    continue;
                }

                inspection._resultItems.Add(new QcResultItem(item.Id, passed, null));
            }
        }

        if (errors.Count > 0)
        {
            return errors.ToValidationError();
        }

        var passedAll = inspection._resultItems.TrueForAll(r => r.Passed);
        inspection.Result = passedAll ? QcResult.Passed : QcResult.Failed;
        inspection.Disposition = passedAll ? QcDisposition.None : QcDisposition.Hold;
        return inspection;
    }
}
