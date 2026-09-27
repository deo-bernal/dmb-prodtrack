using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProdTrack.Domain.Products;
using ProdTrack.Domain.SalesOrders;

namespace ProdTrack.Infrastructure.Persistence.Configurations;

internal sealed class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("SalesOrder", Schemas.Orders);
        builder.Property(s => s.Number).HasMaxLength(20);
        builder.Property(s => s.CustomerName).HasMaxLength(200);
        builder.Property(s => s.PoNumber).HasMaxLength(50);
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(s => s.Number).IsUnique();
        builder.HasMany(s => s.Lines).WithOne().HasForeignKey(l => l.SalesOrderId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(s => s.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class SalesOrderLineConfiguration : IEntityTypeConfiguration<SalesOrderLine>
{
    public void Configure(EntityTypeBuilder<SalesOrderLine> builder)
    {
        builder.ToTable("SalesOrderLine", Schemas.Orders, t => t.HasCheckConstraint("CK_SalesOrderLine_Spec_IsJson", "ISJSON([Spec]) = 1"));
        builder.Property(l => l.Legend).HasMaxLength(500);
        builder.ComplexProperty(l => l.Spec, spec => spec.ToJson());
        builder.HasIndex(l => new { l.SalesOrderId, l.LineNumber }).IsUnique();
        builder.HasOne<Product>().WithMany().HasForeignKey(l => l.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}
