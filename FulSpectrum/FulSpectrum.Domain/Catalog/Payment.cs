namespace FulSpectrum.Domain.Catalog;

public enum PaymentProvider
{
    Stripe = 1,
    MercadoPago = 2
}

public enum PaymentStatus
{
    Created = 0,
    Pending = 1,
    Authorized = 2,
    Succeeded = 3,
    Failed = 4,
    Canceled = 5,
    Refunded = 6
}

public sealed class Payment
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public PaymentProvider Provider { get; set; }
    public string? ProviderPaymentId { get; set; }
    public string ExternalReference { get; set; } = string.Empty;

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentStatus Status { get; set; } = PaymentStatus.Created;

    public string? FailureCode { get; set; }
    public string? FailureMessage { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public bool TryTransitionTo(PaymentStatus nextStatus, out string? error)
    {
        error = null;

        if (nextStatus == Status)
        {
            return true;
        }

        var allowed = Status switch
        {
            PaymentStatus.Created => nextStatus is PaymentStatus.Pending or PaymentStatus.Canceled,
            PaymentStatus.Pending => nextStatus is PaymentStatus.Authorized or PaymentStatus.Succeeded or PaymentStatus.Failed or PaymentStatus.Canceled,
            PaymentStatus.Authorized => nextStatus is PaymentStatus.Succeeded or PaymentStatus.Failed or PaymentStatus.Canceled,
            PaymentStatus.Succeeded => nextStatus is PaymentStatus.Refunded,
            PaymentStatus.Failed => false,
            PaymentStatus.Canceled => false,
            PaymentStatus.Refunded => false,
            _ => false
        };

        if (!allowed)
        {
            error = $"Transición inválida de pago: {Status} -> {nextStatus}.";
            return false;
        }

        Status = nextStatus;
        UpdatedAtUtc = DateTime.UtcNow;
        return true;
    }
}
