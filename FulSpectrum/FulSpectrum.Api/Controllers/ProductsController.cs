using System.Text.Json;
using Asp.Versioning;
using FluentValidation;
using FulSpectrum.Api.Services;
using FulSpectrum.Application.Catalog.Dtos;
using FulSpectrum.Application.Catalog.Mappers;
using FulSpectrum.Application.Catalog.Queries;
using FulSpectrum.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FulSpectrum.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/products")]
[Authorize]
public sealed class ProductsController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly FulSpectrumDbContext _db;
    private readonly IValidator<CreateProductRequest> _createValidator;
    private readonly IValidator<UpdateProductRequest> _updateValidator;
    private readonly IValidator<ProductListQuery> _queryValidator;
    private readonly ICatalogCacheService _catalogCache;

    public ProductsController(
        FulSpectrumDbContext db,
        IValidator<CreateProductRequest> createValidator,
        IValidator<UpdateProductRequest> updateValidator,
        IValidator<ProductListQuery> queryValidator,
        ICatalogCacheService catalogCache)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _queryValidator = queryValidator;
        _catalogCache = catalogCache;
    }

    [HttpGet]
    [AllowAnonymous]
    [ResponseCache(
        Duration = 60,
        Location = ResponseCacheLocation.Any,
        VaryByQueryKeys = new[]
        {
            "search",
            "isPublished",
            "sortBy",
            "sortDirection",
            "page",
            "pageSize",
            "categoryId",
            "minPrice",
            "maxPrice"
        })]
    public async Task<ActionResult<PagedResponse<ProductDto>>> GetAll(
        [FromQuery] ProductListQuery query,
        CancellationToken ct)
    {
        var validation = await _queryValidator.ValidateAsync(query, ct);
        if (!validation.IsValid)
        {
            return ValidationProblem(
                new ValidationProblemDetails(validation.ToDictionary()));
        }

        var version = await _catalogCache.GetCatalogVersionAsync(ct);
        var cacheKey = BuildListCacheKey(query, version);

        var cached = await _catalogCache.GetAsync<PagedResponse<ProductDto>>(cacheKey, ct);
        if (cached is null)
        {
            cached = await BuildProductListAsync(query, ct);
            await _catalogCache.SetAsync(cacheKey, cached, TimeSpan.FromMinutes(3), ct);
        }

        return WithEtag(cached);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any)]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken ct)
    {
        var version = await _catalogCache.GetCatalogVersionAsync(ct);
        var cacheKey = $"catalog:product:{version}:{id}";

        var product = await _catalogCache.GetAsync<ProductDto>(cacheKey, ct);
        if (product is null)
        {
            product = await _db.Products
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => p.ToDto())
                .FirstOrDefaultAsync(ct);

            if (product is null)
            {
                return NotFound();
            }

            await _catalogCache.SetAsync(cacheKey, product, TimeSpan.FromMinutes(10), ct);
        }

        return WithEtag(product);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageCatalog")]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken ct)
    {
        var validation = await _createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return ValidationProblem(
                new ValidationProblemDetails(validation.ToDictionary()));
        }

        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId, ct);
        if (!categoryExists)
        {
            return ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    [nameof(request.CategoryId)] = ["Category does not exist."]
                }));
        }

        var product = new Domain.Catalog.Product
        {
            Id = Guid.NewGuid(),
            CategoryId = request.CategoryId,
            Name = request.Name.Trim(),
            Slug = request.Slug.Trim(),
            Sku = request.Sku.Trim(),
            BasePrice = request.BasePrice,
            Currency = request.Currency.ToUpperInvariant(),
            IsPublished = request.IsPublished,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);
        await _catalogCache.InvalidateCatalogAsync(ct);

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id, version = "1" },
            product.ToDto());
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CanManageCatalog")]
    public async Task<ActionResult<ProductDto>> Update(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken ct)
    {
        var validation = await _updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return ValidationProblem(
                new ValidationProblemDetails(validation.ToDictionary()));
        }

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product is null)
        {
            return NotFound();
        }

        product.CategoryId = request.CategoryId;
        product.Name = request.Name.Trim();
        product.Slug = request.Slug.Trim();
        product.Sku = request.Sku.Trim();
        product.BasePrice = request.BasePrice;
        product.Currency = request.Currency.ToUpperInvariant();
        product.IsPublished = request.IsPublished;

        await _db.SaveChangesAsync(ct);
        await _catalogCache.InvalidateCatalogAsync(ct);

        return Ok(product.ToDto());
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CanManageCatalog")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product is null)
        {
            return NotFound();
        }

        _db.Products.Remove(product);
        await _db.SaveChangesAsync(ct);
        await _catalogCache.InvalidateCatalogAsync(ct);

        return NoContent();
    }

    private async Task<PagedResponse<ProductDto>> BuildProductListAsync(
        ProductListQuery query,
        CancellationToken ct)
    {
        var products = _db.Products
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            products = products.Where(p =>
                p.Name.Contains(search) ||
                p.Sku.Contains(search) ||
                p.Slug.Contains(search));
        }

        if (query.CategoryId.HasValue)
        {
            products = products.Where(p => p.CategoryId == query.CategoryId.Value);
        }

        if (query.IsPublished.HasValue)
        {
            products = products.Where(p => p.IsPublished == query.IsPublished.Value);
        }

        if (query.MinPrice.HasValue)
        {
            products = products.Where(p => p.BasePrice >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            products = products.Where(p => p.BasePrice <= query.MaxPrice.Value);
        }

        var descending = string.Equals(
            query.SortDirection,
            "desc",
            StringComparison.OrdinalIgnoreCase);

        products = ApplySorting(products, query.SortBy, descending);

        var totalCount = await products.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);
        var skip = (query.Page - 1) * query.PageSize;

        var items = await products
            .Skip(skip)
            .Take(query.PageSize)
            .Select(p => p.ToDto())
            .ToListAsync(ct);

        return new PagedResponse<ProductDto>(
            items,
            query.Page,
            query.PageSize,
            totalCount,
            totalPages);
    }

    private static IQueryable<Domain.Catalog.Product> ApplySorting(
        IQueryable<Domain.Catalog.Product> products,
        string sortBy,
        bool descending)
    {
        return sortBy.ToLowerInvariant() switch
        {
            "name" => descending
                ? products.OrderByDescending(p => p.Name)
                : products.OrderBy(p => p.Name),

            "price" => descending
                ? products.OrderByDescending(p => p.BasePrice)
                : products.OrderBy(p => p.BasePrice),

            "sku" => descending
                ? products.OrderByDescending(p => p.Sku)
                : products.OrderBy(p => p.Sku),

            _ => descending
                ? products.OrderByDescending(p => p.CreatedAtUtc)
                : products.OrderBy(p => p.CreatedAtUtc)
        };
    }

    private ActionResult<T> WithEtag<T>(T data)
    {
        var payload = JsonSerializer.Serialize(data, JsonOptions);
        var etag = _catalogCache.BuildEtag(payload);

        if (Request.Headers.IfNoneMatch == etag)
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }

        Response.Headers.ETag = etag;
        return Ok(data);
    }

    private static string BuildListCacheKey(ProductListQuery query, long version)
    {
        return string.Join(
            ':',
            "catalog:list",
            version,
            query.Search?.Trim() ?? string.Empty,
            query.IsPublished?.ToString() ?? string.Empty,
            query.SortBy,
            query.SortDirection,
            query.Page,
            query.PageSize,
            query.CategoryId?.ToString() ?? string.Empty,
            query.MinPrice?.ToString() ?? string.Empty,
            query.MaxPrice?.ToString() ?? string.Empty);
    }
}