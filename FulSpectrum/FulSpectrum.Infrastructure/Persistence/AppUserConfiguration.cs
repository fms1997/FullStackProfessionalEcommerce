using FulSpectrum.Domain.Identity;
using FulSpectrum.Infrastructure.Persistence.Seeds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FulSpectrum.Infrastructure.Persistence.Configurations;

internal sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.Property(x => x.NormalizedEmail).HasMaxLength(256).IsRequired();
        builder.Property(x => x.FirstName).HasMaxLength(80).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(80).IsRequired();
        builder.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();

        builder.HasIndex(x => x.NormalizedEmail).IsUnique();
        builder.HasIndex(x => new { x.IsActive, x.CreatedAtUtc });

        builder.HasData(new AppUser
        {
            Id = SeedData.AdminUserId,
            Email = "admin@fulspectrum.local",
            NormalizedEmail = "ADMIN@FULSPECTRUM.LOCAL",
            FirstName = "System",
            LastName = "Admin",
            PasswordHash = "$2a$11$example.hash.replace.in.real.env",
            IsActive = true,
            CreatedAtUtc = SeedData.SeedDate
        });
    }
}
