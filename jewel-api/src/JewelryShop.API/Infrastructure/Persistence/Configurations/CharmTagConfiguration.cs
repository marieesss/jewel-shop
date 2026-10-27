using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryShop.Infrastructure.Persistence.Configurations;

public class CharmTagConfiguration : IEntityTypeConfiguration<CharmTag>
{
    public void Configure(EntityTypeBuilder<CharmTag> builder)
    {
        builder.ToTable("charm_tags");

        builder.HasKey(ct => new { ct.CharmId, ct.TagId });

        builder.Property(ct => ct.CharmId).HasColumnName("charm_id");
        builder.Property(ct => ct.TagId).HasColumnName("tag_id");

        builder.HasOne<Charm>()
            .WithMany(c => c.CharmTags)
            .HasForeignKey(ct => ct.CharmId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
