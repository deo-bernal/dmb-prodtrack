using ProdTrack.Application.Products.CreateProduct;
using ProdTrack.Application.Products.GetProducts;
using ProdTrack.Application.Security;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.Products;
using ProdTrack.TestSupport;

namespace ProdTrack.Application.Tests;

[Trait("Category", "Unit")]
[Trait("Story", "PT-015")]
public sealed class ProductUseCaseTests : IAsyncLifetime
{
    private TestApplication _app = null!;

    public async Task InitializeAsync() => _app = await TestApplication.CreateAsync();

    public async Task DisposeAsync() => await _app.DisposeAsync();

    private static ProductSpec PipeSpec(string scheme) => new() { Material = "Vinyl", ColorSchemeCode = scheme, PipeOdRange = "1-2 in" };

    [Fact]
    public async Task Planner_can_create_a_pipe_marker_with_a_known_color_scheme()
    {
        _app.User.SetRoles(Roles.Planner);

        var result = await _app.SendAsync(new CreateProductCommand("PM-TEST-1", "Pipe marker", ProductType.PipeMarker, false, PipeSpec("WATER")));

        result.IsSuccess.Should().BeTrue();
        (await _app.QueryAsync(new GetProductsQuery(Search: "PM-TEST"))).Value.Should().ContainSingle();
    }

    [Fact]
    public async Task Unknown_color_scheme_is_rejected()
    {
        var result = await _app.SendAsync(new CreateProductCommand("PM-TEST-2", "Pipe marker", ProductType.PipeMarker, false, PipeSpec("PINK")));

        result.Error.Should().BeOfType<ValidationError>().Which.Errors.Should().ContainKey("spec.colorSchemeCode");
    }

    [Fact]
    public async Task Duplicate_sku_is_rejected()
    {
        await _app.SendAsync(new CreateProductCommand("PM-DUP", "Pipe marker", ProductType.PipeMarker, false, PipeSpec("WATER")));

        var result = await _app.SendAsync(new CreateProductCommand("pm-dup", "Pipe marker", ProductType.PipeMarker, false, PipeSpec("WATER")));

        result.Error.Should().BeOfType<ValidationError>().Which.Errors.Should().ContainKey("sku");
    }

    [Fact]
    public async Task Operator_cannot_create_products()
    {
        _app.User.SetRoles(Roles.Operator);

        var result = await _app.SendAsync(new CreateProductCommand("PM-X", "Pipe marker", ProductType.PipeMarker, false, PipeSpec("WATER")));

        result.Error!.Type.Should().Be(ErrorType.Forbidden);
    }
}
