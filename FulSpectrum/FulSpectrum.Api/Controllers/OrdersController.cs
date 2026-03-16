using Asp.Versioning;
using FulSpectrum.Api.Jobs;
using FulSpectrum.Domain.Catalog;
using FulSpectrum.Domain.Identity;
using FulSpectrum.Infrastructure.Persistence;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FulSpectrum.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/orders")]
[Authorize(Policy = "CustomerOrAdmin")]
public sealed class OrdersController : ControllerBase
{
    private readonly FulSpectrumDbContext _db;
    private readonly IBackgroundJobClient _backgroundJobs;

    public OrdersController(FulSpectrumDbContext db, IBackgroundJobClient backgroundJobs)
    {
        _db = db;
        _backgroundJobs = backgroundJobs;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<OrderSummaryDto>>> GetMyOrders(CancellationToken ct)
    {
        var userId = GetUserId();
        var isAdmin = User.IsInRole(UserRoles.Admin);

        var query = _db.Orders.AsNoTracking();
        if (!isAdmin)
        {
            query = query.Where(x => x.UserId == userId);
        }

        var orders = await query
            .OrderByDescending(o => o.CreatedAtUtc)
            .Select(o => new OrderSummaryDto(
                o.Id,
                o.Status.ToString(),
                o.Currency,
                o.Total,
                o.Items.Sum(i => i.Quantity),
                o.CreatedAtUtc,
                o.UpdatedAtUtc))
            .ToListAsync(ct);

        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var isAdmin = User.IsInRole(UserRoles.Admin);

        var order = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id && (isAdmin || o.UserId == userId), ct);

        if (order is null)
        {
            return NotFound();
        }

        return Ok(OrderMapping.MapOrderDto(order));
    }

    [HttpGet("{id:guid}/tracking")]
    public async Task<ActionResult<OrderTrackingDto>> GetTracking(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var isAdmin = User.IsInRole(UserRoles.Admin);

        var order = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == id && (isAdmin || o.UserId == userId), ct);

        if (order is null)
        {
            return NotFound();
        }

        var steps = BuildTrackingSteps(order).ToList();
        return Ok(new OrderTrackingDto(order.Id, order.Status.ToString(), order.UpdatedAtUtc, steps));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        var isAdmin = User.IsInRole(UserRoles.Admin);

        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id && (isAdmin || o.UserId == userId), ct);

        if (order is null)
        {
            return NotFound();
        }

        if (!Enum.TryParse<OrderStatus>(request.Status, true, out var nextStatus))
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [nameof(request.Status)] = ["Estado inválido."]
            }));
        }

        if (!isAdmin && nextStatus != OrderStatus.Cancelled)
        {
            return Forbid();
        }

        if (!order.TryTransitionTo(nextStatus, out var error))
        {
            return Conflict(new { message = error });
        }

        await _db.SaveChangesAsync(ct);

        if (nextStatus == OrderStatus.Shipped)
        {
            _backgroundJobs.Enqueue<OrderNotificationJobs>(x => x.SendShipmentNotification(order.Id));
        }

        return Ok(OrderMapping.MapOrderDto(order));
    }

    private static IEnumerable<OrderTrackingStepDto> BuildTrackingSteps(Order order)
    {
        var definitions = new[]
        {
            (OrderStatus.PendingPayment, "Orden creada"),
            (OrderStatus.Paid, "Pago confirmado"),
            (OrderStatus.Processing, "Preparando envío"),
            (OrderStatus.Shipped, "Enviado"),
            (OrderStatus.Completed, "Entregado")
        };

        foreach (var (status, label) in definitions)
        {
            var reached = order.Status >= status && order.Status != OrderStatus.Cancelled;
            yield return new OrderTrackingStepDto(label, status.ToString(), reached, reached ? order.UpdatedAtUtc : null);
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            yield return new OrderTrackingStepDto("Orden cancelada", OrderStatus.Cancelled.ToString(), true, order.UpdatedAtUtc);
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
