using ProdTrack.Domain.Common;

namespace ProdTrack.Domain.SalesOrders;

/// <summary>Customer sales order with lines (PT-018). Each line becomes one work order (PT-019).</summary>
public sealed class SalesOrder : AggregateRoot
{
    public const int NumberMaxLength = 20;
    public const int CustomerMaxLength = 200;
    public const int PoMaxLength = 50;

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

    public static Result<SalesOrder> Create(
        string number,
        string customerName,
        string? poNumber,
        DateOnly dueDate,
        IReadOnlyList<SalesOrderLineDefinition> lines,
        DateTimeOffset nowUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(number);
        ArgumentNullException.ThrowIfNull(lines);
        var order = new SalesOrder { Number = number, Status = SalesOrderStatus.Open, CreatedAtUtc = nowUtc };
        var header = order.ApplyHeader(customerName, poNumber, dueDate);
        if (header.IsFailure)
        {
            return header.Error!;
        }

        if (lines.Count == 0)
        {
            return SalesOrderErrors.NoLines;
        }

        var errors = new List<KeyValuePair<string, string>>();
        foreach (var (definition, index) in lines.Select((l, i) => (l, i)))
        {
            var line = SalesOrderLine.Create(order.NextLineNumber(), definition, $"lines[{index}]", errors);
            if (line is not null)
            {
                order._lines.Add(line);
            }
        }

        return errors.Count > 0 ? errors.ToValidationError() : order;
    }

    /// <summary>
    /// Replaces header and lines. Lines missing from <paramref name="lines"/> are removed; lines listed in
    /// <paramref name="lineIdsWithWorkOrders"/> are locked and must stay unchanged.
    /// </summary>
    public Result Update(
        string customerName,
        string? poNumber,
        DateOnly dueDate,
        IReadOnlyList<SalesOrderLineDefinition> lines,
        IReadOnlySet<int> lineIdsWithWorkOrders)
    {
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentNullException.ThrowIfNull(lineIdsWithWorkOrders);
        if (Status != SalesOrderStatus.Open)
        {
            return SalesOrderErrors.NotOpen(Status);
        }

        if (lines.Count == 0)
        {
            return SalesOrderErrors.NoLines;
        }

        foreach (var existing in _lines)
        {
            var incoming = lines.FirstOrDefault(l => l.LineId == existing.Id);
            if (lineIdsWithWorkOrders.Contains(existing.Id) && (incoming is null || !existing.IsSameAs(incoming)))
            {
                return SalesOrderErrors.LineHasWorkOrder(existing.LineNumber);
            }
        }

        var unknown = lines.FirstOrDefault(l => l.LineId is not null && _lines.TrueForAll(x => x.Id != l.LineId));
        if (unknown is not null)
        {
            return SalesOrderErrors.LineNotFound(unknown.LineId!.Value);
        }

        var header = ApplyHeader(customerName, poNumber, dueDate);
        if (header.IsFailure)
        {
            return header;
        }

        var errors = new List<KeyValuePair<string, string>>();
        var keep = new List<SalesOrderLine>();
        foreach (var (definition, index) in lines.Select((l, i) => (l, i)))
        {
            var field = $"lines[{index}]";
            if (definition.LineId is { } id)
            {
                var line = _lines.First(l => l.Id == id);
                if (!lineIdsWithWorkOrders.Contains(id))
                {
                    line.Apply(definition, field, errors);
                }

                keep.Add(line);
            }
            else
            {
                var created = SalesOrderLine.Create(NextLineNumber(keep), definition, field, errors);
                if (created is not null)
                {
                    keep.Add(created);
                }
            }
        }

        if (errors.Count > 0)
        {
            return errors.ToValidationError();
        }

        _lines.RemoveAll(l => !keep.Contains(l));
        _lines.AddRange(keep.Where(l => !_lines.Contains(l)));
        return Result.Success();
    }

    /// <summary>Closes the order once every line has been turned into a work order.</summary>
    public Result Close()
    {
        if (Status != SalesOrderStatus.Open)
        {
            return SalesOrderErrors.NotOpen(Status);
        }

        Status = SalesOrderStatus.Closed;
        return Result.Success();
    }

    public Result Cancel(bool hasOpenWorkOrders)
    {
        if (Status != SalesOrderStatus.Open)
        {
            return SalesOrderErrors.NotOpen(Status);
        }

        if (hasOpenWorkOrders)
        {
            return SalesOrderErrors.HasWorkOrders;
        }

        Status = SalesOrderStatus.Cancelled;
        return Result.Success();
    }

    private int NextLineNumber(IEnumerable<SalesOrderLine>? among = null)
    {
        var lines = (among ?? _lines).ToList();
        var max = Math.Max(lines.Count == 0 ? 0 : lines.Max(l => l.LineNumber), _lines.Count == 0 ? 0 : _lines.Max(l => l.LineNumber));
        return max + 10;
    }

    private Result ApplyHeader(string customerName, string? poNumber, DateOnly dueDate)
    {
        var errors = new List<KeyValuePair<string, string>>();
        var customer = Guard.Required(customerName, CustomerMaxLength, "customerName", errors);
        var po = string.IsNullOrWhiteSpace(poNumber) ? null : poNumber.Trim();
        if (po?.Length > PoMaxLength)
        {
            errors.Add(new("poNumber", $"PO number must be at most {PoMaxLength} characters."));
        }

        if (errors.Count > 0)
        {
            return errors.ToValidationError();
        }

        CustomerName = customer;
        PoNumber = po;
        DueDate = dueDate;
        return Result.Success();
    }
}
