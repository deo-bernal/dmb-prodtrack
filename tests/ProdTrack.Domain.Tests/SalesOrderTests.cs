using ProdTrack.Domain.Common;
using ProdTrack.Domain.Products;
using ProdTrack.Domain.SalesOrders;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Domain.Tests;

[Trait("Category", "Unit")]
public class SalesOrderTests
{
    private static SalesOrder Order(params SalesOrderLineDefinition[] lines) =>
        SalesOrder.Create("SO-2026-00001", "ACME Refinery", "PO-77", TestData.Today.AddDays(10), lines, TestData.Now).Value;

    private static SalesOrderLineDefinition Line(Product product, int qty = 50, string? legend = "STEAM", int? lineId = null, ProductSpec? spec = null) =>
        new(lineId, product, qty, legend, spec);

    [Fact]
    [Trait("Story", "PT-018")]
    public void Create_numbers_lines_by_ten_and_uses_product_spec_by_default()
    {
        var pm = TestData.PipeMarker();
        var order = Order(Line(pm), Line(TestData.SafetySign(), 5, null));

        order.Status.Should().Be(SalesOrderStatus.Open);
        order.Lines.Select(l => l.LineNumber).Should().Equal(10, 20);
        order.Lines[0].Spec.Should().Be(pm.DefaultSpec);
    }

    [Fact]
    [Trait("Story", "PT-018")]
    public void Line_spec_override_replaces_catalog_spec()
    {
        var spec = TestData.PipeMarkerSpec with { PipeOdRange = "3-4 in" };

        var order = Order(Line(TestData.PipeMarker(), spec: spec));

        order.Lines[0].Spec.PipeOdRange.Should().Be("3-4 in");
    }

    [Fact]
    [Trait("Story", "PT-018")]
    public void Create_requires_lines_customer_and_pipe_marker_legend()
    {
        SalesOrder.Create("SO-1", "ACME", null, TestData.Today, [], TestData.Now).Error.Should().BeOfType<ValidationError>();
        SalesOrder.Create("SO-1", " ", null, TestData.Today, [Line(TestData.PipeMarker())], TestData.Now).IsFailure.Should().BeTrue();

        var noLegend = SalesOrder.Create("SO-1", "ACME", null, TestData.Today, [Line(TestData.PipeMarker(), legend: null)], TestData.Now);

        noLegend.Error.Should().BeOfType<ValidationError>().Which.Errors.Keys.Should().Contain(k => k.StartsWith("lines[0]", StringComparison.Ordinal));
    }

    [Fact]
    [Trait("Story", "PT-018")]
    public void Create_rejects_non_positive_quantity()
    {
        var result = SalesOrder.Create("SO-1", "ACME", null, TestData.Today, [Line(TestData.PipeMarker(), qty: 0)], TestData.Now);

        result.Error.Should().BeOfType<ValidationError>();
    }

    [Fact]
    [Trait("Story", "PT-018")]
    public void Update_cannot_change_a_line_that_has_a_work_order()
    {
        var pm = TestData.PipeMarker();
        var order = Order(Line(pm));
        TestData.WithId(order.Lines[0], 5);

        var result = order.Update("ACME", null, TestData.Today, [Line(pm, qty: 99, lineId: 5)], new HashSet<int> { 5 });

        result.Error!.Code.Should().Be("SalesOrder.LineHasWorkOrder");
    }

    [Fact]
    [Trait("Story", "PT-018")]
    public void Update_changes_unlocked_lines_adds_and_removes()
    {
        var pm = TestData.PipeMarker();
        var order = Order(Line(pm), Line(pm, 10, "WATER"));
        TestData.WithId(order.Lines[0], 1);
        TestData.WithId(order.Lines[1], 2);

        var result = order.Update("ACME 2", "PO-78", TestData.Today.AddDays(3), [Line(pm, 75, "STEAM", 1), Line(TestData.SafetySign(), 3, null)], new HashSet<int>());

        result.IsSuccess.Should().BeTrue();
        order.CustomerName.Should().Be("ACME 2");
        order.Lines.Should().HaveCount(2);
        order.Lines.Single(l => l.Id == 1).Quantity.Should().Be(75);
        order.Lines.Should().NotContain(l => l.Id == 2);
        order.Lines.Select(l => l.LineNumber).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    [Trait("Story", "PT-018")]
    public void Cancel_is_blocked_by_open_work_orders_and_only_from_open()
    {
        var order = Order(Line(TestData.PipeMarker()));

        order.Cancel(hasOpenWorkOrders: true).Error!.Code.Should().Be("SalesOrder.HasWorkOrders");
        order.Cancel(hasOpenWorkOrders: false).IsSuccess.Should().BeTrue();
        order.Status.Should().Be(SalesOrderStatus.Cancelled);
        order.Update("X", null, TestData.Today, [Line(TestData.PipeMarker())], new HashSet<int>()).Error!.Code.Should().Be("SalesOrder.NotOpen");
    }

    [Fact]
    [Trait("Story", "PT-019")]
    public void Work_order_from_line_copies_quantity_legend_spec_customer_and_due_date()
    {
        var pm = TestData.PipeMarker();
        var spec = TestData.PipeMarkerSpec with { PipeOdRange = "6 in" };
        var order = Order(Line(pm, 40, "HOT WATER", spec: spec));

        var wo = WorkOrder.CreateFromSalesOrderLine("WO-2026-000009", pm, order, order.Lines[0], 2, TestData.Now).Value;

        wo.Quantity.Should().Be(40);
        wo.Legend.Should().Be("HOT WATER");
        wo.Spec.PipeOdRange.Should().Be("6 in");
        wo.CustomerName.Should().Be("ACME Refinery");
        wo.DueDate.Should().Be(order.DueDate);
        wo.Priority.Should().Be(2);
        wo.Status.Should().Be(WorkOrderStatus.Draft);
    }
}
