using Microsoft.EntityFrameworkCore;

namespace JewelryShop.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Chain> Chains => Set<Chain>();
    public DbSet<ChainLength> ChainLengths => Set<ChainLength>();
    public DbSet<Charm> Charms => Set<Charm>();
    public DbSet<Option> Options => Set<Option>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Creation> Creations => Set<Creation>();
    public DbSet<CreationItem> CreationItems => Set<CreationItem>();
    public DbSet<EmailOutboxMessage> EmailOutboxMessages => Set<EmailOutboxMessage>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<ChainTag> ChainTags => Set<ChainTag>();
    public DbSet<CharmTag> CharmTags => Set<CharmTag>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<OrderStatusHistory> OrderStatusHistory => Set<OrderStatusHistory>();
    public DbSet<OrderNote> OrderNotes => Set<OrderNote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Applique toutes les IEntityTypeConfiguration<> de cet assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
