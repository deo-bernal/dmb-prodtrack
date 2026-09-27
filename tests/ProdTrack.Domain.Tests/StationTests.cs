using ProdTrack.Domain.Stations;

namespace ProdTrack.Domain.Tests;

[Trait("Category", "Unit")]
[Trait("Story", "PT-013")]
public class StationTests
{
    [Theory]
    [InlineData("print-01", "PRINT-01")]
    [InlineData("  lam-01 ", "LAM-01")]
    public void Create_normalizes_code_to_upper_case(string input, string expected)
    {
        var result = Station.Create(input, "Printer", StationType.Printing, "WC-A");

        result.IsSuccess.Should().BeTrue();
        result.Value.Code.Should().Be(expected);
        result.Value.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    [InlineData("PRINT 01")]
    [InlineData("PRINT_01")]
    [InlineData("THIS-CODE-IS-WAY-TOO-LONG")]
    public void Create_rejects_invalid_codes(string code)
    {
        var result = Station.Create(code, "Printer", StationType.Printing, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<Common.ValidationError>()
            .Which.Errors.Should().ContainKey("code");
    }

    [Fact]
    public void Create_requires_a_name()
    {
        var result = Station.Create("PRINT-01", " ", StationType.Printing, null);

        result.Error.Should().BeOfType<Common.ValidationError>().Which.Errors.Should().ContainKey("name");
    }

    [Fact]
    public void Deactivate_and_activate_toggle_the_flag()
    {
        var station = Station.Create("PRINT-01", "Printer", StationType.Printing, null).Value;

        station.Deactivate();
        station.IsActive.Should().BeFalse();

        station.Activate();
        station.IsActive.Should().BeTrue();
    }
}
