using Microsoft.EntityFrameworkCore;
using ProdTrack.Application.Abstractions;
using ProdTrack.Domain.Common;
using ProdTrack.Domain.Products;

namespace ProdTrack.Application.Products;

/// <summary>Spec values must come from reference lists, not free text (BR-15).</summary>
internal static class ProductReferenceCheck
{
    public static async Task<Error?> CheckAsync(IAppDbContext db, ProductSpec spec, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(spec.ColorSchemeCode)
            && !await db.ColorSchemes.AnyAsync(c => c.Code == spec.ColorSchemeCode && c.IsActive, cancellationToken))
        {
            return ProductErrors.UnknownColorScheme(spec.ColorSchemeCode);
        }

        if (!string.IsNullOrWhiteSpace(spec.SignalWord)
            && !await db.SignalWords.AnyAsync(s => s.Word == spec.SignalWord && s.IsActive, cancellationToken))
        {
            return ProductErrors.UnknownSignalWord(spec.SignalWord);
        }

        return null;
    }
}
