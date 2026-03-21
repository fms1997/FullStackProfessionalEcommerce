using Asp.Versioning;
using FulSpectrum.Api.Services;
using FulSpectrum.Domain.Catalog;
using FulSpectrum.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FulSpectrum.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Policy = "CanManageCatalog")]
public sealed class AdminController : ControllerBase
{
    private readonly FulSpectrumDbContext _db;
    private readonly IImageStorageService _imageStorageService;
    private readonly ICatalogCacheService _catalogCache;
    public AdminController(FulSpectrumDbContext db, IImageStorageService imageStorageService, ICatalogCacheService catalogCache)
    {
        _db = db;
        _imageStorageService = imageStorageService;
        _catalogCache = catalogCache;
    }

    [HttpGet("catalog/products")]
    public async Task<ActionResult<IReadOnlyCollection<AdminProductDto>>> GetProducts([FromQuery] string? search, [FromQuery] bool? isPublished, CancellationToken ct)
    {
        var query = _db.Products
            .AsNoTracking()
            .Include(x => x.Variants)
            .ThenInclude(v => v.Inventory)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Name.Contains(term) || x.Sku.Contains(term));
        }

        if (isPublished.HasValue)
        {
            query = query.Where(x => x.IsPublished == isPublished.Value);
        }

        var data = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new AdminProductDto(
                x.Id,
                x.CategoryId,
                x.Name,
                x.Slug,
                x.Sku,
                x.BasePrice,
                x.Currency,
                x.IsPublished,
                x.CreatedAtUtc,
                x.Variants.Select(v => new AdminVariantDto(
                    v.Id,
                    v.ProductId,
                    v.VariantSku,
                    v.Name,
                    v.PriceDelta,
                    v.IsDefault,
                    v.Inventory != null ? v.Inventory.QuantityOnHand : 0,
                    v.Inventory != null ? v.Inventory.ReservedQuantity : 0,
                    v.Inventory != null ? v.Inventory.ReorderThreshold : 0)).ToList()))
            .ToListAsync(ct);

        return Ok(data);
    }

    [HttpPost("catalog/products")]
    public async Task<ActionResult<AdminProductDto>> CreateProduct([FromBody] UpsertAdminProductRequest request, CancellationToken ct)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            CategoryId = request.CategoryId,
            Name = request.Name.Trim(),
            Slug = request.Slug.Trim(),
            Sku = request.Sku.Trim(),
            BasePrice = request.BasePrice,
            Currency = request.Currency.Trim().ToUpperInvariant(),
            IsPublished = request.IsPublished,
            CreatedAtUtc = DateTime.UtcNow
        };

        foreach (var variant in request.Variants)
        {
            var variantEntity = new ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                VariantSku = variant.VariantSku.Trim(),
                Name = variant.Name.Trim(),
                PriceDelta = variant.PriceDelta,
                IsDefault = variant.IsDefault
            };

            variantEntity.Inventory = new InventoryItem
            {
                Id = Guid.NewGuid(),
                VariantId = variantEntity.Id,
                QuantityOnHand = variant.QuantityOnHand,
                ReservedQuantity = variant.ReservedQuantity,
                ReorderThreshold = variant.ReorderThreshold,
                UpdatedAtUtc = DateTime.UtcNow
            };

            product.Variants.Add(variantEntity);
        }

        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);
        await _catalogCache.InvalidateCatalogAsync(ct);
        return CreatedAtAction(nameof(GetProducts), new { version = "1" }, await BuildProductDto(product.Id, ct));
    }

    [HttpPut("catalog/products/{id:guid}")]
    public async Task<ActionResult<AdminProductDto>> UpdateProduct(
      Guid id,
      [FromBody] UpsertAdminProductRequest request,
      CancellationToken ct)
    {
        var product = await _db.Products
            .Include(x => x.Variants)
            .ThenInclude(v => v.Inventory)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (product is null)
            return NotFound();

        product.CategoryId = request.CategoryId;
        product.Name = request.Name.Trim();
        product.Slug = request.Slug.Trim();
        product.Sku = request.Sku.Trim();
        product.BasePrice = request.BasePrice;
        product.Currency = request.Currency.Trim().ToUpperInvariant();
        product.IsPublished = request.IsPublished;

        var existingInventories = product.Variants
            .Where(v => v.Inventory != null)
            .Select(v => v.Inventory!)
            .ToList();

        var existingVariants = product.Variants.ToList();

        _db.Inventory.RemoveRange(existingInventories);
        _db.Variants.RemoveRange(existingVariants);

        await _db.SaveChangesAsync(ct);

        foreach (var variant in request.Variants)
        {
            var variantEntity = new ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                VariantSku = variant.VariantSku.Trim(),
                Name = variant.Name.Trim(),
                PriceDelta = variant.PriceDelta,
                IsDefault = variant.IsDefault
            };

            variantEntity.Inventory = new InventoryItem
            {
                Id = Guid.NewGuid(),
                VariantId = variantEntity.Id,
                QuantityOnHand = variant.QuantityOnHand,
                ReservedQuantity = variant.ReservedQuantity,
                ReorderThreshold = variant.ReorderThreshold,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _db.Variants.Add(variantEntity);
        }

        await _db.SaveChangesAsync(ct);
        await _catalogCache.InvalidateCatalogAsync(ct);
        return Ok(await BuildProductDto(product.Id, ct));
    }
    [HttpDelete("catalog/products/{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken ct)
    {
        var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (product is null) return NotFound();

        _db.Products.Remove(product);
        await _db.SaveChangesAsync(ct);
        await _catalogCache.InvalidateCatalogAsync(ct);
        return NoContent();
    }

    [HttpPost("catalog/products/bulk-publish")]
    public async Task<IActionResult> BulkPublish([FromBody] BulkPublishRequest request, CancellationToken ct)
    {
        var products = await _db.Products.Where(x => request.ProductIds.Contains(x.Id)).ToListAsync(ct);
        products.ForEach(x => x.IsPublished = request.IsPublished);
        await _db.SaveChangesAsync(ct);
        await _catalogCache.InvalidateCatalogAsync(ct);
        return Ok(new { affected = products.Count });
    }

    [HttpDelete("catalog/products/bulk-delete")]
    public async Task<IActionResult> BulkDelete([FromBody] BulkDeleteRequest request, CancellationToken ct)
    {
        var products = await _db.Products.Where(x => request.ProductIds.Contains(x.Id)).ToListAsync(ct);
        _db.Products.RemoveRange(products);
        await _db.SaveChangesAsync(ct);
        await _catalogCache.InvalidateCatalogAsync(ct);
        return Ok(new { affected = products.Count });
    }

    [HttpPatch("catalog/variants/{variantId:guid}/stock")]
    public async Task<ActionResult<AdminVariantDto>> UpdateVariantStock(Guid variantId, [FromBody] UpdateVariantStockRequest request, CancellationToken ct)
    {
        var variant = await _db.Variants.Include(v => v.Inventory).FirstOrDefaultAsync(v => v.Id == variantId, ct);
        if (variant is null)
        {
            return NotFound();
        }

        if (variant.Inventory is null)
        {
            variant.Inventory = new InventoryItem
            {
                Id = Guid.NewGuid(),
                VariantId = variant.Id,
                QuantityOnHand = request.QuantityOnHand,
                ReservedQuantity = request.ReservedQuantity,
                ReorderThreshold = request.ReorderThreshold,
                UpdatedAtUtc = DateTime.UtcNow
            };
        }
        else
        {
            variant.Inventory.QuantityOnHand = request.QuantityOnHand;
            variant.Inventory.ReservedQuantity = request.ReservedQuantity;
            variant.Inventory.ReorderThreshold = request.ReorderThreshold;
            variant.Inventory.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);

        return Ok(new AdminVariantDto(
            variant.Id,
            variant.ProductId,
            variant.VariantSku,
            variant.Name,
            variant.PriceDelta,
            variant.IsDefault,
            variant.Inventory.QuantityOnHand,
            variant.Inventory.ReservedQuantity,
            variant.Inventory.ReorderThreshold));
    }

    [HttpGet("orders")]
    public async Task<ActionResult<IReadOnlyCollection<AdminOrderDto>>> GetOrders([FromQuery] string? status, [FromQuery] string? search, CancellationToken ct)
    {
        var query = _db.Orders.AsNoTracking().Include(o => o.Items).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(o => o.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(o => o.ShippingFullName.Contains(term) || o.ShippingPostalCode.Contains(term));
        }

        var items = await query.OrderByDescending(o => o.CreatedAtUtc)
            .Select(o => new AdminOrderDto(o.Id, o.UserId, o.Status.ToString(), o.Currency, o.Total, o.Items.Sum(i => i.Quantity), o.CreatedAtUtc, o.UpdatedAtUtc))
            .ToListAsync(ct);
         return Ok(items);
    }

    [HttpPatch("orders/{id:guid}/status")]
    public async Task<ActionResult> UpdateAdminOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request, CancellationToken ct)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null) return NotFound();

        if (!Enum.TryParse<OrderStatus>(request.Status, true, out var parsedStatus))
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [nameof(request.Status)] = ["Estado inválido."]
            }));
        }

        if (!order.TryTransitionTo(parsedStatus, out var error))
        {
            return Conflict(new { message = error });
        }

        await _db.SaveChangesAsync(ct);
        await _catalogCache.InvalidateCatalogAsync(ct);
        return NoContent();
    }

    [HttpPost("uploads/images")]
    [RequestSizeLimit(10_000_000)]
    public async Task<ActionResult<ImageUploadResultDto>> UploadImage([FromForm] ImageUploadRequest request, CancellationToken ct)
    {
        if (request.File is null || request.File.Length == 0)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { [nameof(request.File)] = ["Archivo requerido."] }));
        }

        await using var stream = request.File.OpenReadStream();
        var url = await _imageStorageService.UploadAsync(stream, request.File.FileName, request.File.ContentType, ct);
        return Ok(new ImageUploadResultDto(url));
    }

    private async Task<AdminProductDto> BuildProductDto(Guid id, CancellationToken ct)
    {
        return await _db.Products
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Include(x => x.Variants)
            .ThenInclude(v => v.Inventory)
            .Select(x => new AdminProductDto(
                x.Id,
                x.CategoryId,
                x.Name,
                x.Slug,
                x.Sku,
                x.BasePrice,
                x.Currency,
                x.IsPublished,
                x.CreatedAtUtc,
                x.Variants.Select(v => new AdminVariantDto(
                    v.Id,
                    v.ProductId,
                    v.VariantSku,
                    v.Name,
                    v.PriceDelta,
                    v.IsDefault,
                    v.Inventory != null ? v.Inventory.QuantityOnHand : 0,
                    v.Inventory != null ? v.Inventory.ReservedQuantity : 0,
                    v.Inventory != null ? v.Inventory.ReorderThreshold : 0)).ToList()))
            .FirstAsync(ct);
    }
}

public sealed record AdminProductDto(Guid Id, Guid CategoryId, string Name, string Slug, string Sku, decimal BasePrice, string Currency, bool IsPublished, DateTime CreatedAtUtc, IReadOnlyCollection<AdminVariantDto> Variants);
public sealed record AdminVariantDto(Guid Id, Guid ProductId, string VariantSku, string Name, decimal PriceDelta, bool IsDefault, int QuantityOnHand, int ReservedQuantity, int ReorderThreshold);
public sealed record UpsertAdminProductRequest(Guid CategoryId, string Name, string Slug, string Sku, decimal BasePrice, string Currency, bool IsPublished, IReadOnlyCollection<UpsertAdminVariantRequest> Variants);
public sealed record UpsertAdminVariantRequest(string VariantSku, string Name, decimal PriceDelta, bool IsDefault, int QuantityOnHand, int ReservedQuantity, int ReorderThreshold);
public sealed record BulkPublishRequest(IReadOnlyCollection<Guid> ProductIds, bool IsPublished);
public sealed record BulkDeleteRequest(IReadOnlyCollection<Guid> ProductIds);
public sealed record UpdateVariantStockRequest(int QuantityOnHand, int ReservedQuantity, int ReorderThreshold);
public sealed record AdminOrderDto(Guid Id, Guid UserId, string Status, string Currency, decimal Total, int TotalItems, DateTime CreatedAtUtc, DateTime UpdatedAtUtc);
public sealed record ImageUploadRequest(IFormFile File);
public sealed record ImageUploadResultDto(string Url);
