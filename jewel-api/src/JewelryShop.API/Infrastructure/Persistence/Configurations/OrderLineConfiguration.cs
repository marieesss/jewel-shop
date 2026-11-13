using JewelryShop.Features.Orders.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryShop.Infrastructure.Persistence.Configurations;

public class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("order_lines");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasColumnName("id");

        builder.Property(l => l.OrderId).HasColumnName("order_id").IsRequired();
        builder.HasIndex(l => l.OrderId).HasDatabaseName("ix_order_lines_order_id");

        builder.Property(l => l.Kind)
            .HasColumnName("kind")
            .HasConversion<string>()
            .HasMaxLength(OrderRules.LineKindMaxLength)
            .IsRequired();

        builder.Property(l => l.ChainId).HasColumnName("chain_id");
        builder.HasIndex(l => l.ChainId).HasDatabaseName("ix_order_lines_chain_id");

        builder.Property(l => l.CharmId).HasColumnName("charm_id");
        builder.HasIndex(l => l.CharmId).HasDatabaseName("ix_order_lines_charm_id");

        builder.Property(l => l.ChainRollId).HasColumnName("chain_roll_id");
        builder.HasIndex(l => l.ChainRollId).HasDatabaseName("ix_order_lines_chain_roll_id");

        builder.Property(l => l.Name)
            .HasColumnName("name")
            .HasMaxLength(OrderRules.LineNameMaxLength)
            .IsRequired();

        builder.Property(l => l.LengthCm).HasColumnName("length_cm");

        builder.Property(l => l.PricePerMeter)
            .HasColumnName("price_per_meter")
            .HasColumnType("numeric(10,2)");

        builder.Property(l => l.ClaspPrice)
            .HasColumnName("clasp_price")
            .HasColumnType("numeric(10,2)");

        builder.Property(l => l.UnitPrice)
            .HasColumnName("unit_price")
            .HasColumnType("numeric(10,2)")
            .IsRequired();

        builder.Property(l => l.Position).HasColumnName("position");

        builder.HasOne<Chain>()
            .WithMany()
            .HasForeignKey(l => l.ChainId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Charm>()
            .WithMany()
            .HasForeignKey(l => l.CharmId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ChainRoll>()
            .WithMany()
            .HasForeignKey(l => l.ChainRollId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
