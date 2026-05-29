using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryShop.Infrastructure.Persistence.Configurations;

public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.ToTable("favorites", t =>
            t.HasCheckConstraint(
                "chk_favorites_one_product",
                "(chain_id IS NOT NULL)::int + (charm_id IS NOT NULL)::int = 1"));

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).HasColumnName("id");

        builder.Property(f => f.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(f => f.ChainId).HasColumnName("chain_id");
        builder.Property(f => f.CharmId).HasColumnName("charm_id");

        // La propriété calculée IsValid ne doit pas être mappée
        builder.Ignore(f => f.IsValid);

        // Unicité : un même produit n'est favori qu'une fois par utilisateur
        builder.HasIndex(f => new { f.UserId, f.ChainId, f.CharmId })
            .IsUnique()
            .HasDatabaseName("uq_favorites");

        builder.HasOne(f => f.Chain)
            .WithMany(c => c.Favorites)
            .HasForeignKey(f => f.ChainId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Charm)
            .WithMany(c => c.Favorites)
            .HasForeignKey(f => f.CharmId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
