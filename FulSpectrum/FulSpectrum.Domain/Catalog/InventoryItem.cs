namespace FulSpectrum.Domain.Catalog;

public sealed class InventoryItem
{
    public Guid Id { get; set; }
    public Guid VariantId { get; set; }
    public int QuantityOnHand { get; set; }
    public int ReservedQuantity { get; set; }
    public int ReorderThreshold { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public ProductVariant? Variant { get; set; }
}
