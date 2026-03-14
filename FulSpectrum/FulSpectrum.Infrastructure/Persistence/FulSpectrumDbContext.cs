//using Microsoft.EntityFrameworkCore;
using FulSpectrum.Domain.Catalog;
using FulSpectrum.Domain.Identity;
using Microsoft.EntityFrameworkCore;
namespace FulSpectrum.Infrastructure.Persistence;

public sealed class FulSpectrumDbContext : DbContext
{
    public FulSpectrumDbContext(DbContextOptions<FulSpectrumDbContext> options) : base(options) { }

    // Etapa 0: sin DbSets todavía. Se agregan en Etapa 1.
    // public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> Variants => Set<ProductVariant>();
    public DbSet<InventoryItem> Inventory => Set<InventoryItem>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<RefreshSession> RefreshSessions => Set<RefreshSession>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FulSpectrumDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
 
