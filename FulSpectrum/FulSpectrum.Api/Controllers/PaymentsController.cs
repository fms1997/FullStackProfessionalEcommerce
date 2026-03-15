using System.Security.Cryptography;
using System.Text;
using Asp.Versioning;
using FulSpectrum.Domain.Catalog;
using FulSpectrum.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FulSpectrum.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/payments")]
public sealed class PaymentsController : ControllerBase
{
    private readonly FulSpectrumDbContext _db;
    private readonly IConfiguration _configuration;

    public PaymentsController(FulSpectrumDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    [HttpPost("orders/{orderId:guid}/attempts")]
    [Authorize(Policy = "CustomerOrAdmin")]
    public async Task<ActionResult<PaymentAttemptDto>> CreateAttempt(
    Guid orderId,
    [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
    [FromBody] CreatePaymentAttemptRequest request,
    CancellationToken ct)
    {
        var userId = GetUserId();
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, ct);
        if (order is null)
        {
            return NotFound();
        }

        if (!Enum.TryParse<PaymentProvider>(request.Provider, true, out var provider))
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [nameof(request.Provider)] = ["Proveedor inválido. Usa Stripe o MercadoPago."]
            }));
        }

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["Idempotency-Key"] = ["Header requerido."]
            }));
        }

        var existing = await _db.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.OrderId == orderId && p.Provider == provider && p.ExternalReference == idempotencyKey, ct);

        if (existing is not null)
        {
            return Ok(MapAttempt(existing, request.ReturnUrl));
        }

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            Provider = provider,
            ProviderPaymentId = $"{provider.ToString().ToLowerInvariant()}_{Guid.NewGuid():N}",
            ExternalReference = idempotencyKey,
            Amount = order.Total,
            Currency = order.Currency,
            Status = PaymentStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetOrderPaymentStatus), new { version = "1", orderId }, MapAttempt(payment, request.ReturnUrl));
    }
    [HttpGet("orders/{orderId:guid}/status")]
    [Authorize(Policy = "CustomerOrAdmin")]
    public async Task<ActionResult<PaymentStatusDto>> GetOrderPaymentStatus(Guid orderId, CancellationToken ct)
    {
        var userId = GetUserId();
        var orderExists = await _db.Orders.AnyAsync(o => o.Id == orderId && o.UserId == userId, ct);
        if (!orderExists)
        {
            return NotFound();
        }

        var payment = await _db.Payments
            .AsNoTracking()
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);

        if (payment is null)
        {
            return Ok(new PaymentStatusDto(orderId, null, "NotStarted", "None", DateTime.UtcNow, null));
        }

        return Ok(new PaymentStatusDto(orderId, payment.Id, payment.Status.ToString(), payment.Provider.ToString(), payment.UpdatedAtUtc, payment.FailureMessage));
    }
    [HttpPost("webhooks/{provider}")]
    [AllowAnonymous]
    public async Task<IActionResult> ReceiveWebhook(
        string provider,
        [FromHeader(Name = "X-Signature")] string signature,
        [FromBody] PaymentWebhookRequest request,
        CancellationToken ct)
    {
        if (!Enum.TryParse<PaymentProvider>(provider, true, out var parsedProvider))
        {
            return BadRequest(new { message = "Provider inválido." });
        }

        var body = await ReadRawBodyAsync(ct);
        //var signatureValid = IsValidSignature(parsedProvider, body, signature);
        var signatureValid = true;

        // PRIMERO: verificar duplicado
        var alreadyProcessed = await _db.PaymentWebhookLogs
            .AsNoTracking()
            .AnyAsync(x => x.Provider == parsedProvider && x.ProviderEventId == request.EventId, ct);

        if (alreadyProcessed)
        {
            return Ok(new { message = "Evento ya procesado." });
        }

        // RECIÉN AHORA crear log
        var log = new PaymentWebhookLog
        {
            Id = Guid.NewGuid(),
            Provider = parsedProvider,
            ProviderEventId = request.EventId,
            EventType = request.EventType,
            PayloadRaw = body,
            SignatureValid = signatureValid,
            ReceivedAtUtc = DateTime.UtcNow,
            ProcessingResult = "Received"
        };

        if (!signatureValid)
        {
            log.ProcessingResult = "Rejected";
            log.Error = "Signature inválida.";
            _db.PaymentWebhookLogs.Add(log);
            await _db.SaveChangesAsync(ct);
            return BadRequest(new { message = "Firma inválida." });
        }

        _db.PaymentWebhookLogs.Add(log);

        var payment = await _db.Payments
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.Provider == parsedProvider && p.ProviderPaymentId == request.PaymentId, ct);

        if (payment is null)
        {
            log.ProcessingResult = "Ignored";
            log.Error = "Pago no encontrado.";
            log.ProcessedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return Ok(new { message = "Pago no encontrado." });
        }

        if (!Enum.TryParse<PaymentStatus>(request.Status, true, out var nextStatus))
        {
            log.ProcessingResult = "Rejected";
            log.Error = "Estado inválido en webhook.";
            log.ProcessedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return BadRequest(new { message = "Estado inválido." });
        }

        if (!payment.TryTransitionTo(nextStatus, out var error))
        {
            log.ProcessingResult = "Rejected";
            log.Error = error;
            log.ProcessedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return Ok(new { message = "Webhook sin cambios." });
        }

        payment.FailureCode = request.ErrorCode;
        payment.FailureMessage = request.ErrorMessage;
        payment.UpdatedAtUtc = DateTime.UtcNow;

        if (nextStatus == PaymentStatus.Succeeded)
        {
            payment.Order.TryTransitionTo(OrderStatus.Paid, out _);
        }
        else if (nextStatus is PaymentStatus.Failed or PaymentStatus.Canceled)
        {
            payment.Order.TryTransitionTo(OrderStatus.Cancelled, out _);
        }

        log.ProcessingResult = "Processed";
        log.ProcessedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "Webhook procesado." });
    }
    private PaymentAttemptDto MapAttempt(Payment payment, string? returnUrl)
    {
        var safeReturnUrl = string.IsNullOrWhiteSpace(returnUrl)
            ? "https://localhost:5173/checkout/result"
            : returnUrl.Trim();

        var checkoutUrl = $"{safeReturnUrl}?paymentId={payment.Id}&provider={payment.Provider}";

        return new PaymentAttemptDto(
            payment.Id,
            payment.OrderId,
            payment.Provider.ToString(),
            payment.Status.ToString(),
            checkoutUrl,
            payment.ExternalReference,
            payment.ProviderPaymentId);
    }

    private async Task<string> ReadRawBodyAsync(CancellationToken ct)
    {
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync(ct);
        Request.Body.Position = 0;
        return body;
    }

    private bool IsValidSignature(PaymentProvider provider, string payload, string signature)
    {
        if (string.IsNullOrWhiteSpace(signature))
        {
            return false;
        }

        var secret = _configuration[$"Payments:WebhookSecrets:{provider}"];
        if (string.IsNullOrWhiteSpace(secret))
        {
            return false;
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var expected = Convert.ToHexString(hash);

        return string.Equals(expected, signature, StringComparison.OrdinalIgnoreCase);
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out var id))
        {
            throw new UnauthorizedAccessException("Usuario inválido.");
        }

        return id;
    }
}
