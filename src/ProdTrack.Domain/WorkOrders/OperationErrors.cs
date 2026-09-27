using ProdTrack.Domain.Common;

namespace ProdTrack.Domain.WorkOrders;

public static class OperationErrors
{
    public static Error NotFound(int id) => Error.NotFound("Operation.NotFound", $"Operation {id} was not found.");

    public static Error InvalidTransition(OperationStatus status, string action) =>
        Error.BusinessRule("Operation.InvalidStatusTransition", $"Cannot {action} an operation that is {status}.");

    public static Error WrongStation(string expectedStationCode) =>
        Error.BusinessRule("Operation.WrongStation", $"This operation belongs to station {expectedStationCode}.");

    public static ValidationError QuantityExceedsInput(int input) =>
        Error.Validation("goodQuantity", $"Good plus scrap quantity cannot exceed the input quantity of {input}.");

    public static ValidationError InvalidQuantity(string field) => Error.Validation(field, "Quantity must be 0 or more.");

    public static ValidationError ScrapQuantity => Error.Validation("quantity", "Scrap quantity must be greater than 0.");

    public static Error InspectionRequired =>
        Error.BusinessRule("Operation.InspectionRequired", "Record a passed QC inspection before completing this operation.");
}
