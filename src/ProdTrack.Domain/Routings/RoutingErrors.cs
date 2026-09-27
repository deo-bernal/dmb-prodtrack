using ProdTrack.Domain.Common;

namespace ProdTrack.Domain.Routings;

public static class RoutingErrors
{
    public static Error NotFound(int id) => Error.NotFound("Routing.NotFound", $"Routing {id} was not found.");

    public static ValidationError NoSteps => Error.Validation("steps", "A routing needs at least one step.");

    public static ValidationError DuplicateSequence(int sequence) =>
        Error.Validation("steps", $"Step sequence {sequence} is used more than once.");

    public static ValidationError InvalidSequence => Error.Validation("steps", "Step sequences must be greater than 0.");

    public static ValidationError NegativeMinutes => Error.Validation("steps", "Setup and standard minutes cannot be negative.");

    public static Error NoCurrentRouting(string productType) =>
        Error.BusinessRule("Routing.NoCurrentRouting", $"No current routing exists for product type {productType}.");
}
