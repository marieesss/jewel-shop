using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryShop.Infrastructure.Persistence.Configurations;

public class ChainConfiguration : IEntityTypeConfiguration<Chain>
{
    public void Configure(EntityTypeBuilder<Chain> builder)
    {
        builder.ToTable("chains");

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

        builder.Property(c => c.Url)
            .HasColumnName("url")
            .HasMaxLength(500);

        builder.Property(c => c.Cost)
            .HasColumnName("cost")
            .HasColumnType("numeric(10,2)");

        builder.Property(c => c.Price)
            .HasColumnName("price")
            .HasColumnType("numeric(10,2)");

        builder.Property(c => c.Length)
            .HasColumnName("length")
            .HasColumnType("numeric(6,2)");

        builder.Property(c => c.CostPerMeter)
            .HasColumnName("cost_per_meter")
            .HasColumnType("numeric(10,2)");

        builder.Property(c => c.PricePerMeter)
            .HasColumnName("price_per_meter")
            .HasColumnType("numeric(10,2)");

        builder.Property(c => c.ClaspCost)
            .HasColumnName("clasp_cost")
            .HasColumnType("numeric(10,2)")
            .IsRequired();

        builder.Property(c => c.ClaspPrice)
            .HasColumnName("clasp_price")
            .HasColumnType("numeric(10,2)")
            .IsRequired();

        builder.Ignore(c => c.StockCm);

        builder.HasMany(c => c.Rolls)
            .WithOne()
            .HasForeignKey(r => r.ChainId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
