namespace FulSpectrum.Domain.Catalog;

public sealed class CartItem
{
    public Guid Id { get; set; }
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public Cart? Cart { get; set; }
    public Product? Product { get; set; }
}
