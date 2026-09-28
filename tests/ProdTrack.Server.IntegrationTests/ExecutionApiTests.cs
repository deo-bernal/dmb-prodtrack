using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using ProdTrack.Application.Security;
using ProdTrack.Contracts.Operations;
using ProdTrack.Contracts.Products;
using ProdTrack.Contracts.ReasonCodes;
using ProdTrack.Contracts.SalesOrders;
using ProdTrack.Contracts.Users;
using ProdTrack.Contracts.WorkOrders;
using ProdTrack.Domain.Common;
using ProdTrack.Server.Errors;
using ProdTrack.TestSupport;

namespace ProdTrack.Server.IntegrationTests;

[Collection(ServerCollection.Name)]
[Trait("Category", "Integration")]
public sealed class ExecutionApiTests(ProdTrackFactory factory)
{
    private static async Task<int> ProductIdAsync(HttpClient client, string sku) =>
        (await client.GetFromJsonAsync<List<ProductDto>>($"/api/v1/products?search={sku}"))!.Single().Id;

    private static async Task<int> ReasonAsync(HttpClient client, string category, string code) =>
        (await client.GetFromJsonAsync<List<ReasonCodeDto>>($"/api/v1/reason-codes?category={category}"))!.Single(r => r.Code == code).Id;

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response, HttpStatusCode expected)
    {
        response.StatusCode.Should().Be(expected, await response.Content.ReadAsStringAsync());
        return (await response.Content.ReadFromJsonAsync<T>())!;
    }

    private static async Task ExpectAsync(HttpResponseMessage response, HttpStatusCode expected)
    {
        using (response)
        {
            response.StatusCode.Should().Be(expected, await response.Content.ReadAsStringAsync());
        }
    }

    private static async Task<string> ProblemCodeAsync(HttpResponseMessage response)
    {
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("code").GetString()!;
    }

    /// <summary>Creates and releases a valve tag work order (PREPRESS → ENGRAVE-01 → QC-01 → PACK-01).</summary>
    private static async Task<WorkOrderDetailDto> ReleasedValveTagAsync(HttpClient planner, int quantity = 12)
    {
        using var created = await planner.PostAsJsonAsync("/api/v1/work-orders", new { productId = await ProductIdAsync(planner, "VT-BRASS-RND-38"), quantity, dueDate = "2026-12-20", priority = 1, customerName = "Exec Co." });
        var wo = await ReadAsync<WorkOrderCreatedResponse>(created, HttpStatusCode.Created);
        await ExpectAsync(await planner.PostAsync(new Uri($"/api/v1/work-orders/{wo.Id}/release", UriKind.Relative), null), HttpStatusCode.NoContent);
        return (await planner.GetFromJsonAsync<WorkOrderDetailDto>($"/api/v1/work-orders/{wo.Id}"))!;
    }

    private static async Task RunStepAsync(HttpClient client, int operationId, int good)
    {
        await ExpectAsync(await client.PostAsJsonAsync($"/api/v1/operations/{operationId}/start", new StartOperationRequest(null, Guid.NewGuid())), HttpStatusCode.NoContent);
        await ExpectAsync(await client.PostAsJsonAsync($"/api/v1/operations/{operationId}/complete", new CompleteOperationRequest(good, Guid.NewGuid())), HttpStatusCode.NoContent);
    }

    [Fact]
    [Trait("Story", "PT-018")]
    public async Task Sales_order_crud_and_work_order_generation()
    {
        using var planner = factory.CreateClientAs(Roles.Planner);
        var pm = await ProductIdAsync(planner, "PM-VIN-FLAM-2");
        var vt = await ProductIdAsync(planner, "VT-BRASS-RND-38");

        using var createdResponse = await planner.PostAsJsonAsync("/api/v1/sales-orders", new CreateSalesOrderRequest("Refinery A", "PO-900", new DateOnly(2026, 12, 1),
            [new SalesOrderLineRequest(null, pm, 40, "STEAM", null), new SalesOrderLineRequest(null, vt, 8, null, new ProductSpecDto { Material = "Brass", TagShape = "Round", DiameterMm = 50, ThicknessMm = 1, HoleSizeMm = 4 })]));
        var created = await ReadAsync<SalesOrderCreatedResponse>(createdResponse, HttpStatusCode.Created);

        using var get = await planner.GetAsync(new Uri($"/api/v1/sales-orders/{created.Id}", UriKind.Relative));
        var so = await ReadAsync<SalesOrderDetailDto>(get, HttpStatusCode.OK);
        get.Headers.ETag!.Tag.Should().Be($"\"{so.Version}\"");
        so.Lines.Should().HaveCount(2);
        so.Lines[1].Spec.DiameterMm.Should().Be(50);

        using var put = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/sales-orders/{created.Id}")
        {
            Content = JsonContent.Create(new UpdateSalesOrderRequest("Refinery A (North)", "PO-900", so.DueDate,
                [new SalesOrderLineRequest(so.Lines[0].Id, pm, 45, "STEAM", null), new SalesOrderLineRequest(so.Lines[1].Id, vt, 8, null, null)])),
        };
        put.Headers.TryAddWithoutValidation("If-Match", get.Headers.ETag.Tag);
        using var updatedResponse = await planner.SendAsync(put);
        var updated = await ReadAsync<SalesOrderDetailDto>(updatedResponse, HttpStatusCode.OK);
        updated.Lines[0].Quantity.Should().Be(45);

        using var generated = await planner.PostAsJsonAsync($"/api/v1/sales-orders/{created.Id}/work-orders", new GenerateWorkOrdersRequest(2, null));
        var result = await ReadAsync<GenerateWorkOrdersResponse>(generated, HttpStatusCode.OK);
        result.Created.Should().HaveCount(2);

        var list = await planner.GetFromJsonAsync<JsonElement>("/api/v1/sales-orders?search=Refinery%20A");
        list.GetProperty("items").EnumerateArray().Should().Contain(i => i.GetProperty("workOrderCount").GetInt32() == 2);
        var wo = await planner.GetFromJsonAsync<WorkOrderDetailDto>($"/api/v1/work-orders/{result.Created[0].WorkOrderId}");
        wo!.SalesOrderNumber.Should().Be(created.Number);
        wo.Quantity.Should().Be(45);

        using var cancel = await planner.PostAsync(new Uri($"/api/v1/sales-orders/{created.Id}/cancel", UriKind.Relative), null);
        cancel.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await ProblemCodeAsync(cancel)).Should().Be("SalesOrder.HasWorkOrders");
    }

    [Fact]
    [Trait("Story", "PT-018")]
    public async Task Supervisor_cannot_create_sales_orders()
    {
        using var supervisor = factory.CreateClientAs(Roles.Supervisor);

        using var response = await supervisor.PostAsJsonAsync("/api/v1/sales-orders", new CreateSalesOrderRequest("X", null, new DateOnly(2026, 12, 1), [new SalesOrderLineRequest(null, 1, 1, null, null)]));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("Story", "PT-012")]
    public async Task If_match_stale_returns_412_and_current_succeeds()
    {
        using var planner = factory.CreateClientAs(Roles.Planner);
        using var created = await planner.PostAsJsonAsync("/api/v1/work-orders", new { productId = await ProductIdAsync(planner, "VT-BRASS-RND-38"), quantity = 5, dueDate = "2026-12-20", priority = 3 });
        var wo = await ReadAsync<WorkOrderCreatedResponse>(created, HttpStatusCode.Created);
        using var first = await planner.GetAsync(new Uri($"/api/v1/work-orders/{wo.Id}", UriKind.Relative));
        var etag = first.Headers.ETag!.Tag;

        HttpRequestMessage Put(int quantity, string ifMatch)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/work-orders/{wo.Id}") { Content = JsonContent.Create(new { quantity, dueDate = "2026-12-20", priority = 3 }) };
            request.Headers.TryAddWithoutValidation(HeaderNames.IfMatch, ifMatch);
            return request;
        }

        using (var ok = Put(6, etag))
        {
            await ExpectAsync(await planner.SendAsync(ok), HttpStatusCode.NoContent);
        }

        using (var stale = Put(7, etag))
        {
            using var response = await planner.SendAsync(stale);
            response.StatusCode.Should().Be(HttpStatusCode.PreconditionFailed);
            (await ProblemCodeAsync(response)).Should().Be("Concurrency.StaleVersion");
        }

        using (var malformed = Put(7, "\"not base64!\""))
        {
            await ExpectAsync(await planner.SendAsync(malformed), HttpStatusCode.PreconditionFailed);
        }

        using var latest = await planner.GetAsync(new Uri($"/api/v1/work-orders/{wo.Id}", UriKind.Relative));
        latest.Headers.ETag!.Tag.Should().NotBe(etag);
        (await latest.Content.ReadFromJsonAsync<WorkOrderDetailDto>())!.Quantity.Should().Be(6);
        using (var wildcard = Put(8, "*"))
        {
            await ExpectAsync(await planner.SendAsync(wildcard), HttpStatusCode.NoContent);
        }
    }

    [Fact]
    [Trait("Story", "PT-012")]
    public async Task Product_put_honours_if_match()
    {
        using var admin = factory.CreateClientAs(Roles.Admin);
        var id = await ProductIdAsync(admin, "LBL-POLY-50X25");
        using var get = await admin.GetAsync(new Uri($"/api/v1/products/{id}", UriKind.Relative));
        var product = await ReadAsync<ProductDto>(get, HttpStatusCode.OK);

        using var stale = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/products/{id}") { Content = JsonContent.Create(new UpdateProductRequest(product.Name, product.RequiresArtworkApproval, product.Spec, true)) };
        stale.Headers.TryAddWithoutValidation(HeaderNames.IfMatch, "\"AAAAAAAAAAA=\"");

        await ExpectAsync(await admin.SendAsync(stale), HttpStatusCode.PreconditionFailed);
    }

    [Fact]
    [Trait("Story", "PT-012")]
    public async Task Concurrency_errors_map_to_409_and_412()
    {
        static async Task<int> StatusAsync(IServiceProvider services, Error error)
        {
            var context = new DefaultHttpContext { RequestServices = services };
            context.Response.Body = new MemoryStream();
            await error.ToProblem().ExecuteAsync(context);
            return context.Response.StatusCode;
        }

        (await StatusAsync(factory.Services, ConcurrencyErrors.Conflict)).Should().Be(StatusCodes.Status409Conflict);
        (await StatusAsync(factory.Services, ConcurrencyErrors.StaleVersion)).Should().Be(StatusCodes.Status412PreconditionFailed);
    }

    [Fact]
    [Trait("Story", "PT-022")]
    public async Task Hold_resume_cancel_with_reason_codes()
    {
        using var planner = factory.CreateClientAs(Roles.Planner);
        using var supervisor = factory.CreateClientAs(Roles.Supervisor);
        using var operatorClient = factory.CreateClientAs(Roles.Operator);
        var wo = await ReleasedValveTagAsync(planner);
        var hold = await ReasonAsync(supervisor, "Hold", "CUSTOMER-CHANGE");

        await ExpectAsync(await operatorClient.PostAsJsonAsync($"/api/v1/work-orders/{wo.Id}/hold", new HoldWorkOrderRequest(hold, null)), HttpStatusCode.Forbidden);
        await ExpectAsync(await supervisor.PostAsJsonAsync($"/api/v1/work-orders/{wo.Id}/hold", new HoldWorkOrderRequest(hold, "legend change")), HttpStatusCode.NoContent);

        using var blocked = await operatorClient.PostAsJsonAsync($"/api/v1/operations/{wo.Operations[0].Id}/start", new StartOperationRequest(null, null));
        blocked.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await ProblemCodeAsync(blocked)).Should().Be("WorkOrder.OnHold");

        var held = await supervisor.GetFromJsonAsync<WorkOrderDetailDto>($"/api/v1/work-orders/{wo.Id}");
        held!.Status.Should().Be("OnHold");
        held.HoldReason.Should().Contain("legend change");

        await ExpectAsync(await supervisor.PostAsync(new Uri($"/api/v1/work-orders/{wo.Id}/resume", UriKind.Relative), null), HttpStatusCode.NoContent);
        await ExpectAsync(await supervisor.PostAsJsonAsync($"/api/v1/work-orders/{wo.Id}/cancel", new CancelWorkOrderRequest(" ")), HttpStatusCode.BadRequest);
        await ExpectAsync(await supervisor.PostAsJsonAsync($"/api/v1/work-orders/{wo.Id}/cancel", new CancelWorkOrderRequest("customer withdrew")), HttpStatusCode.NoContent);
        (await supervisor.GetFromJsonAsync<WorkOrderDetailDto>($"/api/v1/work-orders/{wo.Id}"))!.Status.Should().Be("Cancelled");
    }

    [Fact]
    [Trait("Story", "PT-031")]
    public async Task Operator_runs_steps_logs_scrap_and_qc_records_inspection()
    {
        using var planner = factory.CreateClientAs(Roles.Planner);
        using var operatorClient = factory.CreateClientAs(Roles.Operator);
        using var qc = factory.CreateClientAs(Roles.QC);
        var wo = await ReleasedValveTagAsync(planner, 12);
        var ops = wo.Operations;

        var queue = await operatorClient.GetFromJsonAsync<StationQueueDto>("/api/v1/stations/PREPRESS/queue");
        queue!.Items.Should().Contain(i => i.OperationId == ops[0].Id && i.Status == "Ready");

        await ExpectAsync(await operatorClient.PostAsJsonAsync($"/api/v1/operations/{ops[0].Id}/start", new StartOperationRequest("PREPRESS", Guid.NewGuid())), HttpStatusCode.NoContent);
        var pause = await ReasonAsync(operatorClient, "Pause", "BREAK");
        await ExpectAsync(await operatorClient.PostAsJsonAsync($"/api/v1/operations/{ops[0].Id}/pause", new PauseOperationRequest(pause, null)), HttpStatusCode.NoContent);
        await ExpectAsync(await operatorClient.PostAsJsonAsync($"/api/v1/operations/{ops[0].Id}/resume", new ResumeOperationRequest(null)), HttpStatusCode.NoContent);
        var scrap = await ReasonAsync(operatorClient, "Scrap", "MATERIAL-DEFECT");
        await ExpectAsync(await operatorClient.PostAsJsonAsync($"/api/v1/operations/{ops[0].Id}/scrap", new LogScrapRequest(2, scrap, "scratched blank")), HttpStatusCode.NoContent);

        using var tooMany = await operatorClient.PostAsJsonAsync($"/api/v1/operations/{ops[0].Id}/complete", new CompleteOperationRequest(11, null));
        tooMany.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await ExpectAsync(await operatorClient.PostAsJsonAsync($"/api/v1/operations/{ops[0].Id}/complete", new CompleteOperationRequest(10, null)), HttpStatusCode.NoContent);

        var next = await operatorClient.GetFromJsonAsync<OperationDetailDto>($"/api/v1/operations/{ops[1].Id}");
        next!.Status.Should().Be("Ready");
        next.InputQuantity.Should().Be(10);
        await RunStepAsync(operatorClient, ops[1].Id, 10);

        var qcOp = await qc.GetFromJsonAsync<OperationDetailDto>($"/api/v1/operations/{ops[2].Id}");
        qcOp!.IsInspection.Should().BeTrue();
        qcOp.Checklist.Should().NotBeNull();
        await ExpectAsync(await qc.PostAsJsonAsync($"/api/v1/operations/{ops[2].Id}/start", new StartOperationRequest("QC-01", null)), HttpStatusCode.NoContent);
        var answers = qcOp.Checklist!.Items.Select(i => new QcItemResultRequest(i.Id, i.Kind == "Measured" ? null : true, i.Kind == "Measured" ? (i.MinValue + i.MaxValue) / 2 : null)).ToList();

        await ExpectAsync(await operatorClient.PostAsJsonAsync($"/api/v1/operations/{ops[2].Id}/inspection", new RecordInspectionRequest(3, answers, null)), HttpStatusCode.Forbidden);
        using var inspected = await qc.PostAsJsonAsync($"/api/v1/operations/{ops[2].Id}/inspection", new RecordInspectionRequest(3, answers, "all good"));
        (await ReadAsync<InspectionRecordedResponse>(inspected, HttpStatusCode.OK)).Result.Should().Be("Passed");

        await RunStepAsync(operatorClient, ops[3].Id, 10);
        var done = await planner.GetFromJsonAsync<WorkOrderDetailDto>($"/api/v1/work-orders/{wo.Id}");
        done!.Status.Should().Be("Completed");
        done.CompletedQuantity.Should().Be(10);
        done.Operations[0].ScrapQuantity.Should().Be(2);
        done.Operations[0].StartedBy.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Story", "PT-027")]
    public async Task Traveler_and_scan_resolution()
    {
        using var planner = factory.CreateClientAs(Roles.Planner);
        using var operatorClient = factory.CreateClientAs(Roles.Operator);
        var wo = await ReleasedValveTagAsync(planner);

        var traveler = await planner.GetFromJsonAsync<TravelerDto>($"/api/v1/work-orders/{wo.Id}/traveler");
        traveler!.HeaderQrSvg.Should().StartWith("<svg").And.Contain("viewBox");
        traveler.Operations.Should().HaveCount(wo.Operations.Count);

        var byOp = await operatorClient.GetFromJsonAsync<ScanResolutionDto>($"/api/v1/scan/{Uri.EscapeDataString(traveler.Operations[1].Payload)}");
        byOp!.OperationId.Should().Be(wo.Operations[1].Id);
        var byHeader = await operatorClient.GetFromJsonAsync<ScanResolutionDto>($"/api/v1/scan/{Uri.EscapeDataString(traveler.HeaderPayload)}");
        byHeader!.Kind.Should().Be("WorkOrder");
        using var unknown = await operatorClient.GetAsync(new Uri("/api/v1/scan/WO-2026-999999", UriKind.Relative));
        unknown.StatusCode.Should().Be(HttpStatusCode.NotFound);

        using var viewer = factory.CreateClientAs(Roles.Viewer);
        using var viewerScan = await viewer.GetAsync(new Uri($"/api/v1/scan/{wo.Number}", UriKind.Relative));
        viewerScan.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("Story", "PT-034")]
    public async Task Qc_templates_are_seeded_for_every_product_type()
    {
        using var viewer = factory.CreateClientAs(Roles.Viewer);

        var templates = await viewer.GetFromJsonAsync<List<QcTemplateDto>>("/api/v1/qc/templates");

        templates!.Select(t => t.ProductType).Should().BeEquivalentTo(["PipeMarker", "ValveTag", "SafetySign", "Label"]);
        templates.Should().OnlyContain(t => t.Items.Count > 0);
    }

    [Fact]
    [Trait("Story", "PT-012")]
    public async Task Admin_manages_users_end_to_end()
    {
        using var admin = factory.CreateClientAs(Roles.Admin);
        using var planner = factory.CreateClientAs(Roles.Planner);
        var email = $"op{Guid.NewGuid():N}"[..14] + "@prodtrack.test";
        var tempPassword = TestPasswords.Generate();

        await ExpectAsync(await planner.GetAsync(new Uri("/api/v1/users", UriKind.Relative)), HttpStatusCode.Forbidden);
        await ExpectAsync(await admin.PostAsJsonAsync("/api/v1/users", new CreateUserRequest(email, "Short Pw", "short", [Roles.Operator], null, null, null)), HttpStatusCode.BadRequest);

        using var createdResponse = await admin.PostAsJsonAsync("/api/v1/users", new CreateUserRequest(email, "Olivia Operator", tempPassword, [Roles.Operator], "E-100", "B-100", "PRINT-01"));
        var created = await ReadAsync<UserCreatedResponse>(createdResponse, HttpStatusCode.Created);
        await ExpectAsync(await admin.PostAsJsonAsync("/api/v1/users", new CreateUserRequest(email, "Dup", tempPassword, [Roles.Operator], null, null, null)), HttpStatusCode.BadRequest);

        using var get = await admin.GetAsync(new Uri($"/api/v1/users/{created.Id}", UriKind.Relative));
        var user = await ReadAsync<UserDetailDto>(get, HttpStatusCode.OK);
        user.MustChangePassword.Should().BeTrue();
        user.Roles.Should().Equal(Roles.Operator);
        user.HomeStationCode.Should().Be("PRINT-01");

        using var update = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/users/{created.Id}") { Content = JsonContent.Create(new UpdateUserRequest("Olivia O.", [Roles.Operator, Roles.QC], "E-100", "B-100", "QC-01")) };
        update.Headers.TryAddWithoutValidation(HeaderNames.IfMatch, get.Headers.ETag!.Tag);
        var updated = await ReadAsync<UserDetailDto>(await admin.SendAsync(update), HttpStatusCode.OK);
        updated.Roles.Should().BeEquivalentTo([Roles.Operator, Roles.QC]);

        using var stale = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/users/{created.Id}") { Content = JsonContent.Create(new UpdateUserRequest("Stale", [Roles.Viewer], null, null, null)) };
        stale.Headers.TryAddWithoutValidation(HeaderNames.IfMatch, get.Headers.ETag.Tag);
        await ExpectAsync(await admin.SendAsync(stale), HttpStatusCode.PreconditionFailed);

        await ExpectAsync(await admin.PostAsJsonAsync($"/api/v1/users/{created.Id}/active", new SetUserActiveRequest(false)), HttpStatusCode.NoContent);
        await ExpectAsync(await admin.PostAsJsonAsync($"/api/v1/users/{created.Id}/reset-password", new ResetPasswordRequest(TestPasswords.Generate())), HttpStatusCode.NoContent);
        var users = await admin.GetFromJsonAsync<List<UserSummaryDto>>("/api/v1/users");
        users!.Single(u => u.Id == created.Id).IsActive.Should().BeFalse();
    }
}
