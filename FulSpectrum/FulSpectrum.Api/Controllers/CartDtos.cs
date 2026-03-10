namespace FulSpectrum.Api.Controllers;

public sealed record CartDto(Guid Id, Guid UserId, string RowVersion, IReadOnlyCollection<CartItemDto> Items, int TotalItems, decimal Subtotal, string Currency);
public sealed record CartItemDto(Guid Id, Guid ProductId, string ProductName, string Sku, decimal UnitPrice, int Quantity, int MaxAllowedQuantity, int AvailableStock, decimal LineTotal);

public sealed record AddCartItemRequest(Guid ProductId, int Quantity, string? RowVersion);
public sealed record UpdateCartItemRequest(int Quantity, string? RowVersion);
public sealed record MergeCartRequest(IReadOnlyCollection<MergeCartItemRequest> Items, string? RowVersion);
public sealed record MergeCartItemRequest(Guid ProductId, int Quantity);
