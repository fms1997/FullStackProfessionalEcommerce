using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FulSpectrum.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ResyncSeedProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "BasePrice", "CategoryId", "CreatedAtUtc", "Currency", "IsPublished", "Name", "Sku", "Slug" },
                values: new object[,]
                {
            {
                new Guid("ec5ba7d8-03a6-4a15-a97c-41f7f7cc8d79"),
                149.99m,
                new Guid("30d0f5fa-c46f-4df0-82c5-f39b4af6f1d2"),
                new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc),
                "USD",
                true,
                "Pulse ANC Headphones",
                "PULSE-ANC",
                "pulse-anc-headphones"
            },
            {
                new Guid("9fdb4abb-6d98-4f1f-a3ac-9034b40f6de2"),
                79.50m,
                new Guid("6ac41c49-72f2-4ee2-a9ac-4f612869321b"),
                new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc),
                "USD",
                true,
                "Aura Desk Lamp",
                "AURA-LAMP",
                "aura-desk-lamp"
            }
                });

            migrationBuilder.InsertData(
                table: "Variants",
                columns: new[] { "Id", "IsDefault", "Name", "PriceDelta", "ProductId", "VariantSku" },
                values: new object[,]
                {
            {
                new Guid("cc589f42-5ff4-40e8-ad89-c4f80d60af60"),
                true,
                "Black",
                0m,
                new Guid("ec5ba7d8-03a6-4a15-a97c-41f7f7cc8d79"),
                "PULSE-ANC-BLK"
            },
            {
                new Guid("6cce416a-16b0-4dd3-8852-7f69f0f304f4"),
                false,
                "White",
                5m,
                new Guid("ec5ba7d8-03a6-4a15-a97c-41f7f7cc8d79"),
                "PULSE-ANC-WHT"
            },
            {
                new Guid("d49631cf-bd4f-418f-b425-d85fa6ea2a7a"),
                true,
                "Warm Light",
                0m,
                new Guid("9fdb4abb-6d98-4f1f-a3ac-9034b40f6de2"),
                "AURA-LAMP-WARM"
            }
                });

            migrationBuilder.InsertData(
                table: "Inventory",
                columns: new[] { "Id", "QuantityOnHand", "ReorderThreshold", "ReservedQuantity", "UpdatedAtUtc", "VariantId" },
                values: new object[,]
                {
            {
                new Guid("4fcd4fbc-4c8b-49ca-8d66-77dcac0a8475"),
                120,
                10,
                4,
                new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc),
                new Guid("cc589f42-5ff4-40e8-ad89-c4f80d60af60")
            },
            {
                new Guid("d4f8c27d-5e6c-45f5-bd66-c7379a8b0452"),
                90,
                10,
                3,
                new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc),
                new Guid("6cce416a-16b0-4dd3-8852-7f69f0f304f4")
            },
            {
                new Guid("f86fda30-5ae7-40de-a979-cab72a24353f"),
                55,
                8,
                1,
                new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc),
                new Guid("d49631cf-bd4f-418f-b425-d85fa6ea2a7a")
            }
                });
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Inventory",
                keyColumn: "Id",
                keyValues: new object[]
                {
            new Guid("4fcd4fbc-4c8b-49ca-8d66-77dcac0a8475"),
            new Guid("d4f8c27d-5e6c-45f5-bd66-c7379a8b0452"),
            new Guid("f86fda30-5ae7-40de-a979-cab72a24353f")
                });
        }
    }
}
