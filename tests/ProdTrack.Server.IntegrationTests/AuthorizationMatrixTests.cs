using System.Net;
using System.Net.Http.Json;
using ProdTrack.Application.Security;

namespace ProdTrack.Server.IntegrationTests;

/// <summary>Role matrix (BRD 6.10 / docs/02 section 7.3) enforced on the REST API.</summary>
[Collection(ServerCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Story", "PT-010")]
public sealed class AuthorizationMatrixTests(ProdTrackFactory factory)
{
    public static TheoryData<string, string> ReadEndpointsForEveryRole()
    {
        var data = new TheoryData<string, string>();
        foreach (var role in Roles.All)
        {
            foreach (var path in (string[])["/api/v1/me", "/api/v1/stations", "/api/v1/work-orders", "/api/v1/products", "/api/v1/routings", "/api/v1/dashboard/wip", "/api/v1/reference/color-schemes", "/api/v1/reason-codes"])
            {
                data.Add(role, path);
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(ReadEndpointsForEveryRole))]
    public async Task Every_role_can_read(string role, string path)
    {
        using var client = factory.CreateClientAs(role);

        using var response = await client.GetAsync(new Uri(path, UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData(Roles.Admin, false)]
    [InlineData(Roles.Planner, true)]
    [InlineData(Roles.Supervisor, true)]
    [InlineData(Roles.Operator, true)]
    [InlineData(Roles.QC, true)]
    [InlineData(Roles.Viewer, true)]
    public async Task Only_admin_manages_stations(string role, bool forbidden)
    {
        using var client = factory.CreateClientAs(role);

        using var response = await client.PostAsJsonAsync("/api/v1/stations", new { code = $"T-{role.ToUpperInvariant()}", name = "Matrix", type = "Packing" });

        if (forbidden)
        {
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
        else
        {
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }
    }

    [Theory]
    [InlineData(Roles.Admin, HttpStatusCode.Created)]
    [InlineData(Roles.Planner, HttpStatusCode.Created)]
    [InlineData(Roles.Supervisor, HttpStatusCode.Forbidden)]
    [InlineData(Roles.Operator, HttpStatusCode.Forbidden)]
    [InlineData(Roles.QC, HttpStatusCode.Forbidden)]
    [InlineData(Roles.Viewer, HttpStatusCode.Forbidden)]
    public async Task Admin_and_planner_create_work_orders(string role, HttpStatusCode expected)
    {
        using var admin = factory.CreateClientAs(Roles.Admin);
        var products = await admin.GetFromJsonAsync<List<ProductRow>>("/api/v1/products?search=VT-BRASS");
        using var client = factory.CreateClientAs(role);

        using var response = await client.PostAsJsonAsync("/api/v1/work-orders", new
        {
            productId = products!.Single().Id,
            quantity = 10,
            dueDate = "2026-12-01",
            priority = 3,
            customerName = "Matrix Inc.",
        });

        response.StatusCode.Should().Be(expected);
    }

    [Fact]
    public async Task Operator_cannot_approve_artwork()
    {
        using var client = factory.CreateClientAs(Roles.Operator);

        using var response = await client.PostAsJsonAsync("/api/v1/work-orders/1/artwork/1/approve", new { note = "ok" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private sealed record ProductRow(int Id, string Sku);
}
