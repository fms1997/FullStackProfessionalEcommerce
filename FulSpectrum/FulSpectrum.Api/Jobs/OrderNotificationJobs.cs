using FulSpectrum.Api.Services;
using FulSpectrum.Domain.Catalog;
using FulSpectrum.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FulSpectrum.Api.Jobs;

public sealed class OrderNotificationJobs
{
    private readonly FulSpectrumDbContext _db;
    private readonly IEmailNotificationService _email;

    public OrderNotificationJobs(FulSpectrumDbContext db, IEmailNotificationService email)
    {
        _db = db;
        _email = email;
    }

    public async Task SendOrderConfirmation(Guid orderId)
    {
        var order = await GetOrder(orderId);
        if (order is null) return;

        await _email.SendAsync(
            order.Email,
            $"Orden {order.Id} confirmada",
            $"Tu orden fue creada por {order.Total:0.00} {order.Currency} y está en estado {order.Status}.");
    }

    public async Task SendPaymentConfirmation(Guid orderId)
    {
        var order = await GetOrder(orderId);
        if (order is null) return;

        await _email.SendAsync(
            order.Email,
            $"Pago confirmado para orden {order.Id}",
            "Tu pago fue acreditado. Estamos preparando tu envío.");
    }

    public async Task SendShipmentNotification(Guid orderId)
    {
        var order = await GetOrder(orderId);
        if (order is null) return;

        await _email.SendAsync(
            order.Email,
            $"Orden {order.Id} enviada",
            "Tu pedido fue despachado. Puedes revisar el tracking desde Mis pedidos.");
    }

    private async Task<OrderWithUser?> GetOrder(Guid orderId)
    {
        return await _db.Orders
            .AsNoTracking()
            .Where(order => order.Id == orderId)
            .Join(
                _db.Users.AsNoTracking(),
                order => order.UserId,
                user => user.Id,
                (order, user) => new OrderWithUser(
                    order.Id,
                    order.Currency,
                    order.Total,
                    order.Status,
                    user.Email))
            .FirstOrDefaultAsync();
    }

    private sealed record OrderWithUser(Guid Id, string Currency, decimal Total, OrderStatus Status, string Email);
}
