using FulSpectrum.Domain.Catalog;
using FulSpectrum.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FulSpectrum.Api.Jobs;

public sealed class OrderWorkflowJobs
{
    private readonly FulSpectrumDbContext _db;

    public OrderWorkflowJobs(FulSpectrumDbContext db)
    {
        _db = db;
    }

    public async Task MoveToProcessing(Guid orderId)
    {
        await Transition(orderId, OrderStatus.Processing);
    }

    public async Task MoveToShipped(Guid orderId)
    {
        await Transition(orderId, OrderStatus.Shipped);
    }

    private async Task Transition(Guid orderId, OrderStatus target)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null)
        {
            return;
        }

        if (!order.TryTransitionTo(target, out _))
        {
            return;
        }

        await _db.SaveChangesAsync();
    }
}
