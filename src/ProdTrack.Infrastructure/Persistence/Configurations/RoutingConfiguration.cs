using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProdTrack.Domain.Routings;
using ProdTrack.Domain.Stations;

namespace ProdTrack.Infrastructure.Persistence.Configurations;

internal sealed class RoutingConfiguration : IEntityTypeConfiguration<Routing>
{
    public void Configure(EntityTypeBuilder<Routing> builder)
    {
        builder.ToTable("Routing", Schemas.MasterData);
        builder.Property(r => r.ProductType).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Name).HasMaxLength(Routing.NameMaxLength);
        builder.HasIndex(r => new { r.ProductType, r.Version }).IsUnique();
        builder.HasMany(r => r.Steps).WithOne().HasForeignKey(s => s.RoutingId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(r => r.Steps).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class RoutingStepConfiguration : IEntityTypeConfiguration<RoutingStep>
{
    public void Configure(EntityTypeBuilder<RoutingStep> builder)
    {
        builder.ToTable("RoutingStep", Schemas.MasterData);
        builder.Property(s => s.SetupMinutes).HasPrecision(9, 2);
        builder.Property(s => s.StdMinutesPerUnit).HasPrecision(9, 3);
        builder.HasIndex(s => new { s.RoutingId, s.Sequence }).IsUnique();
        builder.HasOne<Station>().WithMany().HasForeignKey(s => s.StationId).OnDelete(DeleteBehavior.Restrict);
    }
}
