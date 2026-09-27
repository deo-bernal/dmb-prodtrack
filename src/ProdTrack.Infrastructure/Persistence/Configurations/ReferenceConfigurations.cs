using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProdTrack.Domain.Reference;

namespace ProdTrack.Infrastructure.Persistence.Configurations;

internal sealed class ColorSchemeConfiguration : IEntityTypeConfiguration<ColorScheme>
{
    public void Configure(EntityTypeBuilder<ColorScheme> builder)
    {
        builder.ToTable("ColorScheme", Schemas.MasterData);
        builder.Property(c => c.Code).HasMaxLength(40);
        builder.Property(c => c.Name).HasMaxLength(100);
        builder.Property(c => c.TextColor).HasMaxLength(30);
        builder.Property(c => c.BackgroundColor).HasMaxLength(30);
        builder.HasIndex(c => c.Code).IsUnique();
    }
}

internal sealed class SignalWordConfiguration : IEntityTypeConfiguration<SignalWord>
{
    public void Configure(EntityTypeBuilder<SignalWord> builder)
    {
        builder.ToTable("SignalWord", Schemas.MasterData);
        builder.Property(s => s.Word).HasMaxLength(40);
        builder.Property(s => s.HeaderColor).HasMaxLength(30);
        builder.Property(s => s.TextColor).HasMaxLength(30);
        builder.HasIndex(s => s.Word).IsUnique();
    }
}
