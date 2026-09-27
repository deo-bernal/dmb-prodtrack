using ProdTrack.Domain.Common;

namespace ProdTrack.Domain.WorkOrders;

public static class WorkOrderErrors
{
    public static Error NotFound(int id) => Error.NotFound("WorkOrder.NotFound", $"Work order {id} was not found.");

    public static Error NotFoundByNumber(string number) => Error.NotFound("WorkOrder.NotFound", $"Work order {number} was not found.");

    public static Error ArtworkNotApproved => Error.BusinessRule("WorkOrder.ArtworkNotApproved", "Artwork not approved");

    public static Error InvalidStatusTransition(WorkOrderStatus from, string action) =>
        Error.BusinessRule("WorkOrder.InvalidStatusTransition", $"Cannot {action} a work order in status {from}.");

    public static Error NotEditable(WorkOrderStatus status) =>
        Error.BusinessRule("WorkOrder.NotEditable", $"This field cannot be changed when the work order is {status}.");

    public static Error RoutingProductTypeMismatch =>
        Error.BusinessRule("WorkOrder.RoutingMismatch", "The routing is for a different product type.");

    public static ValidationError LegendRequired => Error.Validation("legend", "Legend text is required for pipe markers.");

    public static ValidationError InvalidQuantity => Error.Validation("quantity", "Quantity must be greater than 0.");

    public static ValidationError InvalidPriority => Error.Validation("priority", "Priority must be between 1 (high) and 5 (low).");

    public static Error ArtworkProofNotFound(int version) =>
        Error.NotFound("WorkOrder.ArtworkProofNotFound", $"Artwork proof version {version} was not found.");

    public static Error ArtworkProofNotPending(int version) =>
        Error.BusinessRule("WorkOrder.ArtworkProofNotPending", $"Artwork proof version {version} is not pending approval.");
}
