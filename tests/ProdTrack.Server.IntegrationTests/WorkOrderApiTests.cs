using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ProdTrack.Application.Security;
using ProdTrack.Contracts.Common;
using ProdTrack.Contracts.Products;
using ProdTrack.Contracts.WorkOrders;

namespace ProdTrack.Server.IntegrationTests;

[Collection(ServerCollection.Name)]
[Trait("Category", "Integration")]
public sealed class WorkOrderApiTests(ProdTrackFactory factory)
{
    private static async Task<WorkOrderCreatedResponse> CreateAsync(HttpClient client, string sku, string? legend = null)
    {
        var product = (await client.GetFromJsonAsync<List<ProductDto>>($"/api/v1/products?search={sku}"))!.Single();
        using var response = await client.PostAsJsonAsync("/api/v1/work-orders", new
        {
            productId = product.Id,
            quantity = 50,
            dueDate = "2026-12-15",
            priority = 2,
            customerName = "Integration Co.",
            legend,
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
        return (await response.Content.ReadFromJsonAsync<WorkOrderCreatedResponse>())!;
    }

    [Fact]
    [Trait("Story", "PT-024")]
    public async Task Artwork_gate_upload_approve_and_release()
    {
        using var planner = factory.CreateClientAs(Roles.Planner);
        var wo = await CreateAsync(planner, "SS-ALU-DANGER-A4");
        wo.Number.Should().MatchRegex(@"^WO-\d{4}-\d{6}$");

        using var blocked = await planner.PostAsync(new Uri($"/api/v1/work-orders/{wo.Id}/release", UriKind.Relative), null);
        blocked.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        using (var problem = JsonDocument.Parse(await blocked.Content.ReadAsStringAsync()))
        {
            problem.RootElement.GetProperty("code").GetString().Should().Be("WorkOrder.ArtworkNotApproved");
            problem.RootElement.GetProperty("title").GetString().Should().Be("Artwork not approved");
        }

        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(Encoding.ASCII.GetBytes("%PDF-1.7 integration proof"));
        file.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(file, "file", "proof.pdf");
        using var uploaded = await planner.PostAsync(new Uri($"/api/v1/work-orders/{wo.Id}/artwork", UriKind.Relative), form);
        uploaded.StatusCode.Should().Be(HttpStatusCode.Created, await uploaded.Content.ReadAsStringAsync());

        using var download = await planner.GetAsync(new Uri($"/api/v1/work-orders/{wo.Id}/artwork/1", UriKind.Relative));
        download.StatusCode.Should().Be(HttpStatusCode.OK);
        download.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");

        using var approved = await planner.PostAsJsonAsync($"/api/v1/work-orders/{wo.Id}/artwork/1/approve", new { note = "OK" });
        approved.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var released = await planner.PostAsync(new Uri($"/api/v1/work-orders/{wo.Id}/release", UriKind.Relative), null);
        released.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var detail = await planner.GetFromJsonAsync<WorkOrderDetailDto>($"/api/v1/work-orders/by-number/{wo.Number}");
        detail!.Status.Should().Be("Released");
        detail.Operations.Should().NotBeEmpty();
        detail.Operations[0].Status.Should().Be("Ready");
        detail.ArtworkProofs.Should().ContainSingle(p => p.Status == "Approved");
    }

    [Fact]
    [Trait("Story", "PT-019")]
    public async Task Draft_update_delete_and_search()
    {
        using var planner = factory.CreateClientAs(Roles.Planner);
        var wo = await CreateAsync(planner, "PM-VIN-FLAM-2", legend: "STEAM 10 BAR");

        using var updated = await planner.PutAsJsonAsync($"/api/v1/work-orders/{wo.Id}", new { quantity = 75, dueDate = "2026-12-20", priority = 1, customerName = "Integration Co.", legend = "STEAM 12 BAR" });
        updated.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var page = await planner.GetFromJsonAsync<PagedResponse<WorkOrderSummaryDto>>($"/api/v1/work-orders?search={wo.Number}");
        page!.Items.Should().ContainSingle().Which.Quantity.Should().Be(75);

        using var deleted = await planner.DeleteAsync(new Uri($"/api/v1/work-orders/{wo.Id}", UriKind.Relative));
        deleted.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var missing = await planner.GetAsync(new Uri($"/api/v1/work-orders/{wo.Id}", UriKind.Relative));
        missing.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("Story", "PT-019")]
    public async Task Pipe_marker_without_legend_is_rejected()
    {
        using var planner = factory.CreateClientAs(Roles.Planner);
        var product = (await planner.GetFromJsonAsync<List<ProductDto>>("/api/v1/products?search=PM-VIN"))!.Single();

        using var response = await planner.PostAsJsonAsync("/api/v1/work-orders", new { productId = product.Id, quantity = 5, dueDate = "2026-12-01", priority = 3 });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("legend");
    }

    [Fact]
    [Trait("Story", "PT-023")]
    public async Task Upload_rejects_disguised_files()
    {
        using var planner = factory.CreateClientAs(Roles.Planner);
        var wo = await CreateAsync(planner, "LBL-POLY-50X25");
        using var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(Encoding.ASCII.GetBytes("MZ not really a pdf")), "file", "proof.pdf");

        using var response = await planner.PostAsync(new Uri($"/api/v1/work-orders/{wo.Id}/artwork", UriKind.Relative), form);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
