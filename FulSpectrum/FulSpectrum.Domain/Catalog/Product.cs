namespace FulSpectrum.Domain.Catalog;

public sealed class Product
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsPublished { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public Category? Category { get; set; }
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
}
