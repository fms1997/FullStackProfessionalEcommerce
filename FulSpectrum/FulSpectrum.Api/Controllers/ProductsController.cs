using Asp.Versioning;
using FluentValidation;
using FulSpectrum.Application.Catalog.Dtos;
using FulSpectrum.Application.Catalog.Mappers;
using FulSpectrum.Application.Catalog.Queries;
using FulSpectrum.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FulSpectrum.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly FulSpectrumDbContext _db;
    private readonly IValidator<CreateProductRequest> _createValidator;
    private readonly IValidator<UpdateProductRequest> _updateValidator;
    private readonly IValidator<ProductListQuery> _queryValidator;

    public ProductsController(
        FulSpectrumDbContext db,
        IValidator<CreateProductRequest> createValidator,
        IValidator<UpdateProductRequest> updateValidator,
        IValidator<ProductListQuery> queryValidator)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _queryValidator = queryValidator;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<ProductDto>>> GetAll([FromQuery] ProductListQuery query, CancellationToken ct)
    {
        var validation = await _queryValidator.ValidateAsync(query, ct);
        if (!validation.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));
        }

        var products = _db.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            products = products.Where(p => p.Name.Contains(search) || p.Sku.Contains(search) || p.Slug.Contains(search));
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

        var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        products = query.SortBy.ToLowerInvariant() switch
        {
            "name" => descending ? products.OrderByDescending(p => p.Name) : products.OrderBy(p => p.Name),
            "price" => descending ? products.OrderByDescending(p => p.BasePrice) : products.OrderBy(p => p.BasePrice),
            "sku" => descending ? products.OrderByDescending(p => p.Sku) : products.OrderBy(p => p.Sku),
            _ => descending ? products.OrderByDescending(p => p.CreatedAtUtc) : products.OrderBy(p => p.CreatedAtUtc)
        };

        var totalCount = await products.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);
        var skip = (query.Page - 1) * query.PageSize;

        var items = await products
            .Skip(skip)
            .Take(query.PageSize)
            .Select(p => p.ToDto())
            .ToListAsync(ct);

        return Ok(new PagedResponse<ProductDto>(items, query.Page, query.PageSize, totalCount, totalPages));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken ct)
    {
        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product is null)
        {
            return NotFound();
        }

        return Ok(product.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var validation = await _createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));
        }

        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId, ct);
        if (!categoryExists)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
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

        return CreatedAtAction(nameof(GetById), new { id = product.Id, version = "1" }, product.ToDto());
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDto>> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct)
    {
        var validation = await _updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));
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

        return Ok(product.ToDto());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product is null)
        {
            return NotFound();
        }

        _db.Products.Remove(product);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
