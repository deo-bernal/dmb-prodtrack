using ProdTrack.Domain.Common;

namespace ProdTrack.Domain.WorkOrders;

public sealed record WorkOrderCreated(string Number) : IDomainEvent;

public sealed record WorkOrderReleased(int WorkOrderId, string Number) : IDomainEvent;

public sealed record WorkOrderUpdated(int WorkOrderId, string Number, WorkOrderStatus Status) : IDomainEvent;
