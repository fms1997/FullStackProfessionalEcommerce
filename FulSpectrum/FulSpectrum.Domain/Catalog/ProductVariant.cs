namespace FulSpectrum.Domain.Catalog;

public sealed class ProductVariant
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string VariantSku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal PriceDelta { get; set; }
    public bool IsDefault { get; set; }

    public Product? Product { get; set; }
    public InventoryItem? Inventory { get; set; }
}
