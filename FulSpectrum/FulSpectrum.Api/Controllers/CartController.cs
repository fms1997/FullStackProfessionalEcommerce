using Asp.Versioning;
using FulSpectrum.Domain.Catalog;
using FulSpectrum.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FulSpectrum.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/cart")]
[Authorize(Policy = "CustomerOrAdmin")]
public sealed class CartController : ControllerBase
{
    private const int MaxDistinctItems = 20;
    private const int MaxUnitsPerProduct = 10;

    private readonly FulSpectrumDbContext _db;

    public CartController(FulSpectrumDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<CartDto>> Get(CancellationToken ct)
    {
        var cart = await GetOrCreateCartAsync(ct);
        return Ok(await MapCartAsync(cart.Id, ct));
    }

    //[HttpPost("items")]
    //public async Task<ActionResult<CartDto>> AddItem([FromBody] AddCartItemRequest request, CancellationToken ct)
    //{
    //    if (request.Quantity <= 0)
    //    {
    //        return ValidationProblem(new ValidationProblemDetails(
    //            new Dictionary<string, string[]>
    //            {
    //                [nameof(request.Quantity)] = ["La cantidad debe ser mayor a 0."]
    //            }));
    //    }

    //    var cart = await GetOrCreateCartAsync(ct);

    //    if (!TryApplyRowVersion(cart, request.RowVersion, out var error))
    //    {
    //        return Conflict(new { message = error });
    //    }

    //    var product = await _db.Products
    //        .AsNoTracking()
    //        .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsPublished, ct);

    //    if (product is null)
    //    {
    //        return NotFound(new { message = "Producto no encontrado o no publicado." });
    //    }

    //    var available = await GetAvailableStockAsync(request.ProductId, ct);
    //    var existing = cart.Items.FirstOrDefault(x => x.ProductId == request.ProductId);

    //    if (existing is null && cart.Items.Count >= MaxDistinctItems)
    //    {
    //        return Conflict(new
    //        {
    //            message = $"El carrito permite hasta {MaxDistinctItems} productos distintos."
    //        });
    //    }

    //    var desired = (existing?.Quantity ?? 0) + request.Quantity;
    //    var capped = Math.Min(desired, MaxUnitsPerProduct);

    //    if (capped > available)
    //    {
    //        return Conflict(new
    //        {
    //            message = "No hay stock suficiente para la cantidad solicitada.",
    //            availableStock = available
    //        });
    //    }

    //    if (existing is null)
    //    {
    //        cart.Items.Add(new CartItem
    //        {
    //            Id = Guid.NewGuid(),
    //            CartId = cart.Id,
    //            ProductId = request.ProductId,
    //            Quantity = capped,
    //            CreatedAtUtc = DateTime.UtcNow,
    //            UpdatedAtUtc = DateTime.UtcNow
    //        });
    //    }
    //    else
    //    {
    //        existing.Quantity = capped;
    //        existing.UpdatedAtUtc = DateTime.UtcNow;
    //    }

    //    cart.UpdatedAtUtc = DateTime.UtcNow;

    //    try
    //    {
    //        await _db.SaveChangesAsync(ct);
    //    }
    //    catch (DbUpdateConcurrencyException)
    //    {
    //        return Conflict(new
    //        {
    //            message = "El carrito fue modificado por otro proceso. Recargá el carrito e intentá nuevamente."
    //        });
    //    }

    //    return Ok(await MapCartAsync(cart.Id, ct));
    //}


    //[HttpPost("items")]
    //public async Task<ActionResult<CartDto>> AddItem([FromBody] AddCartItemRequest request, CancellationToken ct)
    //{
    //    if (request.Quantity <= 0)
    //    {
    //        return ValidationProblem(new ValidationProblemDetails(
    //            new Dictionary<string, string[]>
    //            {
    //                [nameof(request.Quantity)] = ["La cantidad debe ser mayor a 0."]
    //            }));
    //    }

    //    var cart = await GetOrCreateCartAsync(ct);

    //    var product = await _db.Products
    //        .AsNoTracking()
    //        .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsPublished, ct);

    //    if (product is null)
    //    {
    //        return NotFound(new { message = "Producto no encontrado o no publicado." });
    //    }

    //    var available = await GetAvailableStockAsync(request.ProductId, ct);
    //    var existing = cart.Items.FirstOrDefault(x => x.ProductId == request.ProductId);

    //    if (existing is null && cart.Items.Count >= MaxDistinctItems)
    //    {
    //        return Conflict(new
    //        {
    //            message = $"El carrito permite hasta {MaxDistinctItems} productos distintos."
    //        });
    //    }

    //    var desired = (existing?.Quantity ?? 0) + request.Quantity;
    //    var capped = Math.Min(desired, MaxUnitsPerProduct);

    //    if (capped > available)
    //    {
    //        return Conflict(new
    //        {
    //            message = "No hay stock suficiente para la cantidad solicitada.",
    //            availableStock = available
    //        });
    //    }

    //    if (existing is null)
    //    {
    //        cart.Items.Add(new CartItem
    //        {
    //            Id = Guid.NewGuid(),
    //            CartId = cart.Id,
    //            ProductId = request.ProductId,
    //            Quantity = capped,
    //            CreatedAtUtc = DateTime.UtcNow,
    //            UpdatedAtUtc = DateTime.UtcNow
    //        });
    //    }
    //    else
    //    {
    //        existing.Quantity = capped;
    //        existing.UpdatedAtUtc = DateTime.UtcNow;
    //    }

    //    cart.UpdatedAtUtc = DateTime.UtcNow;

    //    try
    //    {
    //        await _db.SaveChangesAsync(ct);
    //    }
    //    catch (DbUpdateConcurrencyException ex)
    //    {
    //        return Conflict(new
    //        {
    //            message = "Conflicto de concurrencia al guardar el carrito.",
    //            entries = ex.Entries.Select(e => new
    //            {
    //                entity = e.Metadata.Name,
    //                state = e.State.ToString(),
    //                originalValues = e.Properties.ToDictionary(
    //                    p => p.Metadata.Name,
    //                    p => e.OriginalValues[p.Metadata.Name]
    //                )
    //            })
    //        });
    //    }

    //    return Ok(await MapCartAsync(cart.Id, ct));
    //}



    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddItem([FromBody] AddCartItemRequest request, CancellationToken ct)
    {
        if (request.Quantity <= 0)
        {
            return ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    [nameof(request.Quantity)] = ["La cantidad debe ser mayor a 0."]
                }));
        }

        var cart = await GetOrCreateCartAsync(ct);

        var product = await _db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsPublished, ct);

        if (product is null)
        {
            return NotFound(new { message = "Producto no encontrado o no publicado." });
        }

        var available = await GetAvailableStockAsync(request.ProductId, ct);

        var existing = await _db.CartItems
            .FirstOrDefaultAsync(x => x.CartId == cart.Id && x.ProductId == request.ProductId, ct);

        var distinctItemsCount = await _db.CartItems
            .CountAsync(x => x.CartId == cart.Id, ct);

        if (existing is null && distinctItemsCount >= MaxDistinctItems)
        {
            return Conflict(new
            {
                message = $"El carrito permite hasta {MaxDistinctItems} productos distintos."
            });
        }

        var desired = (existing?.Quantity ?? 0) + request.Quantity;
        var capped = Math.Min(desired, MaxUnitsPerProduct);

        if (capped > available)
        {
            return Conflict(new
            {
                message = "No hay stock suficiente para la cantidad solicitada.",
                availableStock = available
            });
        }

        if (existing is null)
        {
            _db.CartItems.Add(new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                ProductId = request.ProductId,
                Quantity = capped,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            });
        }
        else
        {
            existing.Quantity = capped;
            existing.UpdatedAtUtc = DateTime.UtcNow;
        }

        cart.UpdatedAtUtc = DateTime.UtcNow;

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new
            {
                message = "El carrito cambió mientras intentabas actualizarlo. Recargá e intentá de nuevo."
            });
        }

        return Ok(await MapCartAsync(cart.Id, ct));
    }











    [HttpPut("items/{productId:guid}")]
    public async Task<ActionResult<CartDto>> UpdateItem(Guid productId, [FromBody] UpdateCartItemRequest request, CancellationToken ct)
    {
        var cart = await GetOrCreateCartAsync(ct);
        if (!TryApplyRowVersion(cart, request.RowVersion, out var error))
        {
            return Conflict(new { message = error });
        }

        var existing = cart.Items.FirstOrDefault(x => x.ProductId == productId);
        if (existing is null)
        {
            return NotFound(new { message = "El producto no está en el carrito." });
        }

        if (request.Quantity <= 0)
        {
            _db.CartItems.Remove(existing);
        }
        else
        {
            var available = await GetAvailableStockAsync(productId, ct);
            var capped = Math.Min(request.Quantity, MaxUnitsPerProduct);
            if (capped > available)
            {
                return Conflict(new { message = "No hay stock suficiente para actualizar el carrito.", availableStock = available });
            }

            existing.Quantity = capped;
            existing.UpdatedAtUtc = DateTime.UtcNow;
        }

        cart.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return Ok(await MapCartAsync(cart.Id, ct));
    }

    [HttpDelete("items/{productId:guid}")]
    public async Task<ActionResult<CartDto>> RemoveItem(Guid productId, [FromQuery] string? rowVersion, CancellationToken ct)
    {
        var cart = await GetOrCreateCartAsync(ct);
        if (!TryApplyRowVersion(cart, rowVersion, out var error))
        {
            return Conflict(new { message = error });
        }

        var existing = cart.Items.FirstOrDefault(x => x.ProductId == productId);
        if (existing is not null)
        {
            _db.CartItems.Remove(existing);
            cart.UpdatedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        return Ok(await MapCartAsync(cart.Id, ct));
    }

    [HttpDelete]
    public async Task<ActionResult<CartDto>> Clear([FromQuery] string? rowVersion, CancellationToken ct)
    {
        var cart = await GetOrCreateCartAsync(ct);
        if (!TryApplyRowVersion(cart, rowVersion, out var error))
        {
            return Conflict(new { message = error });
        }

        _db.CartItems.RemoveRange(cart.Items);
        cart.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return Ok(await MapCartAsync(cart.Id, ct));
    }

    [HttpPost("merge")]
    public async Task<ActionResult<CartDto>> Merge([FromBody] MergeCartRequest request, CancellationToken ct)
    {
        var cart = await GetOrCreateCartAsync(ct);
        if (!TryApplyRowVersion(cart, request.RowVersion, out var error))
        {
            return Conflict(new { message = error });
        }

        foreach (var mergeItem in request.Items.Where(x => x.Quantity > 0))
        {
            if (cart.Items.Count >= MaxDistinctItems && cart.Items.All(x => x.ProductId != mergeItem.ProductId))
            {
                break;
            }

            var available = await GetAvailableStockAsync(mergeItem.ProductId, ct);
            if (available <= 0)
            {
                continue;
            }

            var existing = cart.Items.FirstOrDefault(x => x.ProductId == mergeItem.ProductId);
            var desired = (existing?.Quantity ?? 0) + mergeItem.Quantity;
            var capped = Math.Min(Math.Min(desired, MaxUnitsPerProduct), available);

            if (capped <= 0)
            {
                continue;
            }

            if (existing is null)
            {
                cart.Items.Add(new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    ProductId = mergeItem.ProductId,
                    Quantity = capped,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow
                });
            }
            else
            {
                existing.Quantity = capped;
                existing.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        cart.UpdatedAtUtc = DateTime.UtcNow;
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new
            {
                message = "El carrito fue modificado por otro proceso. Recargá el carrito e intentá nuevamente."
            });
        }

        return Ok(await MapCartAsync(cart.Id, ct));
    }

    private async Task<Cart> GetOrCreateCartAsync(CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out var id))
        {
            throw new UnauthorizedAccessException("Usuario inválido.");
        }

        var cart = await _db.Carts
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.UserId == id, ct);

        if (cart is not null)
        {
            return cart;
        }

        cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = id,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _db.Carts.Add(cart);
        await _db.SaveChangesAsync(ct);

        return cart;
    }

    private bool TryApplyRowVersion(Cart cart, string? rowVersion, out string? error)
    {
        error = null;

        if (string.IsNullOrWhiteSpace(rowVersion))
        {
            return true;
        }

        try
        {
            var expected = Convert.FromBase64String(rowVersion);
            var current = Convert.ToBase64String(cart.RowVersion);

            if (!cart.RowVersion.SequenceEqual(expected))
            {
                error = $"RowVersion conflict. Actual: {current}, enviada: {rowVersion}";
                return false;
            }

            _db.Entry(cart).Property(x => x.RowVersion).OriginalValue = expected;
            return true;
        }
        catch
        {
            error = "rowVersion inválido.";
            return false;
        }
    }
    private async Task<int> GetAvailableStockAsync(Guid productId, CancellationToken ct)
    {
        return await _db.Inventory
            .Where(x => x.Variant!.ProductId == productId)
            .Select(x => x.QuantityOnHand - x.ReservedQuantity)
            .SumAsync(ct);
    }

    private async Task<CartDto> MapCartAsync(Guid cartId, CancellationToken ct)
    {
        var cart = await _db.Carts
            .AsNoTracking()
            .Where(x => x.Id == cartId)
            .Select(x => new
            {
                x.Id,
                x.UserId,
                x.RowVersion,
                Items = x.Items.Select(i => new
                {
                    i.Id,
                    i.ProductId,
                    i.Quantity,
                    ProductName = i.Product!.Name,
                    i.Product.Sku,
                    i.Product.BasePrice,
                    i.Product.Currency,
                    AvailableStock = i.Product.Variants.Sum(v => v.Inventory != null ? (v.Inventory.QuantityOnHand - v.Inventory.ReservedQuantity) : 0)
                }).ToList()
            })
            .FirstAsync(ct);

        var currency = cart.Items.Select(x => x.Currency).FirstOrDefault() ?? "USD";

        var items = cart.Items.Select(item =>
        {
            var qty = Math.Min(item.Quantity, Math.Min(item.AvailableStock, MaxUnitsPerProduct));
            return new CartItemDto(item.Id, item.ProductId, item.ProductName, item.Sku, item.BasePrice, qty, MaxUnitsPerProduct, item.AvailableStock, item.BasePrice * qty);
        }).ToList();

        return new CartDto(
            cart.Id,
            cart.UserId,
            Convert.ToBase64String(cart.RowVersion),
            items,
            items.Sum(x => x.Quantity),
            items.Sum(x => x.LineTotal),
            currency);
    }
}
