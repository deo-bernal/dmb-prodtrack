using ProdTrack.Domain.Common;
using ProdTrack.Domain.Products;

namespace ProdTrack.Domain.SalesOrders;

public sealed class SalesOrderLine : Entity
{
    private SalesOrderLine()
    {
    }

    public int SalesOrderId { get; private set; }

    public int LineNumber { get; private set; }

    public int ProductId { get; private set; }

    public int Quantity { get; private set; }

    public string? Legend { get; private set; }

    public ProductSpec Spec { get; private set; } = new();
}
