namespace FulSpectrum.Domain.Catalog;

public sealed class Cart
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
