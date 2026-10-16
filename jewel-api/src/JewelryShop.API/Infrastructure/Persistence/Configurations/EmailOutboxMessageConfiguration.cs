using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace JewelryShop.Infrastructure.Persistence.Configurations;

public class EmailOutboxMessageConfiguration : IEntityTypeConfiguration<EmailOutboxMessage>
{
    // Les colonnes sont en timestamp sans fuseau et contiennent de l'UTC. Npgsql refuse d'y écrire
    // un DateTime de Kind Utc : on l'écrit en Unspecified et on le relit en Utc.
    private static readonly ValueConverter<DateTime, DateTime> _utcConverter = new(
        value => DateTime.SpecifyKind(value, DateTimeKind.Unspecified),
        value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

    private static readonly ValueConverter<DateTime?, DateTime?> _nullableUtcConverter = new(
        value => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Unspecified) : value,
        value => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value);

    public void Configure(EntityTypeBuilder<EmailOutboxMessage> builder)
    {
        builder.ToTable("email_outbox");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");

        builder.Property(m => m.ToAddress)
            .HasColumnName("to_address")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(m => m.Subject)
            .HasColumnName("subject")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(m => m.HtmlBody).HasColumnName("html_body").IsRequired();
        builder.Property(m => m.TextBody).HasColumnName("text_body").IsRequired();

        builder.Property(m => m.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(m => m.Attempts).HasColumnName("attempts").IsRequired();

        builder.Property(m => m.NextAttemptAt)
            .HasColumnName("next_attempt_at")
            .HasColumnType("timestamp")
            .HasConversion(_utcConverter)
            .IsRequired();

        builder.Property(m => m.LastError).HasColumnName("last_error");

        builder.Property(m => m.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp")
            .HasConversion(_utcConverter)
            .IsRequired();

        builder.Property(m => m.SentAt)
            .HasColumnName("sent_at")
            .HasColumnType("timestamp")
            .HasConversion(_nullableUtcConverter);
    }
}
