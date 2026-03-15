namespace FulSpectrum.Api.Controllers;

public sealed record CreatePaymentAttemptRequest(string Provider, string? ReturnUrl);

public sealed record PaymentAttemptDto(Guid PaymentId, Guid OrderId, string Provider, string Status, string CheckoutUrl, string ExternalReference, string? ProviderPaymentId);

public sealed record PaymentStatusDto(Guid OrderId, Guid? PaymentId, string Status, string Provider, DateTime UpdatedAtUtc, string? FailureMessage);

public sealed record PaymentWebhookRequest(string EventId, string EventType, string PaymentId, string Status, decimal? Amount, string? Currency, string? ErrorCode, string? ErrorMessage);
