using ProdTrack.Domain.Common;

namespace ProdTrack.Domain.Tests;

[Trait("Category", "Unit")]
public class ResultTests
{
    [Fact]
    public void Value_of_failed_result_throws()
    {
        Result<int> result = Error.NotFound("X.NotFound", "missing");

        result.IsFailure.Should().BeTrue();
        var act = () => result.Value;
        act.Should().Throw<InvalidOperationException>().WithMessage("*X.NotFound*");
    }

    [Fact]
    public void Implicit_success_conversion_carries_the_value()
    {
        Result<string> result = "ok";

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("ok");
    }
}
