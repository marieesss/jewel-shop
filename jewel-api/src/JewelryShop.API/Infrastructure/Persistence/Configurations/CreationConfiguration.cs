using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryShop.Infrastructure.Persistence.Configurations;

public class CreationConfiguration : IEntityTypeConfiguration<Creation>
{
    public void Configure(EntityTypeBuilder<Creation> builder)
    {
        builder.ToTable("creations");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(c => c.ChainId).HasColumnName("chain_id").IsRequired();
        builder.Property(c => c.OptionId).HasColumnName("option_id").IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp")
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.HasOne(c => c.Chain)
            .WithMany(ch => ch.Creations)
            .HasForeignKey(c => c.ChainId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Option)
            .WithMany(o => o.Creations)
            .HasForeignKey(c => c.OptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Items)
            .WithOne(i => i.Creation)
            .HasForeignKey(i => i.CreationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
