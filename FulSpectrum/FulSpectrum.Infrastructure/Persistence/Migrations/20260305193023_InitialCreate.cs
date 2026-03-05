using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FulSpectrum.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(140)", maxLength: 140, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.CheckConstraint("CK_Products_BasePrice", "[BasePrice] >= 0");
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Variants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VariantSku = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    PriceDelta = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Variants", x => x.Id);
                    table.CheckConstraint("CK_Variants_PriceDelta", "[PriceDelta] >= 0");
                    table.ForeignKey(
                        name: "FK_Variants_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VariantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuantityOnHand = table.Column<int>(type: "int", nullable: false),
                    ReservedQuantity = table.Column<int>(type: "int", nullable: false),
                    ReorderThreshold = table.Column<int>(type: "int", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventory", x => x.Id);
                    table.CheckConstraint("CK_Inventory_QuantityOnHand", "[QuantityOnHand] >= 0");
                    table.CheckConstraint("CK_Inventory_ReorderThreshold", "[ReorderThreshold] >= 0");
                    table.CheckConstraint("CK_Inventory_ReservedQuantity", "[ReservedQuantity] >= 0");
                    table.ForeignKey(
                        name: "FK_Inventory_Variants_VariantId",
                        column: x => x.VariantId,
                        principalTable: "Variants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAtUtc", "Description", "IsActive", "Name", "Slug" },
                values: new object[,]
                {
                    { new Guid("30d0f5fa-c46f-4df0-82c5-f39b4af6f1d2"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Devices and gadgets", true, "Electronics", "electronics" },
                    { new Guid("6ac41c49-72f2-4ee2-a9ac-4f612869321b"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Home essentials", true, "Home", "home" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAtUtc", "Email", "FirstName", "IsActive", "LastName", "NormalizedEmail", "PasswordHash" },
                values: new object[] { new Guid("af2fbf41-5fb8-4840-a8d0-a869b9159ff9"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@fulspectrum.local", "System", true, "Admin", "ADMIN@FULSPECTRUM.LOCAL", "$2a$11$example.hash.replace.in.real.env" });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "BasePrice", "CategoryId", "CreatedAtUtc", "Currency", "IsPublished", "Name", "Sku", "Slug" },
                values: new object[,]
                {
                    { new Guid("9fdb4abb-6d98-4f1f-a3ac-9034b40f6de2"), 79.50m, new Guid("6ac41c49-72f2-4ee2-a9ac-4f612869321b"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "USD", true, "Aura Desk Lamp", "AURA-LAMP", "aura-desk-lamp" },
                    { new Guid("ec5ba7d8-03a6-4a15-a97c-41f7f7cc8d79"), 149.99m, new Guid("30d0f5fa-c46f-4df0-82c5-f39b4af6f1d2"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "USD", true, "Pulse ANC Headphones", "PULSE-ANC", "pulse-anc-headphones" }
                });

            migrationBuilder.InsertData(
                table: "Variants",
                columns: new[] { "Id", "IsDefault", "Name", "PriceDelta", "ProductId", "VariantSku" },
                values: new object[,]
                {
                    { new Guid("6cce416a-16b0-4dd3-8852-7f69f0f304f4"), false, "White", 5m, new Guid("ec5ba7d8-03a6-4a15-a97c-41f7f7cc8d79"), "PULSE-ANC-WHT" },
                    { new Guid("cc589f42-5ff4-40e8-ad89-c4f80d60af60"), true, "Black", 0m, new Guid("ec5ba7d8-03a6-4a15-a97c-41f7f7cc8d79"), "PULSE-ANC-BLK" },
                    { new Guid("d49631cf-bd4f-418f-b425-d85fa6ea2a7a"), true, "Warm Light", 0m, new Guid("9fdb4abb-6d98-4f1f-a3ac-9034b40f6de2"), "AURA-LAMP-WARM" }
                });

            migrationBuilder.InsertData(
                table: "Inventory",
                columns: new[] { "Id", "QuantityOnHand", "ReorderThreshold", "ReservedQuantity", "UpdatedAtUtc", "VariantId" },
                values: new object[,]
                {
                    { new Guid("4fcd4fbc-4c8b-49ca-8d66-77dcac0a8475"), 120, 10, 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("cc589f42-5ff4-40e8-ad89-c4f80d60af60") },
                    { new Guid("d4f8c27d-5e6c-45f5-bd66-c7379a8b0452"), 90, 10, 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("6cce416a-16b0-4dd3-8852-7f69f0f304f4") },
                    { new Guid("f86fda30-5ae7-40de-a979-cab72a24353f"), 55, 8, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("d49631cf-bd4f-418f-b425-d85fa6ea2a7a") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_IsActive_Name",
                table: "Categories",
                columns: new[] { "IsActive", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Slug",
                table: "Categories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_VariantId",
                table: "Inventory",
                column: "VariantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId_IsPublished",
                table: "Products",
                columns: new[] { "CategoryId", "IsPublished" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Sku",
                table: "Products",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Slug",
                table: "Products",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsActive_CreatedAtUtc",
                table: "Users",
                columns: new[] { "IsActive", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedEmail",
                table: "Users",
                column: "NormalizedEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Variants_ProductId_IsDefault",
                table: "Variants",
                columns: new[] { "ProductId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_Variants_VariantSku",
                table: "Variants",
                column: "VariantSku",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Inventory");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Variants");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
