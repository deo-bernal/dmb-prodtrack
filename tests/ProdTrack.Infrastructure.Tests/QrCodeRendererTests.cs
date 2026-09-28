using ProdTrack.Infrastructure.Qr;

namespace ProdTrack.Infrastructure.Tests;

[Trait("Category", "Unit")]
[Trait("Story", "PT-027")]
public class QrCodeRendererTests
{
    [Fact]
    public void Renders_scalable_svg_for_payload()
    {
        var svg = new QrCodeRenderer().RenderSvg("OP:WO-2026-000001:20");

        svg.Should().StartWith("<svg").And.Contain("viewBox").And.EndWith("</svg>");
    }

    [Fact]
    public void Different_payloads_give_different_codes() =>
        new QrCodeRenderer().RenderSvg("WO:WO-2026-000001").Should().NotBe(new QrCodeRenderer().RenderSvg("WO:WO-2026-000002"));
}
