using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProdTrack.Domain.Products;
using ProdTrack.Domain.Routings;
using ProdTrack.Domain.SalesOrders;
using ProdTrack.Domain.WorkOrders;

namespace ProdTrack.Infrastructure.Persistence.Configurations;

internal sealed class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.ToTable("WorkOrder", Schemas.Orders, t => t.HasCheckConstraint("CK_WorkOrder_Spec_IsJson", "ISJSON([Spec]) = 1"));
        builder.Property(w => w.Number).HasMaxLength(WorkOrder.NumberMaxLength);
        builder.Property(w => w.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(w => w.ProductType).HasConversion<string>().HasMaxLength(20);
        builder.Property(w => w.CustomerName).HasMaxLength(WorkOrder.CustomerMaxLength);
        builder.Property(w => w.Legend).HasMaxLength(WorkOrder.LegendMaxLength);
        builder.Property(w => w.HoldReason).HasMaxLength(200);
        builder.ComplexProperty(w => w.Spec, spec => spec.ToJson());

        builder.HasIndex(w => w.Number).IsUnique();
        builder.HasIndex(w => new { w.Status, w.DueDate });
        builder.HasIndex(w => w.SalesOrderLineId);

        builder.HasOne<Product>().WithMany().HasForeignKey(w => w.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Routing>().WithMany().HasForeignKey(w => w.RoutingId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<SalesOrderLine>().WithMany().HasForeignKey(w => w.SalesOrderLineId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(w => w.Operations).WithOne().HasForeignKey(o => o.WorkOrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(w => w.ArtworkProofs).WithOne().HasForeignKey(p => p.WorkOrderId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(w => w.Operations).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(w => w.ArtworkProofs).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class ArtworkProofConfiguration : IEntityTypeConfiguration<ArtworkProof>
{
    public void Configure(EntityTypeBuilder<ArtworkProof> builder)
    {
        builder.ToTable("ArtworkProof", Schemas.Orders);
        builder.Property(p => p.FileKey).HasMaxLength(300);
        builder.Property(p => p.ContentType).HasMaxLength(100);
        builder.Property(p => p.OriginalFileName).HasMaxLength(255);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.DecisionNote).HasMaxLength(500);
        builder.Property(p => p.DecidedBy).HasMaxLength(256);
        builder.Property(p => p.UploadedBy).HasMaxLength(256);
        builder.HasIndex(p => new { p.WorkOrderId, p.Version }).IsUnique();
    }
}
