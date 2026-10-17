using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace JewelryShop.Infrastructure.Persistence.Configurations;

public class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    // Les colonnes sont en timestamp sans fuseau et contiennent de l'UTC. Npgsql refuse d'y écrire
    // un DateTime de Kind Utc : on l'écrit en Unspecified et on le relit en Utc.
    private static readonly ValueConverter<DateTime, DateTime> _utcConverter = new(
        value => DateTime.SpecifyKind(value, DateTimeKind.Unspecified),
        value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

    private static readonly ValueConverter<DateTime?, DateTime?> _nullableUtcConverter = new(
        value => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Unspecified) : value,
        value => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value);

    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.ToTable("invitations");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");

        builder.Property(i => i.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(i => i.Email);

        builder.Property(i => i.TokenHash)
            .HasColumnName("token_hash")
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(i => i.TokenHash).IsUnique();

        builder.Property(i => i.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(i => i.Status);

        builder.Property(i => i.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamp")
            .HasConversion(_utcConverter)
            .IsRequired();

        builder.Property(i => i.InvitedById).HasColumnName("invited_by_id");

        builder.Property(i => i.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp")
            .HasConversion(_utcConverter)
            .IsRequired();

        builder.Property(i => i.AcceptedAt)
            .HasColumnName("accepted_at")
            .HasColumnType("timestamp")
            .HasConversion(_nullableUtcConverter);

        builder.Property(i => i.AcceptedUserId).HasColumnName("accepted_user_id");

        builder.Property(i => i.RevokedAt)
            .HasColumnName("revoked_at")
            .HasColumnType("timestamp")
            .HasConversion(_nullableUtcConverter);

        builder.HasOne(i => i.InvitedBy)
            .WithMany()
            .HasForeignKey(i => i.InvitedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.AcceptedUser)
            .WithMany()
            .HasForeignKey(i => i.AcceptedUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
