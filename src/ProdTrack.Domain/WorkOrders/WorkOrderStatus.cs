namespace ProdTrack.Domain.WorkOrders;

/// <summary>Work order lifecycle (BRD 5.5).</summary>
public enum WorkOrderStatus
{
    Draft,
    Released,
    InProgress,
    OnHold,
    Completed,
    Cancelled,
}
