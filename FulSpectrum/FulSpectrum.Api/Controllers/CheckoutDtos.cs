namespace FulSpectrum.Api.Controllers;

public sealed record ShippingAddressRequest(
    string FullName,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string PostalCode,
    string CountryCode);

public sealed record CheckoutPreviewRequest(ShippingAddressRequest ShippingAddress);

public sealed record CheckoutItemSnapshotDto(Guid ProductId, string ProductName, string Sku, decimal UnitPrice, int Quantity, decimal LineTotal);

public sealed record CheckoutTotalsDto(decimal Subtotal, decimal ShippingAmount, decimal TaxAmount, decimal Total, string Currency);

public sealed record CheckoutPreviewDto(Guid CartId, IReadOnlyCollection<CheckoutItemSnapshotDto> Items, CheckoutTotalsDto Totals, ShippingAddressRequest ShippingAddress);

public sealed record PlaceOrderRequest(ShippingAddressRequest ShippingAddress);

public sealed record OrderItemDto(Guid ProductId, string ProductName, string Sku, decimal UnitPrice, int Quantity, decimal LineTotal);

public sealed record OrderDto(Guid Id, Guid UserId, string Status, string Currency, decimal Subtotal, decimal ShippingAmount, decimal TaxAmount, decimal Total, IReadOnlyCollection<OrderItemDto> Items, ShippingAddressRequest ShippingAddress, DateTime CreatedAtUtc);

public sealed record UpdateOrderStatusRequest(string Status);
