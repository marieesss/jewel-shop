using JewelryShop.Features.Tags.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JewelryShop.Infrastructure.Persistence.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("tags");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");

        builder.Property(t => t.Name)
            .HasColumnName("name")
            .HasMaxLength(TagRules.NameMaxLength)
            .IsRequired();

        builder.Property(t => t.Slug)
            .HasColumnName("slug")
            .HasMaxLength(TagRules.SlugMaxLength)
            .IsRequired();

        builder.Property(t => t.Category)
            .HasColumnName("category")
            .HasConversion<string>()
            .HasMaxLength(TagRules.CategoryMaxLength)
            .IsRequired();

        builder.HasIndex(t => new { t.Category, t.Slug }).IsUnique();

        builder.HasMany(t => t.ChainTags)
            .WithOne(ct => ct.Tag)
            .HasForeignKey(ct => ct.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.CharmTags)
            .WithOne(ct => ct.Tag)
            .HasForeignKey(ct => ct.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
