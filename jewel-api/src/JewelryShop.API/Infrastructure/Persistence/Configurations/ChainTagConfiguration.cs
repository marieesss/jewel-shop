using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryShop.Infrastructure.Persistence.Configurations;

public class ChainTagConfiguration : IEntityTypeConfiguration<ChainTag>
{
    public void Configure(EntityTypeBuilder<ChainTag> builder)
    {
        builder.ToTable("chain_tags");

        builder.HasKey(ct => new { ct.ChainId, ct.TagId });

        builder.Property(ct => ct.ChainId).HasColumnName("chain_id");
        builder.Property(ct => ct.TagId).HasColumnName("tag_id");

        builder.HasOne<Chain>()
            .WithMany(c => c.ChainTags)
            .HasForeignKey(ct => ct.ChainId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
