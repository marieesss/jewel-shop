using Microsoft.EntityFrameworkCore;

namespace JewelryShop.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Chain> Chains => Set<Chain>();
    public DbSet<Charm> Charms => Set<Charm>();
    public DbSet<Option> Options => Set<Option>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Creation> Creations => Set<Creation>();
    public DbSet<CreationItem> CreationItems => Set<CreationItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Applique toutes les IEntityTypeConfiguration<> de cet assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
