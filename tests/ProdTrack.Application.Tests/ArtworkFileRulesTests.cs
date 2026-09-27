using System.Text;
using ProdTrack.Application.WorkOrders.Artwork;

namespace ProdTrack.Application.Tests;

[Trait("Category", "Unit")]
[Trait("Story", "PT-023")]
public class ArtworkFileRulesTests
{
    [Theory]
    [InlineData("proof.pdf", "application/pdf")]
    [InlineData("proof.PNG", "image/png")]
    [InlineData("proof.jpeg", "image/jpeg")]
    [InlineData("proof.svg", "image/svg+xml")]
    public void Allowed_extensions_map_to_content_types(string fileName, string expected)
    {
        ArtworkFileRules.TryGetContentType(fileName, out var contentType).Should().BeTrue();
        contentType.Should().Be(expected);
    }

    [Theory]
    [InlineData("proof.exe")]
    [InlineData("proof.html")]
    [InlineData("proof")]
    public void Other_extensions_are_rejected(string fileName) =>
        ArtworkFileRules.TryGetContentType(fileName, out _).Should().BeFalse();

    [Fact]
    public void Header_sniffing_detects_real_formats()
    {
        ArtworkFileRules.HeaderMatches(".pdf", "%PDF-1.4"u8).Should().BeTrue();
        ArtworkFileRules.HeaderMatches(".png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0 }).Should().BeTrue();
        ArtworkFileRules.HeaderMatches(".jpg", new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }).Should().BeTrue();
        ArtworkFileRules.HeaderMatches(".svg", Encoding.UTF8.GetBytes("<?xml version=\"1.0\"?><svg xmlns=\"http://www.w3.org/2000/svg\"/>")).Should().BeTrue();
        ArtworkFileRules.HeaderMatches(".pdf", "MZ"u8).Should().BeFalse();
        ArtworkFileRules.HeaderMatches(".svg", "<html>"u8).Should().BeFalse();
    }
}
