using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProdTrack.Domain.ReasonCodes;
using ProdTrack.Domain.Stations;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Infrastructure.Persistence.Configurations;

internal sealed class OperationConfiguration : IEntityTypeConfiguration<Operation>
{
    public void Configure(EntityTypeBuilder<Operation> builder)
    {
        builder.ToTable("Operation", Schemas.Orders);
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.SetupMinutes).HasPrecision(9, 2);
        builder.Property(o => o.StdMinutesPerUnit).HasPrecision(9, 3);
        builder.Property(o => o.RowVersion).IsRowVersion();
        builder.HasIndex(o => new { o.StationId, o.Status }).IncludeProperties(o => new { o.WorkOrderId, o.Sequence });
        builder.HasIndex(o => new { o.WorkOrderId, o.Sequence });
        builder.HasOne<Station>().WithMany().HasForeignKey(o => o.StationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(o => o.Events).WithOne().HasForeignKey(e => e.OperationId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(o => o.ScrapRecords).WithOne().HasForeignKey(s => s.OperationId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(o => o.Events).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(o => o.ScrapRecords).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class OperationEventConfiguration : IEntityTypeConfiguration<OperationEvent>
{
    public void Configure(EntityTypeBuilder<OperationEvent> builder)
    {
        builder.ToTable("OperationEvent", Schemas.Execution);
        builder.Property(e => e.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.UserId).HasMaxLength(450);
        builder.Property(e => e.DeviceId).HasMaxLength(100);
        builder.HasIndex(e => new { e.OperationId, e.OccurredAtUtc });
        builder.HasIndex(e => e.RequestId).IsUnique().HasFilter("[RequestId] IS NOT NULL");
        builder.HasOne<ReasonCode>().WithMany().HasForeignKey(e => e.ReasonCodeId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ScrapRecordConfiguration : IEntityTypeConfiguration<ScrapRecord>
{
    public void Configure(EntityTypeBuilder<ScrapRecord> builder)
    {
        builder.ToTable("ScrapRecord", Schemas.Execution);
        builder.Property(s => s.Note).HasMaxLength(500);
        builder.Property(s => s.PhotoFileKey).HasMaxLength(300);
        builder.Property(s => s.UserId).HasMaxLength(450);
        builder.HasIndex(s => s.OccurredAtUtc).IncludeProperties(s => new { s.Quantity, s.ReasonCodeId });
        builder.HasOne<ReasonCode>().WithMany().HasForeignKey(s => s.ReasonCodeId).OnDelete(DeleteBehavior.Restrict);
    }
}
