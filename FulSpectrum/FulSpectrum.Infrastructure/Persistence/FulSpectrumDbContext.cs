using Microsoft.EntityFrameworkCore;

namespace FulSpectrum.Infrastructure.Persistence;

public sealed class FulSpectrumDbContext : DbContext
{
    public FulSpectrumDbContext(DbContextOptions<FulSpectrumDbContext> options) : base(options) { }

    // Etapa 0: sin DbSets todavía. Se agregan en Etapa 1.
    // public DbSet<Product> Products => Set<Product>();
}