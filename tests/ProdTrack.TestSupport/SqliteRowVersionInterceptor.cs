using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ProdTrack.TestSupport;

/// <summary>
/// Emulates SQL Server <c>rowversion</c> on SQLite: every inserted or updated row with a RowVersion column gets a
/// fresh random version, while EF keeps using the original value in the UPDATE's WHERE clause. That lets tests
/// exercise If-Match (412) and concurrent-write (409) behaviour without SQL Server.
/// </summary>
public sealed class SqliteRowVersionInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Stamp(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Stamp(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void Stamp(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        context.ChangeTracker.DetectChanges();
        foreach (var entry in context.ChangeTracker.Entries().Where(e => e.State is EntityState.Added or EntityState.Modified))
        {
            var property = entry.Metadata.FindProperty("RowVersion");
            if (property is null || property.ClrType != typeof(byte[]))
            {
                continue;
            }

            entry.Property(property.Name).CurrentValue = RandomNumberGenerator.GetBytes(8);
        }
    }
}
