using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryShop.Infrastructure.Persistence.Configurations;

public class ChainRollConfiguration : IEntityTypeConfiguration<ChainRoll>
{
    public void Configure(EntityTypeBuilder<ChainRoll> builder)
    {
        builder.ToTable("chain_rolls");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");

        builder.Property(r => r.ChainId).HasColumnName("chain_id").IsRequired();

        builder.Property(r => r.InitialLengthCm)
            .HasColumnName("initial_length_cm")
            .IsRequired();

        builder.Property(r => r.RemainingLengthCm)
            .HasColumnName("remaining_length_cm")
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasIndex(r => r.ChainId).HasDatabaseName("ix_chain_rolls_chain_id");
    }
}
