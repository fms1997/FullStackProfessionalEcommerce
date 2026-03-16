using Asp.Versioning;
using FulSpectrum.Domain.Catalog;
using FulSpectrum.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FulSpectrum.Api.Jobs;
using Hangfire;
namespace FulSpectrum.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/checkout")]
[Authorize(Policy = "CustomerOrAdmin")]
public sealed class CheckoutController : ControllerBase
{
    private readonly FulSpectrumDbContext _db;
    private readonly IBackgroundJobClient _backgroundJobs;
    public CheckoutController(FulSpectrumDbContext db, IBackgroundJobClient backgroundJobs)
    {
        _db = db;
        _backgroundJobs = backgroundJobs;
    }

    [HttpPost("preview")]
    public async Task<ActionResult<CheckoutPreviewDto>> Preview([FromBody] CheckoutPreviewRequest request, CancellationToken ct)
    {
        if (!TryValidateAddress(request.ShippingAddress, out var validation))
        {
            return ValidationProblem(validation);
        }

        var cart = await GetUserCartAsync(ct);
        if (cart.Items.Count == 0)
        {
            return Conflict(new { message = "El carrito está vacío." });
        }

        var summary = await BuildCheckoutSummaryAsync(cart, request.ShippingAddress, ct);
        return Ok(summary);
    }

    [HttpPost("orders")]
    public async Task<ActionResult<OrderDto>> PlaceOrder([FromBody] PlaceOrderRequest request, CancellationToken ct)
    {
        if (!TryValidateAddress(request.ShippingAddress, out var validation))
        {
            return ValidationProblem(validation);
        }

        var cart = await GetUserCartAsync(ct);
        if (cart.Items.Count == 0)
        {
            return Conflict(new { message = "El carrito está vacío." });
        }

        var summary = await BuildCheckoutSummaryAsync(cart, request.ShippingAddress, ct);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = cart.UserId,
            Status = OrderStatus.PendingPayment,
            Currency = summary.Totals.Currency,
            Subtotal = summary.Totals.Subtotal,
            ShippingAmount = summary.Totals.ShippingAmount,
            TaxAmount = summary.Totals.TaxAmount,
            Total = summary.Totals.Total,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            ShippingFullName = request.ShippingAddress.FullName.Trim(),
            ShippingAddressLine1 = request.ShippingAddress.AddressLine1.Trim(),
            ShippingAddressLine2 = request.ShippingAddress.AddressLine2?.Trim(),
            ShippingCity = request.ShippingAddress.City.Trim(),
            ShippingState = request.ShippingAddress.State.Trim(),
            ShippingPostalCode = request.ShippingAddress.PostalCode.Trim(),
            ShippingCountryCode = request.ShippingAddress.CountryCode.Trim().ToUpperInvariant(),
            Items = summary.Items.Select(i => new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = i.ProductId,
                ProductNameSnapshot = i.ProductName,
                SkuSnapshot = i.Sku,
                UnitPriceSnapshot = i.UnitPrice,
                Quantity = i.Quantity,
                LineTotal = i.LineTotal
            }).ToList()
        };

        _db.Orders.Add(order);
        _db.CartItems.RemoveRange(cart.Items);
        cart.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        _backgroundJobs.Enqueue<OrderNotificationJobs>(x => x.SendOrderConfirmation(order.Id));

        return CreatedAtAction(nameof(GetOrderById), new { version = "1", id = order.Id }, OrderMapping.MapOrderDto(order));
    }

    [HttpGet("orders/{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetOrderById(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var order = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId, ct);

        if (order is null)
        {
            return NotFound();
        }

        return Ok(OrderMapping.MapOrderDto(order));
    }

    private async Task<Cart> GetUserCartAsync(CancellationToken ct)
    {
        var userId = GetUserId();
        var cart = await _db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);

        if (cart is not null)
        {
            return cart;
        }

        cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _db.Carts.Add(cart);
        await _db.SaveChangesAsync(ct);
        return cart;
    }

    private async Task<CheckoutPreviewDto> BuildCheckoutSummaryAsync(Cart cart, ShippingAddressRequest shippingAddress, CancellationToken ct)
    {
        var productIds = cart.Items.Select(i => i.ProductId).Distinct().ToArray();

        var products = await _db.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id) && p.IsPublished)
            .Select(p => new { p.Id, p.Name, p.Sku, p.BasePrice, p.Currency })
            .ToDictionaryAsync(x => x.Id, ct);

        var missingProducts = cart.Items.Where(i => !products.ContainsKey(i.ProductId)).Select(i => i.ProductId).ToArray();
        if (missingProducts.Length > 0)
        {
            throw new InvalidOperationException("Algunos productos del carrito ya no están disponibles.");
        }

        var items = cart.Items.Select(item =>
        {
            var product = products[item.ProductId];
            var lineTotal = product.BasePrice * item.Quantity;
            return new CheckoutItemSnapshotDto(item.ProductId, product.Name, product.Sku, product.BasePrice, item.Quantity, lineTotal);
        }).ToList();

        var subtotal = items.Sum(i => i.LineTotal);
        var shipping = CalculateShipping(subtotal, shippingAddress.CountryCode);
        var tax = CalculateTax(subtotal, shippingAddress.CountryCode);
        var total = subtotal + shipping + tax;
        var currency = products.Values.Select(x => x.Currency).FirstOrDefault() ?? "USD";

        return new CheckoutPreviewDto(
            cart.Id,
            items,
            new CheckoutTotalsDto(subtotal, shipping, tax, total, currency),
            shippingAddress);
    }

    private static decimal CalculateShipping(decimal subtotal, string countryCode)
    {
        if (subtotal >= 100m)
        {
            return 0m;
        }

        return string.Equals(countryCode, "US", StringComparison.OrdinalIgnoreCase) ? 8m : 18m;
    }

    private static decimal CalculateTax(decimal subtotal, string countryCode)
    {
        return string.Equals(countryCode, "US", StringComparison.OrdinalIgnoreCase)
            ? Math.Round(subtotal * 0.08m, 2)
            : 0m;
    }

    private static bool TryValidateAddress(ShippingAddressRequest address, out ValidationProblemDetails validation)
    {
        var errors = new Dictionary<string, string[]>();

        AddIfEmpty(errors, nameof(address.FullName), address.FullName);
        AddIfEmpty(errors, nameof(address.AddressLine1), address.AddressLine1);
        AddIfEmpty(errors, nameof(address.City), address.City);
        AddIfEmpty(errors, nameof(address.State), address.State);
        AddIfEmpty(errors, nameof(address.PostalCode), address.PostalCode);
        AddIfEmpty(errors, nameof(address.CountryCode), address.CountryCode);

        if (!string.IsNullOrWhiteSpace(address.CountryCode) && address.CountryCode.Trim().Length != 2)
        {
            errors[nameof(address.CountryCode)] = ["CountryCode debe tener 2 caracteres ISO."];
        }

        validation = new ValidationProblemDetails(errors);
        return errors.Count == 0;
    }

    private static void AddIfEmpty(Dictionary<string, string[]> errors, string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors[key] = [$"{key} es requerido."];
        }
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
