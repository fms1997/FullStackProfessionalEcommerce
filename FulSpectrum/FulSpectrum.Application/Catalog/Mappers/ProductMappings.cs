using FulSpectrum.Application.Catalog.Dtos;
using FulSpectrum.Domain.Catalog;

namespace FulSpectrum.Application.Catalog.Mappers;

public static class ProductMappings
{
    public static ProductDto ToDto(this Product product) =>
        new(
            product.Id,
            product.CategoryId,
            product.Name,
            product.Slug,
            product.Sku,
            product.BasePrice,
            product.Currency,
            product.IsPublished,
            product.CreatedAtUtc);
}
