using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProdTrack.Domain.Products;

namespace ProdTrack.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Product", Schemas.MasterData, t => t.HasCheckConstraint("CK_Product_DefaultSpec_IsJson", "ISJSON([DefaultSpec]) = 1"));
        builder.Property(p => p.Sku).HasMaxLength(Product.SkuMaxLength);
        builder.Property(p => p.Name).HasMaxLength(Product.NameMaxLength);
        builder.Property(p => p.ProductType).HasConversion<string>().HasMaxLength(20);
        builder.ComplexProperty(p => p.DefaultSpec, spec => spec.ToJson());
        builder.HasIndex(p => p.Sku).IsUnique();
    }
}
