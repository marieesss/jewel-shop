using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryShop.Infrastructure.Persistence.Configurations;

public class CharmConfiguration : IEntityTypeConfiguration<Charm>
{
    public void Configure(EntityTypeBuilder<Charm> builder)
    {
        builder.ToTable("charms");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasColumnName("description")
            .HasColumnType("text")
            .HasDefaultValue(string.Empty)
            .IsRequired();

        builder.Property(c => c.Color)
            .HasColumnName("color")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(c => c.ImageUrl)
            .HasColumnName("image_url")
            .HasMaxLength(500);

        builder.Property(c => c.Cost)
            .HasColumnName("cost")
            .HasColumnType("numeric(10,2)")
            .IsRequired();

        builder.Property(c => c.Price)
            .HasColumnName("price")
            .HasColumnType("numeric(10,2)")
            .IsRequired();

        builder.Property(c => c.Stock)
            .HasColumnName("stock")
            .HasDefaultValue(0)
            .IsRequired();
    }
}
