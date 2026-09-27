using ProdTrack.Domain.Common;

namespace ProdTrack.Domain.Quality;

public sealed class QcResultItem : Entity
{
    private QcResultItem()
    {
    }

    internal QcResultItem(int checklistItemId, bool passed, decimal? measuredValue)
    {
        ChecklistItemId = checklistItemId;
        Passed = passed;
        MeasuredValue = measuredValue;
    }

    public int InspectionId { get; private set; }

    public int ChecklistItemId { get; private set; }

    public bool Passed { get; private set; }

    public decimal? MeasuredValue { get; private set; }
}
