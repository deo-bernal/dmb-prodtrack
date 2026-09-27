using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProdTrack.Domain.ReasonCodes;

namespace ProdTrack.Infrastructure.Persistence.Configurations;

internal sealed class ReasonCodeConfiguration : IEntityTypeConfiguration<ReasonCode>
{
    public void Configure(EntityTypeBuilder<ReasonCode> builder)
    {
        builder.ToTable("ReasonCode", Schemas.MasterData);
        builder.Property(r => r.Code).HasMaxLength(ReasonCode.CodeMaxLength);
        builder.Property(r => r.Description).HasMaxLength(ReasonCode.DescriptionMaxLength);
        builder.Property(r => r.Category).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(r => new { r.Category, r.Code }).IsUnique();
    }
}
