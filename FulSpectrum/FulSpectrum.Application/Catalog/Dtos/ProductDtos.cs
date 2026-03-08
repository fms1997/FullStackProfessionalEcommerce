namespace FulSpectrum.Application.Catalog.Dtos;

public sealed record ProductDto(
    Guid Id,
    Guid CategoryId,
    string Name,
    string Slug,
    string Sku,
    decimal BasePrice,
    string Currency,
    bool IsPublished,
    DateTime CreatedAtUtc);

public sealed record CreateProductRequest(
    Guid CategoryId,
    string Name,
    string Slug,
    string Sku,
    decimal BasePrice,
    string Currency,
    bool IsPublished);

public sealed record UpdateProductRequest(
    Guid CategoryId,
    string Name,
    string Slug,
    string Sku,
    decimal BasePrice,
    string Currency,
    bool IsPublished);

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
