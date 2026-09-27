using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProdTrack.Domain.Quality;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Infrastructure.Persistence.Configurations;

internal sealed class QcChecklistTemplateConfiguration : IEntityTypeConfiguration<QcChecklistTemplate>
{
    public void Configure(EntityTypeBuilder<QcChecklistTemplate> builder)
    {
        builder.ToTable("QcChecklistTemplate", Schemas.Quality);
        builder.Property(t => t.ProductType).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Name).HasMaxLength(100);
        builder.HasMany(t => t.Items).WithOne().HasForeignKey(i => i.TemplateId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(t => t.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class QcChecklistItemConfiguration : IEntityTypeConfiguration<QcChecklistItem>
{
    public void Configure(EntityTypeBuilder<QcChecklistItem> builder)
    {
        builder.ToTable("QcChecklistItem", Schemas.Quality);
        builder.Property(i => i.Description).HasMaxLength(200);
        builder.Property(i => i.Kind).HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.MinValue).HasPrecision(12, 4);
        builder.Property(i => i.MaxValue).HasPrecision(12, 4);
        builder.Property(i => i.Unit).HasMaxLength(20);
    }
}

internal sealed class QcInspectionConfiguration : IEntityTypeConfiguration<QcInspection>
{
    public void Configure(EntityTypeBuilder<QcInspection> builder)
    {
        builder.ToTable("QcInspection", Schemas.Quality);
        builder.Property(i => i.Result).HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.Disposition).HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.InspectorId).HasMaxLength(450);
        builder.Property(i => i.Notes).HasMaxLength(1000);
        builder.HasOne<Operation>().WithMany().HasForeignKey(i => i.OperationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(i => i.ResultItems).WithOne().HasForeignKey(r => r.InspectionId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(i => i.ResultItems).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class QcResultItemConfiguration : IEntityTypeConfiguration<QcResultItem>
{
    public void Configure(EntityTypeBuilder<QcResultItem> builder)
    {
        builder.ToTable("QcResultItem", Schemas.Quality);
        builder.Property(r => r.MeasuredValue).HasPrecision(12, 4);
        builder.HasOne<QcChecklistItem>().WithMany().HasForeignKey(r => r.ChecklistItemId).OnDelete(DeleteBehavior.Restrict);
    }
}
