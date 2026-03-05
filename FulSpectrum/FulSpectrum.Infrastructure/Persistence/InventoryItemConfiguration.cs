using FulSpectrum.Domain.Catalog;
using FulSpectrum.Infrastructure.Persistence.Seeds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FulSpectrum.Infrastructure.Persistence.Configurations;

internal sealed class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("Inventory");

        builder.HasKey(x => x.Id);

        builder.HasCheckConstraint("CK_Inventory_QuantityOnHand", "[QuantityOnHand] >= 0");
        builder.HasCheckConstraint("CK_Inventory_ReservedQuantity", "[ReservedQuantity] >= 0");
        builder.HasCheckConstraint("CK_Inventory_ReorderThreshold", "[ReorderThreshold] >= 0");

        builder.HasIndex(x => x.VariantId).IsUnique();

        builder.HasOne(x => x.Variant)
            .WithOne(x => x.Inventory)
            .HasForeignKey<InventoryItem>(x => x.VariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(
            new InventoryItem
            {
                Id = SeedData.HeadphonesBlackInventoryId,
                VariantId = SeedData.HeadphonesBlackVariantId,
                QuantityOnHand = 120,
                ReservedQuantity = 4,
                ReorderThreshold = 10,
                UpdatedAtUtc = SeedData.SeedDate
            },
            new InventoryItem
            {
                Id = SeedData.HeadphonesWhiteInventoryId,
                VariantId = SeedData.HeadphonesWhiteVariantId,
                QuantityOnHand = 90,
                ReservedQuantity = 3,
                ReorderThreshold = 10,
                UpdatedAtUtc = SeedData.SeedDate
            },
            new InventoryItem
            {
                Id = SeedData.LampWarmInventoryId,
                VariantId = SeedData.LampWarmVariantId,
                QuantityOnHand = 55,
                ReservedQuantity = 1,
                ReorderThreshold = 8,
                UpdatedAtUtc = SeedData.SeedDate
            });
    }
}
