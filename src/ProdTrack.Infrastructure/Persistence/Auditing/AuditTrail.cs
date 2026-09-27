using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ProdTrack.Domain.Audit;

namespace ProdTrack.Infrastructure.Persistence.Auditing;

/// <summary>Builds audit entries (entity, key, field changes old -> new) from the change tracker (PT-047).</summary>
internal static class AuditTrail
{
    private static readonly HashSet<string> ExcludedProperties = new(StringComparer.Ordinal)
    {
        "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "RowVersion", "NormalizedEmail", "NormalizedUserName",
    };

    private static readonly HashSet<Type> ExcludedTypes =
    [
        typeof(AuditEntry), typeof(DataProtectionKey), typeof(IdentityUserToken<string>), typeof(NumberSequence),
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
        WriteIndented = false,
    };

    public static List<PendingAudit> Capture(ChangeTracker changeTracker)
    {
        var pending = new List<PendingAudit>();
        foreach (var entry in changeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted)
                || ExcludedTypes.Contains(entry.Metadata.ClrType)
                || entry.Metadata.IsOwned())
            {
                continue;
            }

            var action = entry.State switch
            {
                EntityState.Added => AuditAction.Insert,
                EntityState.Deleted => AuditAction.Delete,
                _ => AuditAction.Update,
            };
            var changes = CollectChanges(entry, action);
            if (action == AuditAction.Update && changes.Count == 0)
            {
                continue;
            }

            pending.Add(new PendingAudit(entry, action, changes));
        }

        return pending;
    }

    public static AuditEntry ToAuditEntry(PendingAudit pending, string? userId, string? correlationId, DateTimeOffset nowUtc)
    {
        var entry = pending.Entry;
        return new AuditEntry(
            entry.Metadata.ClrType.Name,
            ResolveKey(entry),
            pending.Action,
            JsonSerializer.Serialize(pending.Changes, JsonOptions),
            userId,
            correlationId,
            nowUtc);
    }

    private static Dictionary<string, AuditChange> CollectChanges(EntityEntry entry, AuditAction action)
    {
        var changes = new Dictionary<string, AuditChange>(StringComparer.Ordinal);
        foreach (var property in entry.Properties)
        {
            AddChange(changes, property.Metadata.Name, property, action);
        }

        foreach (var complex in entry.ComplexProperties)
        {
            foreach (var property in complex.Properties)
            {
                AddChange(changes, $"{complex.Metadata.Name}.{property.Metadata.Name}", property, action);
            }
        }

        return changes;
    }

    private static void AddChange(Dictionary<string, AuditChange> changes, string name, PropertyEntry property, AuditAction action)
    {
        if (ExcludedProperties.Contains(property.Metadata.Name) || property.Metadata.IsShadowProperty())
        {
            return;
        }

        switch (action)
        {
            case AuditAction.Insert when property.CurrentValue is not null && !property.Metadata.IsPrimaryKey():
                changes[name] = new AuditChange(null, property.CurrentValue);
                break;
            case AuditAction.Delete:
                changes[name] = new AuditChange(property.OriginalValue, null);
                break;
            case AuditAction.Update when property.IsModified && !Equals(property.OriginalValue, property.CurrentValue):
                changes[name] = new AuditChange(property.OriginalValue, property.CurrentValue);
                break;
            default:
                break;
        }
    }

    private static string ResolveKey(EntityEntry entry)
    {
        foreach (var businessKey in (string[])["Number", "Sku"])
        {
            var property = entry.Metadata.FindProperty(businessKey);
            if (property is not null && entry.Property(businessKey).CurrentValue is string value)
            {
                return value;
            }
        }

        var key = entry.Metadata.FindPrimaryKey();
        return key is null
            ? string.Empty
            : string.Join(",", key.Properties.Select(p => Convert.ToString(entry.Property(p.Name).CurrentValue, System.Globalization.CultureInfo.InvariantCulture)));
    }
}

internal sealed record PendingAudit(EntityEntry Entry, AuditAction Action, Dictionary<string, AuditChange> Changes);

internal sealed record AuditChange(
    [property: JsonPropertyName("old")] object? Old,
    [property: JsonPropertyName("new")] object? New);
