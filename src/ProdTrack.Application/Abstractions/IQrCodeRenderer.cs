namespace ProdTrack.Application.Abstractions;

/// <summary>Renders QR codes for travelers (docs/02 section 9: error correction level M). Implemented with QRCoder.</summary>
public interface IQrCodeRenderer
{
    /// <summary>Returns a self-contained SVG document for the payload.</summary>
    string RenderSvg(string payload);
}
