using FulSpectrum.Domain.Catalog;
using FulSpectrum.Infrastructure.Persistence.Seeds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FulSpectrum.Infrastructure.Persistence.Configurations;

internal sealed class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("Variants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.VariantSku).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.PriceDelta).HasPrecision(18, 2);

        builder.HasCheckConstraint("CK_Variants_PriceDelta", "[PriceDelta] >= 0");
        builder.HasIndex(x => x.VariantSku).IsUnique();
        builder.HasIndex(x => new { x.ProductId, x.IsDefault });

        builder.HasOne(x => x.Product)
            .WithMany(x => x.Variants)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(
            new ProductVariant
            {
                Id = SeedData.HeadphonesBlackVariantId,
                ProductId = SeedData.HeadphonesProductId,
                VariantSku = "PULSE-ANC-BLK",
                Name = "Black",
                PriceDelta = 0,
                IsDefault = true
            },
            new ProductVariant
            {
                Id = SeedData.HeadphonesWhiteVariantId,
                ProductId = SeedData.HeadphonesProductId,
                VariantSku = "PULSE-ANC-WHT",
                Name = "White",
                PriceDelta = 5,
                IsDefault = false
            },
            new ProductVariant
            {
                Id = SeedData.LampWarmVariantId,
                ProductId = SeedData.LampProductId,
                VariantSku = "AURA-LAMP-WARM",
                Name = "Warm Light",
                PriceDelta = 0,
                IsDefault = true
            });
    }
}
