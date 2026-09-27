using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProdTrack.Domain.Stations;

namespace ProdTrack.Infrastructure.Persistence.Configurations;

internal sealed class StationConfiguration : IEntityTypeConfiguration<Station>
{
    public void Configure(EntityTypeBuilder<Station> builder)
    {
        builder.ToTable("Station", Schemas.MasterData);
        builder.Property(s => s.Code).HasMaxLength(Station.CodeMaxLength);
        builder.Property(s => s.Name).HasMaxLength(Station.NameMaxLength);
        builder.Property(s => s.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(s => s.WorkCenter).HasMaxLength(50);
        builder.HasIndex(s => s.Code).IsUnique();
    }
}
