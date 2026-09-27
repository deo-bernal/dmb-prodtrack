using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProdTrack.Domain.Audit;

namespace ProdTrack.Infrastructure.Persistence.Configurations;

internal sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("AuditEntry", Schemas.Audit);
        builder.Property(a => a.EntityName).HasMaxLength(100);
        builder.Property(a => a.EntityKey).HasMaxLength(100);
        builder.Property(a => a.Action).HasConversion<string>().HasMaxLength(10);
        builder.Property(a => a.UserId).HasMaxLength(450);
        builder.Property(a => a.CorrelationId).HasMaxLength(100);
        builder.HasIndex(a => new { a.EntityName, a.EntityKey, a.OccurredAtUtc });
    }
}
