using ProdTrack.Domain.Common;
using ProdTrack.Domain.Products;

namespace ProdTrack.Domain.Tests;

[Trait("Category", "Unit")]
[Trait("Story", "PT-015")]
public class ProductTests
{
    [Fact]
    public void Pipe_marker_requires_color_scheme_and_pipe_od_range()
    {
        var result = Product.Create("PM-1", "Pipe marker", ProductType.PipeMarker, false, new ProductSpec { Material = "Vinyl" });

        var errors = result.Error.Should().BeOfType<ValidationError>().Subject.Errors;
        errors.Should().ContainKeys("spec.colorSchemeCode", "spec.pipeOdRange");
    }

    [Fact]
    public void Valve_tag_requires_positive_dimensions()
    {
        var spec = new ProductSpec { Material = "Brass", TagShape = "Round", DiameterMm = 0, ThicknessMm = 1, HoleSizeMm = -1 };

        var result = Product.Create("VT-1", "Valve tag", ProductType.ValveTag, false, spec);

        result.Error.Should().BeOfType<ValidationError>().Which.Errors.Should().ContainKeys("spec.diameterMm", "spec.holeSizeMm")
            .And.NotContainKey("spec.thicknessMm");
    }

    [Fact]
    public void Safety_sign_requires_signal_word()
    {
        var spec = TestData.SafetySignSpec with { SignalWord = null };

        var result = Product.Create("SS-1", "Sign", ProductType.SafetySign, true, spec);

        result.Error.Should().BeOfType<ValidationError>().Which.Errors.Should().ContainKey("spec.signalWord");
    }

    [Fact]
    public void Valid_product_normalizes_sku_and_keeps_spec()
    {
        var result = Product.Create(" pm-1 ", "Pipe marker", ProductType.PipeMarker, true, TestData.PipeMarkerSpec);

        result.IsSuccess.Should().BeTrue();
        result.Value.Sku.Should().Be("PM-1");
        result.Value.RequiresArtworkApproval.Should().BeTrue();
        result.Value.DefaultSpec.ColorSchemeCode.Should().Be("FLAMMABLE");
    }

    [Fact]
    public void Update_validates_spec_for_the_existing_type()
    {
        var product = TestData.PipeMarker();

        var result = product.Update("Renamed", false, new ProductSpec { Material = "Vinyl" }, isActive: true);

        result.IsFailure.Should().BeTrue();
        product.Name.Should().Be("Pipe marker");
    }
}
