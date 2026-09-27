using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ProdTrack.Infrastructure.Persistence.Configurations;

internal sealed class NumberSequenceConfiguration : IEntityTypeConfiguration<NumberSequence>
{
    public void Configure(EntityTypeBuilder<NumberSequence> builder)
    {
        builder.ToTable("NumberSequence", Schemas.Orders);
        builder.HasKey(s => new { s.Name, s.Year });
        builder.Property(s => s.Name).HasMaxLength(10);
        builder.Property(s => s.RowVersion).IsRowVersion();
    }
}
