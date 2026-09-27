using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;

namespace ProdTrack.Infrastructure.Persistence;

/// <summary>
/// Increments the yearly counter in the same unit of work as the entity using the number. Concurrent increments
/// conflict on the row version and surface as 409 (the caller retries).
/// </summary>
internal sealed class NumberSequenceGenerator(AppDbContext db, IPlantClock clock) : INumberSequenceGenerator
{
    public async Task<string> NextAsync(NumberSequenceKind kind, CancellationToken cancellationToken)
    {
        var year = clock.Today.Year;
        var (name, digits) = kind switch
        {
            NumberSequenceKind.WorkOrder => ("WO", 6),
            NumberSequenceKind.SalesOrder => ("SO", 5),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown sequence."),
        };

        var sequence = db.NumberSequences.Local.FirstOrDefault(s => s.Name == name && s.Year == year)
            ?? await db.NumberSequences.FirstOrDefaultAsync(s => s.Name == name && s.Year == year, cancellationToken);
        if (sequence is null)
        {
            sequence = new NumberSequence(name, year);
            db.NumberSequences.Add(sequence);
        }

        var value = sequence.Next();
        return $"{name}-{year}-{value.ToString($"D{digits}", System.Globalization.CultureInfo.InvariantCulture)}";
    }
}
