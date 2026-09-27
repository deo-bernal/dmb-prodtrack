using Microsoft.EntityFrameworkCore;
using ProdTrack.Domain.Products;

namespace ProdTrack.Infrastructure.Persistence.Seeding;

/// <summary>Fictitious demo catalog, enabled only locally or explicitly (Seed:Demo=true).</summary>
internal static class DemoDataSeeder
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        if (await db.Products.AnyAsync(cancellationToken))
        {
            return;
        }

        db.Products.AddRange(
            Product.Create("PM-VIN-FLAM-2", "Pipe marker, vinyl, flammable, 2in OD", ProductType.PipeMarker, requiresArtworkApproval: false, new ProductSpec
            {
                Material = "Vinyl self-adhesive",
                ColorSchemeCode = "FLAMMABLE",
                PipeOdRange = "1.5-2.5 in",
                LetterHeightMm = 19,
                Mounting = "Adhesive",
                Standard = "ASME A13.1",
            }).Value,
            Product.Create("VT-BRASS-RND-38", "Valve tag, brass, round 38 mm", ProductType.ValveTag, requiresArtworkApproval: false, new ProductSpec
            {
                Material = "Brass",
                TagShape = "Round",
                DiameterMm = 38,
                ThicknessMm = 0.8m,
                HoleSizeMm = 4.8m,
            }).Value,
            Product.Create("SS-ALU-DANGER-A4", "Safety sign, aluminium, DANGER, A4", ProductType.SafetySign, requiresArtworkApproval: true, new ProductSpec
            {
                Material = "Aluminium",
                SignalWord = "DANGER",
                WidthMm = 297,
                HeightMm = 210,
                Mounting = "Holes",
                Standard = "ANSI Z535",
            }).Value,
            Product.Create("LBL-POLY-50X25", "Label, polyester, 50 x 25 mm", ProductType.Label, requiresArtworkApproval: true, new ProductSpec
            {
                Material = "Polyester",
                WidthMm = 50,
                HeightMm = 25,
                Adhesive = "Permanent acrylic",
                Finish = "Matte laminate",
            }).Value);
        await db.SaveChangesAsync(cancellationToken);
    }
}
