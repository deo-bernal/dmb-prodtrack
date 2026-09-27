using ProdTrack.Domain.Common;

namespace ProdTrack.Domain.SalesOrders;

/// <summary>Customer sales order (schema only in Sprint 0-1; behaviour arrives with PT-018/PT-019).</summary>
public sealed class SalesOrder : AggregateRoot
{
    private readonly List<SalesOrderLine> _lines = [];

    private SalesOrder()
    {
    }

    public string Number { get; private set; } = string.Empty;

    public string CustomerName { get; private set; } = string.Empty;

    public string? PoNumber { get; private set; }

    public DateOnly DueDate { get; private set; }

    public SalesOrderStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public IReadOnlyList<SalesOrderLine> Lines => _lines;
}
