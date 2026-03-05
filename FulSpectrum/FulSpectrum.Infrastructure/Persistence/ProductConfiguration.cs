using FulSpectrum.Domain.Catalog;
using FulSpectrum.Infrastructure.Persistence.Seeds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FulSpectrum.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(180).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Sku).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.BasePrice).HasPrecision(18, 2);

        builder.HasCheckConstraint("CK_Products_BasePrice", "[BasePrice] >= 0");
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.Sku).IsUnique();
        builder.HasIndex(x => new { x.CategoryId, x.IsPublished });

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new Product
            {
                Id = SeedData.HeadphonesProductId,
                CategoryId = SeedData.ElectronicsCategoryId,
                Name = "Pulse ANC Headphones",
                Slug = "pulse-anc-headphones",
                Sku = "PULSE-ANC",
                BasePrice = 149.99m,
                Currency = "USD",
                IsPublished = true,
                CreatedAtUtc = SeedData.SeedDate
            },
            new Product
            {
                Id = SeedData.LampProductId,
                CategoryId = SeedData.HomeCategoryId,
                Name = "Aura Desk Lamp",
                Slug = "aura-desk-lamp",
                Sku = "AURA-LAMP",
                BasePrice = 79.50m,
                Currency = "USD",
                IsPublished = true,
                CreatedAtUtc = SeedData.SeedDate
            });
    }
}
