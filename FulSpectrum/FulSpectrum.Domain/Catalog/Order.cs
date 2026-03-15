namespace FulSpectrum.Domain.Catalog;

public enum OrderStatus
{
    Draft = 0,
    PendingPayment = 1,
    Paid = 2,
    Processing = 3,
    Shipped = 4,
    Completed = 5,
    Cancelled = 6
}

public sealed class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Draft;
    public string Currency { get; set; } = "USD";
    public decimal Subtotal { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public string ShippingFullName { get; set; } = string.Empty;
    public string ShippingAddressLine1 { get; set; } = string.Empty;
    public string? ShippingAddressLine2 { get; set; }
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingState { get; set; } = string.Empty;
    public string ShippingPostalCode { get; set; } = string.Empty;
    public string ShippingCountryCode { get; set; } = string.Empty;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public bool TryTransitionTo(OrderStatus nextStatus, out string? error)
    {
        error = null;

        if (nextStatus == Status)
        {
            return true;
        }

        var allowed = Status switch
        {
            OrderStatus.Draft => nextStatus is OrderStatus.PendingPayment or OrderStatus.Cancelled,
            OrderStatus.PendingPayment => nextStatus is OrderStatus.Paid or OrderStatus.Cancelled,
            OrderStatus.Paid => nextStatus is OrderStatus.Processing or OrderStatus.Cancelled,
            OrderStatus.Processing => nextStatus is OrderStatus.Shipped,
            OrderStatus.Shipped => nextStatus is OrderStatus.Completed,
            OrderStatus.Completed => false,
            OrderStatus.Cancelled => false,
            _ => false
        };

        if (!allowed)
        {
            error = $"Transición inválida: {Status} -> {nextStatus}.";
            return false;
        }

        Status = nextStatus;
        UpdatedAtUtc = DateTime.UtcNow;
        return true;
    }
}
