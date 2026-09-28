using ProdTrack.Application.Abstractions;
using QRCoder;

namespace ProdTrack.Infrastructure.Qr;

/// <summary>QR codes with QRCoder (MIT), error correction level M, scalable SVG (docs/02 section 9).</summary>
internal sealed class QrCodeRenderer : IQrCodeRenderer
{
    public string RenderSvg(string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
        var svg = new SvgQRCode(data);
        return svg.GetGraphic(4, "#000000", "#ffffff", drawQuietZones: true, sizingMode: SvgQRCode.SizingMode.ViewBoxAttribute);
    }
}
