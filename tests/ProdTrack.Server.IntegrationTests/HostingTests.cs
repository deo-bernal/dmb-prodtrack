using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ProdTrack.Application.Security;

namespace ProdTrack.Server.IntegrationTests;

[Collection(ServerCollection.Name)]
[Trait("Category", "Integration")]
public sealed class HostingTests(ProdTrackFactory factory)
{
    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    [InlineData("/health")]
    [Trait("Story", "PT-003")]
    public async Task Health_endpoints_are_anonymous_and_healthy(string path)
    {
        using var client = factory.CreateAnonymousClient();

        using var response = await client.GetAsync(new Uri(path, UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Be("Healthy");
    }

    [Fact]
    [Trait("Story", "PT-004")]
    public async Task Correlation_id_is_echoed_or_generated()
    {
        using var client = factory.CreateAnonymousClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        request.Headers.Add("X-Correlation-Id", "it-corr-123");

        using var echoed = await client.SendAsync(request);
        using var generated = await client.GetAsync(new Uri("/health/live", UriKind.Relative));

        echoed.Headers.GetValues("X-Correlation-Id").Should().ContainSingle().Which.Should().Be("it-corr-123");
        generated.Headers.GetValues("X-Correlation-Id").Single().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    [Trait("Story", "PT-005")]
    public async Task Validation_errors_are_problem_details_400()
    {
        using var client = factory.CreateClientAs(Roles.Admin);

        using var response = await client.PostAsJsonAsync("/api/v1/stations", new { code = "", name = "", type = "Printing" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("errors").TryGetProperty("code", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("Story", "PT-005")]
    public async Task Unhandled_exceptions_are_problem_details_500_without_stack_trace()
    {
        using var client = factory.CreateClientAs(Roles.Admin);

        using var response = await client.GetAsync(new Uri("/api/v1/_test/throw", UriKind.Relative));
        var text = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json", text);
        text.Should().Contain("An unexpected error occurred").And.NotContain("InvalidOperationException").And.NotContain(" at ");
    }

    [Fact]
    [Trait("Story", "PT-009")]
    public async Task Anonymous_api_calls_get_401_not_a_redirect()
    {
        using var client = factory.CreateAnonymousClient();

        using var response = await client.GetAsync(new Uri("/api/v1/stations", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    [Trait("Story", "PT-009")]
    public async Task Anonymous_pages_redirect_to_login()
    {
        using var client = factory.CreateAnonymousClient();

        using var response = await client.GetAsync(new Uri("/work-orders", UriKind.Relative));

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.ToString().Should().Contain("/account/login");
    }

    [Fact]
    [Trait("Story", "PT-009")]
    public async Task Login_page_renders_without_self_registration()
    {
        using var client = factory.CreateAnonymousClient();

        using var response = await client.GetAsync(new Uri("/account/login", UriKind.Relative));
        var html = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        html.Should().Contain("Sign in to DMB ProdTrack").And.NotContainEquivalentOf("register");
        response.Headers.GetValues("X-Content-Type-Options").Should().Contain("nosniff");
    }

    [Fact]
    [Trait("Story", "PT-010")]
    public async Task Dashboard_page_renders_for_viewer_and_hides_admin_menu()
    {
        using var client = factory.CreateClientAs(Roles.Viewer);

        using var response = await client.GetAsync(new Uri("/", UriKind.Relative));
        var html = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        html.Should().Contain("Dashboard").And.NotContain("nav-admin-stations").And.NotContain("nav-new-work-order");
    }

    [Fact]
    [Trait("Story", "PT-010")]
    public async Task Admin_sees_admin_menu()
    {
        using var client = factory.CreateClientAs(Roles.Admin);

        var html = await client.GetStringAsync(new Uri("/", UriKind.Relative));

        html.Should().Contain("nav-admin-stations");
    }
}
