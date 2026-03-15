namespace FulSpectrum.Domain.Catalog;

public sealed class PaymentWebhookLog
{
    public Guid Id { get; set; }
    public PaymentProvider Provider { get; set; }
    public string ProviderEventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string PayloadRaw { get; set; } = string.Empty;
    public bool SignatureValid { get; set; }
    public DateTime ReceivedAtUtc { get; set; }
    public DateTime? ProcessedAtUtc { get; set; }
    public string ProcessingResult { get; set; } = "Received";
    public string? Error { get; set; }
}
