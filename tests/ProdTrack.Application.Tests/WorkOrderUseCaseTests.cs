using System.Text;
using ProdTrack.Application.Dashboard.GetDashboardSummary;
using ProdTrack.Application.Products.GetProducts;
using ProdTrack.Application.Security;
using ProdTrack.Application.WorkOrders;
using ProdTrack.Application.WorkOrders.Artwork;
using ProdTrack.Application.WorkOrders.CreateWorkOrder;
using ProdTrack.Application.WorkOrders.DeleteWorkOrder;
using ProdTrack.Application.WorkOrders.GetWorkOrder;
using ProdTrack.Application.WorkOrders.ReleaseWorkOrder;
using ProdTrack.Application.WorkOrders.SearchWorkOrders;
using ProdTrack.Application.WorkOrders.UpdateWorkOrder;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.WorkOrders;
using ProdTrack.TestSupport;

namespace ProdTrack.Application.Tests;

[Trait("Category", "Unit")]
public sealed class WorkOrderUseCaseTests : IAsyncLifetime
{
    private static readonly DateOnly Today = new(2026, 3, 10);
    private TestApplication _app = null!;

    public async Task InitializeAsync() => _app = await TestApplication.CreateAsync(seedDemoData: true);

    public async Task DisposeAsync() => await _app.DisposeAsync();

    private async Task<int> ProductIdAsync(string sku) =>
        (await _app.QueryAsync(new GetProductsQuery(Search: sku))).Value.Single().Id;

    private async Task<WorkOrderCreatedModel> CreateAsync(string sku = "PM-VIN-FLAM-2", string? legend = "STEAM", int dueInDays = 7)
    {
        var result = await _app.SendAsync(new CreateWorkOrderCommand(await ProductIdAsync(sku), 100, Today.AddDays(dueInDays), 3, "ACME", legend));
        result.IsSuccess.Should().BeTrue(result.IsFailure ? result.Error!.Message : null);
        return result.Value;
    }

    [Fact]
    [Trait("Story", "PT-019")]
    public async Task Create_assigns_sequential_plant_year_numbers()
    {
        var first = await CreateAsync();
        var second = await CreateAsync();

        first.Number.Should().Be("WO-2026-000001");
        second.Number.Should().Be("WO-2026-000002");
    }

    [Fact]
    [Trait("Story", "PT-019")]
    public async Task Create_with_unknown_product_is_a_validation_error()
    {
        var result = await _app.SendAsync(new CreateWorkOrderCommand(9999, 10, Today, 3, null, null));

        result.Error.Should().BeOfType<ValidationError>().Which.Errors.Should().ContainKey("productId");
    }

    [Fact]
    [Trait("Story", "PT-010")]
    public async Task Viewer_cannot_create_but_can_read()
    {
        var created = await CreateAsync();
        _app.User.SetRoles(Roles.Viewer);

        (await _app.SendAsync(new CreateWorkOrderCommand(1, 10, Today, 3, null, "X"))).Error!.Type.Should().Be(ErrorType.Forbidden);
        (await _app.QueryAsync(new GetWorkOrderQuery(created.Id))).IsSuccess.Should().BeTrue();
    }

    [Fact]
    [Trait("Story", "PT-020")]
    public async Task Release_creates_operations_from_current_routing_and_notifies()
    {
        var created = await CreateAsync();

        var result = await _app.SendAsync(new ReleaseWorkOrderCommand(created.Id));

        result.IsSuccess.Should().BeTrue();
        var detail = (await _app.QueryAsync(new GetWorkOrderByNumberQuery(created.Number))).Value;
        detail.Status.Should().Be(WorkOrderStatus.Released);
        detail.RoutingVersion.Should().Be(1);
        detail.Operations.Should().NotBeEmpty();
        detail.Operations[0].Status.Should().Be(OperationStatus.Ready);
        _app.Notifier.Notifications.Should().Contain(n => n.WorkOrderId == created.Id && n.Status == "Released");
    }

    [Fact]
    [Trait("Story", "PT-024")]
    public async Task Artwork_gate_blocks_release_until_proof_is_approved()
    {
        var created = await CreateAsync("SS-ALU-DANGER-A4", legend: null);

        var blocked = await _app.SendAsync(new ReleaseWorkOrderCommand(created.Id));
        blocked.Error!.Code.Should().Be("WorkOrder.ArtworkNotApproved");

        await using var pdf = new MemoryStream(Encoding.ASCII.GetBytes("%PDF-1.7 test proof"));
        var upload = await _app.SendAsync(new UploadArtworkProofCommand(created.Id, "proof.pdf", pdf.Length, pdf));
        upload.IsSuccess.Should().BeTrue();
        upload.Value.Version.Should().Be(1);
        _app.Files.Keys.Should().ContainSingle(k => k.StartsWith($"workorders/{created.Number}/artwork/v1/", StringComparison.Ordinal));

        (await _app.SendAsync(new ApproveArtworkProofCommand(created.Id, 1, "Looks good"))).IsSuccess.Should().BeTrue();
        (await _app.SendAsync(new ReleaseWorkOrderCommand(created.Id))).IsSuccess.Should().BeTrue();
    }

    [Fact]
    [Trait("Story", "PT-023")]
    public async Task Upload_rejects_content_that_does_not_match_the_extension()
    {
        var created = await CreateAsync("SS-ALU-DANGER-A4", legend: null);
        await using var fake = new MemoryStream(Encoding.ASCII.GetBytes("MZ this is not a pdf"));

        var result = await _app.SendAsync(new UploadArtworkProofCommand(created.Id, "proof.pdf", fake.Length, fake));

        result.Error.Should().BeOfType<ValidationError>().Which.Errors.Should().ContainKey("file");
    }

    [Fact]
    [Trait("Story", "PT-023")]
    public async Task Upload_rejects_disallowed_extensions()
    {
        var created = await CreateAsync("SS-ALU-DANGER-A4", legend: null);
        await using var exe = new MemoryStream([0x4D, 0x5A]);

        var result = await _app.SendAsync(new UploadArtworkProofCommand(created.Id, "proof.exe", exe.Length, exe));

        result.Error.Should().BeOfType<ValidationError>();
    }

    [Fact]
    [Trait("Story", "PT-019")]
    public async Task Draft_can_be_updated_and_deleted()
    {
        var created = await CreateAsync();

        (await _app.SendAsync(new UpdateWorkOrderCommand(created.Id, 250, Today.AddDays(3), 1, "ACME", "STEAM"))).IsSuccess.Should().BeTrue();
        (await _app.QueryAsync(new GetWorkOrderQuery(created.Id))).Value.Quantity.Should().Be(250);

        (await _app.SendAsync(new DeleteWorkOrderCommand(created.Id))).IsSuccess.Should().BeTrue();
        (await _app.QueryAsync(new GetWorkOrderQuery(created.Id))).Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    [Trait("Story", "PT-019")]
    public async Task Released_work_order_cannot_be_deleted()
    {
        var created = await CreateAsync();
        await _app.SendAsync(new ReleaseWorkOrderCommand(created.Id));

        var result = await _app.SendAsync(new DeleteWorkOrderCommand(created.Id));

        result.Error!.Type.Should().Be(ErrorType.BusinessRule);
    }

    [Fact]
    [Trait("Story", "PT-021")]
    public async Task Search_filters_by_status_text_and_late_flag()
    {
        await CreateAsync(dueInDays: 5);
        var late = await CreateAsync(dueInDays: 1);
        _app.Clock.Advance(TimeSpan.FromDays(3));

        var lateOnly = (await _app.QueryAsync(new SearchWorkOrdersQuery(LateOnly: true))).Value;
        lateOnly.Items.Should().ContainSingle().Which.Number.Should().Be(late.Number);
        lateOnly.Items[0].IsLate.Should().BeTrue();

        var byText = (await _app.QueryAsync(new SearchWorkOrdersQuery(Search: late.Number))).Value;
        byText.TotalCount.Should().Be(1);

        var drafts = (await _app.QueryAsync(new SearchWorkOrdersQuery(Status: WorkOrderStatus.Draft))).Value;
        drafts.TotalCount.Should().Be(2);
        drafts.PageSize.Should().Be(25);
    }

    [Fact]
    [Trait("Story", "PT-041")]
    public async Task Dashboard_counts_work_orders_and_station_wip()
    {
        var released = await CreateAsync();
        await CreateAsync();
        await _app.SendAsync(new ReleaseWorkOrderCommand(released.Id));

        var summary = (await _app.QueryAsync(new GetDashboardSummaryQuery())).Value;

        summary.WorkOrdersByStatus[WorkOrderStatus.Draft].Should().Be(1);
        summary.WorkOrdersByStatus[WorkOrderStatus.Released].Should().Be(1);
        summary.WipByStation.Sum(s => s.Ready).Should().Be(1);
    }
}
