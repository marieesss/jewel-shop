using JewelryShop.Features.Orders.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryShop.Infrastructure.Persistence.Configurations;

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("order_status_history");

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).HasColumnName("id");

        builder.Property(h => h.OrderId).HasColumnName("order_id").IsRequired();
        builder.HasIndex(h => h.OrderId).HasDatabaseName("ix_order_status_history_order_id");

        builder.Property(h => h.FromStatus)
            .HasColumnName("from_status")
            .HasConversion<string>()
            .HasMaxLength(OrderRules.StatusMaxLength);

        builder.Property(h => h.ToStatus)
            .HasColumnName("to_status")
            .HasConversion<string>()
            .HasMaxLength(OrderRules.StatusMaxLength)
            .IsRequired();

        builder.Property(h => h.ChangedBy).HasColumnName("changed_by").IsRequired();

        builder.Property(h => h.Message)
            .HasColumnName("message")
            .HasMaxLength(OrderRules.StatusMessageMaxLength);

        builder.Property(h => h.CustomerNotified)
            .HasColumnName("customer_notified")
            .IsRequired();

        builder.Property(h => h.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(h => h.ChangedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
