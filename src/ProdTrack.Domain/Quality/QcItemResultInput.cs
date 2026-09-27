namespace ProdTrack.Domain.Quality;

/// <summary>Inspector input for one checklist item: pass/fail for PassFail items, a value for Measured items.</summary>
public sealed record QcItemResultInput(int ChecklistItemId, bool? Passed, decimal? MeasuredValue);
