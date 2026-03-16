using FulSpectrum.Domain.Catalog;

namespace FulSpectrum.Api.Controllers;

internal static class OrderMapping
{
    public static OrderDto MapOrderDto(Order order)
    {
        var address = new ShippingAddressRequest(
            order.ShippingFullName,
            order.ShippingAddressLine1,
            order.ShippingAddressLine2,
            order.ShippingCity,
            order.ShippingState,
            order.ShippingPostalCode,
            order.ShippingCountryCode);

        return new OrderDto(
            order.Id,
            order.UserId,
            order.Status.ToString(),
            order.Currency,
            order.Subtotal,
            order.ShippingAmount,
            order.TaxAmount,
            order.Total,
            order.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductNameSnapshot, i.SkuSnapshot, i.UnitPriceSnapshot, i.Quantity, i.LineTotal)).ToList(),
            address,
            order.CreatedAtUtc,
            order.UpdatedAtUtc);
    }
}
