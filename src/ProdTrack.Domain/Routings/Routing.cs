using ProdTrack.Domain.Common;
using ProdTrack.Domain.Products;
using ProdTrack.Domain.Stations;

namespace ProdTrack.Domain.Routings;

/// <summary>
/// Versioned routing template for a product type. Editing creates a new version; released work orders keep
/// the version they were released with (PT-014).
/// </summary>
public sealed class Routing : AggregateRoot
{
    public const int NameMaxLength = 100;

    private readonly List<RoutingStep> _steps = [];

    private Routing()
    {
    }

    public ProductType ProductType { get; private set; }

    public int Version { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public bool IsCurrent { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public IReadOnlyList<RoutingStep> Steps => _steps;

    public static Result<Routing> Create(
        ProductType productType,
        int version,
        string name,
        IReadOnlyCollection<RoutingStepDefinition> steps,
        DateTimeOffset nowUtc)
    {
        ArgumentNullException.ThrowIfNull(steps);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(version);

        var errors = new List<KeyValuePair<string, string>>();
        var trimmedName = Guard.Required(name, NameMaxLength, "name", errors);
        if (errors.Count > 0)
        {
            return errors.ToValidationError();
        }

        var stepError = ValidateSteps(steps);
        if (stepError is not null)
        {
            return stepError;
        }

        var routing = new Routing
        {
            ProductType = productType,
            Version = version,
            Name = trimmedName,
            IsCurrent = true,
            CreatedAtUtc = nowUtc,
        };
        routing._steps.AddRange(steps
            .OrderBy(s => s.Sequence)
            .Select(s => new RoutingStep(s.Sequence, s.Station.Id, s.SetupMinutes, s.StdMinutesPerUnit, s.AllowOverlap)));
        return routing;
    }

    /// <summary>Marks this version as no longer current (a newer version exists).</summary>
    public void Supersede() => IsCurrent = false;

    private static Error? ValidateSteps(IReadOnlyCollection<RoutingStepDefinition> steps)
    {
        if (steps.Count == 0)
        {
            return RoutingErrors.NoSteps;
        }

        if (steps.Any(s => s.Sequence <= 0))
        {
            return RoutingErrors.InvalidSequence;
        }

        var duplicate = steps.GroupBy(s => s.Sequence).FirstOrDefault(g => g.Count() > 1);
        if (duplicate is not null)
        {
            return RoutingErrors.DuplicateSequence(duplicate.Key);
        }

        if (steps.Any(s => s.SetupMinutes < 0 || s.StdMinutesPerUnit < 0))
        {
            return RoutingErrors.NegativeMinutes;
        }

        var inactive = steps.FirstOrDefault(s => !s.Station.IsActive);
        return inactive is null ? null : StationErrors.Inactive(inactive.Station.Code);
    }
}
