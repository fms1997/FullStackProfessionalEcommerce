using FulSpectrum.Domain.Catalog;
using FulSpectrum.Infrastructure.Persistence.Seeds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FulSpectrum.Infrastructure.Persistence.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(140).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(512);

        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => new { x.IsActive, x.Name });

        builder.HasData(
            new Category
            {
                Id = SeedData.ElectronicsCategoryId,
                Name = "Electronics",
                Slug = "electronics",
                Description = "Devices and gadgets",
                IsActive = true,
                CreatedAtUtc = SeedData.SeedDate
            },
            new Category
            {
                Id = SeedData.HomeCategoryId,
                Name = "Home",
                Slug = "home",
                Description = "Home essentials",
                IsActive = true,
                CreatedAtUtc = SeedData.SeedDate
            });
    }
}
