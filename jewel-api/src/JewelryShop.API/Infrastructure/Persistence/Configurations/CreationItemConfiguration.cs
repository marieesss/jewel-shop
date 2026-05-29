using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryShop.Infrastructure.Persistence.Configurations;

public class CreationItemConfiguration : IEntityTypeConfiguration<CreationItem>
{
    public void Configure(EntityTypeBuilder<CreationItem> builder)
    {
        builder.ToTable("creation_items");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");

        builder.Property(i => i.CreationId).HasColumnName("creation_id").IsRequired();
        builder.Property(i => i.CharmId).HasColumnName("charm_id").IsRequired();
        builder.Property(i => i.Position).HasColumnName("position").IsRequired();

        builder.HasOne(i => i.Charm)
            .WithMany(c => c.CreationItems)
            .HasForeignKey(i => i.CharmId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
