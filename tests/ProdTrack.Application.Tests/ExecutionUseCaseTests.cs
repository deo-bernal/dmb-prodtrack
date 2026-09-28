using ProdTrack.Application.Operations;
using ProdTrack.Application.Products.GetProducts;
using ProdTrack.Application.Quality;
using ProdTrack.Application.ReasonCodes.GetReasonCodes;
using ProdTrack.Application.SalesOrders;
using ProdTrack.Application.Scanning;
using ProdTrack.Application.Security;
using ProdTrack.Application.WorkOrders;
using ProdTrack.Application.WorkOrders.CreateWorkOrder;
using ProdTrack.Application.WorkOrders.GetWorkOrder;
using ProdTrack.Application.WorkOrders.HoldResumeCancel;
using ProdTrack.Application.WorkOrders.ReleaseWorkOrder;
using ProdTrack.Application.WorkOrders.Traveler;
using ProdTrack.Application.WorkOrders.UpdateWorkOrder;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.Quality;
using ProdTrack.Domain.ReasonCodes;
using ProdTrack.Domain.SalesOrders;
using ProdTrack.Domain.WorkOrders;
using ProdTrack.TestSupport;

namespace ProdTrack.Application.Tests;

[Trait("Category", "Unit")]
public sealed class ExecutionUseCaseTests : IAsyncLifetime
{
    private static readonly DateOnly Today = new(2026, 3, 10);
    private TestApplication _app = null!;

    public async Task InitializeAsync() => _app = await TestApplication.CreateAsync(seedDemoData: true);

    public async Task DisposeAsync() => await _app.DisposeAsync();

    private async Task<int> ProductIdAsync(string sku) =>
        (await _app.QueryAsync(new GetProductsQuery(Search: sku))).Value.Single().Id;

    private async Task<int> ReasonAsync(ReasonCategory category, string code) =>
        (await _app.QueryAsync(new GetReasonCodesQuery(category))).Value.Single(r => r.Code == code).Id;

    /// <summary>Valve tag (no artwork): PREPRESS → ENGRAVE-01 → QC-01 → PACK-01.</summary>
    private async Task<WorkOrderDetailModel> ReleasedValveTagAsync(int quantity = 20)
    {
        var created = (await _app.SendAsync(new CreateWorkOrderCommand(await ProductIdAsync("VT-BRASS-RND-38"), quantity, Today.AddDays(5), 3, "ACME", null))).Value;
        (await _app.SendAsync(new ReleaseWorkOrderCommand(created.Id))).IsSuccess.Should().BeTrue();
        return await DetailAsync(created.Id);
    }

    private async Task<WorkOrderDetailModel> DetailAsync(int id) => (await _app.QueryAsync(new GetWorkOrderQuery(id))).Value;

    private async Task RunAsync(int operationId, int good)
    {
        (await _app.SendAsync(new StartOperationCommand(operationId))).IsSuccess.Should().BeTrue();
        var completed = await _app.SendAsync(new CompleteOperationCommand(operationId, good));
        completed.IsSuccess.Should().BeTrue(completed.Error?.Message);
    }

    [Fact]
    [Trait("Story", "PT-018")]
    public async Task Sales_order_lines_become_draft_work_orders_once()
    {
        var pm = await ProductIdAsync("PM-VIN-FLAM-2");
        var vt = await ProductIdAsync("VT-BRASS-RND-38");
        var so = (await _app.SendAsync(new CreateSalesOrderCommand("ACME Refinery", "PO-1", Today.AddDays(20),
            [new SalesOrderLineInput(null, pm, 30, "STEAM"), new SalesOrderLineInput(null, vt, 12, null)]))).Value;

        var first = (await _app.SendAsync(new CreateWorkOrdersFromSalesOrderCommand(so.Id, 2))).Value;
        var second = (await _app.SendAsync(new CreateWorkOrdersFromSalesOrderCommand(so.Id))).Value;

        so.Number.Should().StartWith("SO-2026-");
        first.Created.Should().HaveCount(2);
        second.Created.Should().BeEmpty();
        second.Skipped.Should().HaveCount(2);
        var wo = await DetailAsync(first.Created[0].WorkOrderId);
        wo.SalesOrderNumber.Should().Be(so.Number);
        wo.Legend.Should().Be("STEAM");
        wo.Priority.Should().Be(2);
        var detail = (await _app.QueryAsync(new GetSalesOrderQuery(so.Id))).Value;
        detail.Lines.Should().OnlyContain(l => l.WorkOrderNumber != null);
    }

    [Fact]
    [Trait("Story", "PT-018")]
    public async Task Sales_order_with_open_work_orders_cannot_be_cancelled()
    {
        var so = (await _app.SendAsync(new CreateSalesOrderCommand("ACME", null, Today, [new SalesOrderLineInput(null, await ProductIdAsync("VT-BRASS-RND-38"), 5, null)]))).Value;
        await _app.SendAsync(new CreateWorkOrdersFromSalesOrderCommand(so.Id));

        (await _app.SendAsync(new CancelSalesOrderCommand(so.Id))).Error!.Code.Should().Be("SalesOrder.HasWorkOrders");
    }

    [Fact]
    [Trait("Story", "PT-018")]
    public async Task Viewer_cannot_create_sales_orders()
    {
        _app.User.SetRoles(Roles.Viewer);

        var result = await _app.SendAsync(new CreateSalesOrderCommand("ACME", null, Today, [new SalesOrderLineInput(null, 1, 5, null)]));

        result.Error!.Type.Should().Be(ErrorType.Forbidden);
        (await _app.QueryAsync(new SearchSalesOrdersQuery())).IsSuccess.Should().BeTrue();
    }

    [Fact]
    [Trait("Story", "PT-012")]
    public async Task Stale_expected_version_is_precondition_failed()
    {
        var created = (await _app.SendAsync(new CreateWorkOrderCommand(await ProductIdAsync("VT-BRASS-RND-38"), 5, Today, 3, null, null))).Value;
        var v1 = (await DetailAsync(created.Id)).Version;
        (await _app.SendAsync(new UpdateWorkOrderCommand(created.Id, 6, Today, 3, null, null, v1))).IsSuccess.Should().BeTrue();

        var stale = await _app.SendAsync(new UpdateWorkOrderCommand(created.Id, 7, Today, 3, null, null, v1));

        stale.Error!.Type.Should().Be(ErrorType.PreconditionFailed);
        (await DetailAsync(created.Id)).Quantity.Should().Be(6);
    }

    [Fact]
    [Trait("Story", "PT-022")]
    public async Task Hold_resume_and_cancel_with_reason_codes()
    {
        var wo = await ReleasedValveTagAsync();
        var hold = await ReasonAsync(ReasonCategory.Hold, "CUSTOMER-CHANGE");

        (await _app.SendAsync(new HoldWorkOrderCommand(wo.Id, hold, "waiting on customer"))).IsSuccess.Should().BeTrue();
        var held = await DetailAsync(wo.Id);
        (await _app.SendAsync(new StartOperationCommand(held.Operations[0].Id))).Error!.Code.Should().Be("WorkOrder.OnHold");
        (await _app.SendAsync(new ResumeWorkOrderCommand(wo.Id))).IsSuccess.Should().BeTrue();
        (await _app.SendAsync(new CancelWorkOrderCommand(wo.Id, "order withdrawn"))).IsSuccess.Should().BeTrue();

        held.Status.Should().Be(WorkOrderStatus.OnHold);
        held.HoldReason.Should().Contain("waiting on customer");
        var cancelled = await DetailAsync(wo.Id);
        cancelled.Status.Should().Be(WorkOrderStatus.Cancelled);
        cancelled.CancelReason.Should().Be("order withdrawn");
    }

    [Fact]
    [Trait("Story", "PT-022")]
    public async Task Operator_cannot_hold_a_work_order()
    {
        var wo = await ReleasedValveTagAsync();
        var hold = await ReasonAsync(ReasonCategory.Hold, "CUSTOMER-CHANGE");
        _app.User.SetRoles(Roles.Operator);

        (await _app.SendAsync(new HoldWorkOrderCommand(wo.Id, hold, null))).Error!.Type.Should().Be(ErrorType.Forbidden);
    }

    [Fact]
    [Trait("Story", "PT-031")]
    public async Task Station_queue_lists_ready_work_and_start_checks_station()
    {
        var wo = await ReleasedValveTagAsync();

        var queue = (await _app.QueryAsync(new GetStationQueueQuery("PREPRESS"))).Value;
        var wrong = await _app.SendAsync(new StartOperationCommand(wo.Operations[0].Id, "PACK-01"));

        queue.Items.Should().Contain(i => i.WorkOrderId == wo.Id && i.Status == OperationStatus.Ready);
        wrong.Error!.Code.Should().Be("Operation.WrongStation");
    }

    [Fact]
    [Trait("Story", "PT-031")]
    public async Task Start_with_same_request_id_is_idempotent()
    {
        var wo = await ReleasedValveTagAsync();
        var requestId = Guid.NewGuid();

        (await _app.SendAsync(new StartOperationCommand(wo.Operations[0].Id, null, requestId))).IsSuccess.Should().BeTrue();
        (await _app.SendAsync(new StartOperationCommand(wo.Operations[0].Id, null, requestId))).IsSuccess.Should().BeTrue();
    }

    [Fact]
    [Trait("Story", "PT-032")]
    public async Task Running_steps_moves_quantities_and_logs_scrap()
    {
        var wo = await ReleasedValveTagAsync(20);
        var misprint = await ReasonAsync(ReasonCategory.Scrap, "MISPRINT");
        var first = wo.Operations[0].Id;

        (await _app.SendAsync(new StartOperationCommand(first))).IsSuccess.Should().BeTrue();
        (await _app.SendAsync(new LogScrapCommand(first, 2, misprint, "bad proof"))).IsSuccess.Should().BeTrue();
        (await _app.SendAsync(new CompleteOperationCommand(first, 18))).IsSuccess.Should().BeTrue();

        var detail = await DetailAsync(wo.Id);
        detail.Status.Should().Be(WorkOrderStatus.InProgress);
        detail.Operations[0].ScrapQuantity.Should().Be(2);
        detail.Operations[1].Status.Should().Be(OperationStatus.Ready);
        detail.Operations[1].InputQuantity.Should().Be(18);
        var op = (await _app.QueryAsync(new GetOperationQuery(first))).Value;
        op.Scrap.Should().ContainSingle().Which.ReasonCode.Should().Be("MISPRINT");
        _app.Notifier.OperationNotifications.Should().Contain(n => n.OperationId == first && n.Status == "Completed");
    }

    [Fact]
    [Trait("Story", "PT-034")]
    public async Task Qc_pass_completes_inspection_step_and_fail_holds_work_order()
    {
        var passWo = await ReleasedValveTagAsync(10);
        await RunAsync(passWo.Operations[0].Id, 10);
        await RunAsync(passWo.Operations[1].Id, 10);
        var qcOp = passWo.Operations[2].Id;
        (await _app.SendAsync(new StartOperationCommand(qcOp))).IsSuccess.Should().BeTrue();
        var checklist = (await _app.QueryAsync(new GetOperationQuery(qcOp))).Value.Checklist!;

        (await _app.SendAsync(new CompleteOperationCommand(qcOp, 10))).Error!.Code.Should().Be("Operation.InspectionRequired");
        var passed = await _app.SendAsync(new RecordInspectionCommand(qcOp, 3, Answers(checklist, pass: true), null));

        passed.Value.Result.Should().Be(QcResult.Passed);
        var afterPass = await DetailAsync(passWo.Id);
        afterPass.Operations[2].Status.Should().Be(OperationStatus.Completed);
        afterPass.Operations[3].Status.Should().Be(OperationStatus.Ready);

        var failWo = await ReleasedValveTagAsync(10);
        await RunAsync(failWo.Operations[0].Id, 10);
        await RunAsync(failWo.Operations[1].Id, 10);
        await _app.SendAsync(new StartOperationCommand(failWo.Operations[2].Id));
        var failed = await _app.SendAsync(new RecordInspectionCommand(failWo.Operations[2].Id, 3, Answers(checklist, pass: false), "engraving shallow"));

        failed.Value.Result.Should().Be(QcResult.Failed);
        var afterFail = await DetailAsync(failWo.Id);
        afterFail.Status.Should().Be(WorkOrderStatus.OnHold);
        afterFail.HoldReason.Should().Contain("QC-FAIL");
    }

    private static List<QcItemResultInput> Answers(QcTemplateModel checklist, bool pass) =>
        [.. checklist.Items.Select(i => i.Kind == QcItemKind.Measured
            ? new QcItemResultInput(i.Id, null, pass ? ((i.MinValue ?? 0) + (i.MaxValue ?? 0)) / 2 : (i.MaxValue ?? 0) + 100)
            : new QcItemResultInput(i.Id, pass, null))];

    [Fact]
    [Trait("Story", "PT-027")]
    public async Task Traveler_has_header_and_operation_qr_codes()
    {
        var wo = await ReleasedValveTagAsync();

        var traveler = (await _app.QueryAsync(new GetTravelerQuery(wo.Id))).Value;

        traveler.HeaderPayload.Should().Be($"WO:{wo.Number}");
        traveler.HeaderQrSvg.Should().Contain("<svg");
        traveler.Operations.Should().HaveCount(4);
        traveler.Operations[0].Payload.Should().Be($"OP:{wo.Number}:{wo.Operations[0].Sequence}");
    }

    [Fact]
    [Trait("Story", "PT-028")]
    public async Task Scan_resolves_work_order_operation_and_station_codes()
    {
        var wo = await ReleasedValveTagAsync();

        var byWo = (await _app.QueryAsync(new ResolveScanCodeQuery($"WO:{wo.Number}"))).Value;
        var byOp = (await _app.QueryAsync(new ResolveScanCodeQuery($"OP:{wo.Number}:{wo.Operations[1].Sequence}"))).Value;
        var bare = (await _app.QueryAsync(new ResolveScanCodeQuery(wo.Number.ToLowerInvariant()))).Value;
        var station = (await _app.QueryAsync(new ResolveScanCodeQuery("ST:QC-01"))).Value;
        var unknown = await _app.QueryAsync(new ResolveScanCodeQuery("WO:WO-2026-999999"));

        byWo.WorkOrderId.Should().Be(wo.Id);
        byOp.OperationId.Should().Be(wo.Operations[1].Id);
        bare.Kind.Should().Be(ScanKind.WorkOrder);
        station.StationCode.Should().Be("QC-01");
        unknown.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Theory]
    [Trait("Story", "PT-028")]
    [InlineData("WO:WO-2026-000001", ScanKind.WorkOrder, "WO-2026-000001", null)]
    [InlineData("OP:WO-2026-000001:30", ScanKind.Operation, "WO-2026-000001", 30)]
    [InlineData("st:pack-01", ScanKind.Station, "PACK-01", null)]
    [InlineData(" WO-2026-000001:20 ", ScanKind.Operation, "WO-2026-000001", 20)]
    public void Scan_parser_accepts_payloads_and_manual_entry(string raw, ScanKind kind, string value, int? sequence)
    {
        var parsed = ScanCodeParser.Parse(raw);

        parsed.Value.Should().Be(new ScanCode(kind, value, sequence));
    }

    [Theory]
    [Trait("Story", "PT-028")]
    [InlineData("")]
    [InlineData("OP:WO-2026-000001:abc")]
    [InlineData("hello world")]
    public void Scan_parser_rejects_garbage(string raw) =>
        ScanCodeParser.Parse(raw).IsFailure.Should().BeTrue();
}
