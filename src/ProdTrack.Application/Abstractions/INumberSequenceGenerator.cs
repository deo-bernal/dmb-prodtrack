namespace ProdTrack.Application.Abstractions;

public enum NumberSequenceKind
{
    WorkOrder,
    SalesOrder,
}

/// <summary>
/// Generates human-readable business numbers (BR-01): WO-yyyy-nnnnnn, SO-yyyy-nnnnn, reset yearly.
/// The increment is saved in the same unit of work as the entity that uses the number.
/// </summary>
public interface INumberSequenceGenerator
{
    Task<string> NextAsync(NumberSequenceKind kind, CancellationToken cancellationToken);
}
