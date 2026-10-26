using JewelryShop.Features.ChainLengths.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryShop.Infrastructure.Persistence.Configurations;

public class ChainLengthConfiguration : IEntityTypeConfiguration<ChainLength>
{
    public void Configure(EntityTypeBuilder<ChainLength> builder)
    {
        builder.ToTable("chain_lengths");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasColumnName("id");

        builder.Property(l => l.LengthCm)
            .HasColumnName("length_cm")
            .IsRequired();

        builder.HasIndex(l => l.LengthCm).IsUnique();

        builder.Property(l => l.Label)
            .HasColumnName("label")
            .HasMaxLength(ChainLengthRules.LabelMaxLength)
            .IsRequired();
    }
}
