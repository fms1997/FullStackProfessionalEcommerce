using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;

namespace FulSpectrum.Infrastructure.Persistence;
 
public class FulSpectrumDbContextFactory : IDesignTimeDbContextFactory<FulSpectrumDbContext>
{
    public FulSpectrumDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../FulSpectrum.Api"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<FulSpectrumDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new FulSpectrumDbContext(optionsBuilder.Options);
    }
}