using JewelryShop.Features.Orders.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryShop.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasColumnName("id");

        builder.Property(o => o.Number)
            .HasColumnName("number")
            .HasMaxLength(OrderRules.NumberMaxLength)
            .IsRequired();
        builder.HasIndex(o => o.Number).IsUnique();

        builder.Property(o => o.UserId).HasColumnName("user_id").IsRequired();
        builder.HasIndex(o => o.UserId).HasDatabaseName("ix_orders_user_id");

        builder.Property(o => o.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(OrderRules.StatusMaxLength)
            .IsRequired();

        builder.Property(o => o.Comment)
            .HasColumnName("comment")
            .HasMaxLength(OrderRules.CommentMaxLength);

        builder.Property(o => o.RecipientName)
            .HasColumnName("recipient_name")
            .HasMaxLength(OrderRules.RecipientNameMaxLength)
            .IsRequired();

        builder.Property(o => o.RecipientSurname)
            .HasColumnName("recipient_surname")
            .HasMaxLength(OrderRules.RecipientNameMaxLength)
            .IsRequired();

        builder.Property(o => o.RecipientPhone)
            .HasColumnName("recipient_phone")
            .HasMaxLength(OrderRules.RecipientPhoneMaxLength)
            .IsRequired();

        builder.Property(o => o.AddressLine1)
            .HasColumnName("address_line1")
            .HasMaxLength(OrderRules.AddressLineMaxLength)
            .IsRequired();

        builder.Property(o => o.AddressLine2)
            .HasColumnName("address_line2")
            .HasMaxLength(OrderRules.AddressLineMaxLength);

        builder.Property(o => o.PostalCode)
            .HasColumnName("postal_code")
            .HasMaxLength(OrderRules.PostalCodeMaxLength)
            .IsRequired();

        builder.Property(o => o.City)
            .HasColumnName("city")
            .HasMaxLength(OrderRules.CityMaxLength)
            .IsRequired();

        builder.Property(o => o.Total)
            .HasColumnName("total")
            .HasColumnType("numeric(10,2)")
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Lines)
            .WithOne(l => l.Order)
            .HasForeignKey(l => l.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.StatusHistory)
            .WithOne(h => h.Order)
            .HasForeignKey(h => h.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Notes)
            .WithOne(n => n.Order)
            .HasForeignKey(n => n.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
