using ProdTrack.Domain.Common;

namespace ProdTrack.Domain.WorkOrders;

public sealed record WorkOrderCreated(string Number) : IDomainEvent;

public sealed record WorkOrderReleased(int WorkOrderId, string Number) : IDomainEvent;

public sealed record WorkOrderUpdated(int WorkOrderId, string Number, WorkOrderStatus Status) : IDomainEvent;

public sealed record OperationChanged(int WorkOrderId, string Number, int OperationId, int Sequence, int StationId, OperationStatus Status) : IDomainEvent;
