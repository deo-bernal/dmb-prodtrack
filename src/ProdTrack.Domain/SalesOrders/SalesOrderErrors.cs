using ProdTrack.Domain.Common;

namespace ProdTrack.Domain.SalesOrders;

public static class SalesOrderErrors
{
    public static Error NotFound(int id) => Error.NotFound("SalesOrder.NotFound", $"Sales order {id} was not found.");

    public static Error NotOpen(SalesOrderStatus status) =>
        Error.BusinessRule("SalesOrder.NotOpen", $"The sales order is {status}; only Open orders can be changed.");

    public static Error LineNotFound(int lineId) => Error.NotFound("SalesOrder.LineNotFound", $"Sales order line {lineId} was not found.");

    public static Error LineHasWorkOrder(int lineNumber) =>
        Error.BusinessRule("SalesOrder.LineHasWorkOrder", $"Line {lineNumber} already has a work order and cannot be changed or removed.");

    public static Error HasWorkOrders =>
        Error.BusinessRule("SalesOrder.HasWorkOrders", "The sales order has open work orders; cancel them first.");

    public static ValidationError NoLines => Error.Validation("lines", "A sales order needs at least one line.");
}
