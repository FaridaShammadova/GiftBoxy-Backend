using GiftBoxy.Application.DTOs.Cart;
using GiftBoxy.Application.DTOs.Cart.GiftBoxy.Application.DTOs.Cart;
using GiftBoxy.Domain.Entities;
using GiftBoxy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GiftBoxy.API.Controllers
{
    [Route("api/cart")]
    [ApiController]
    [Authorize(Roles = "Buyer")]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return Ok(new CartResponseDto());

            return Ok(MapToDto(cart));
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
        {
            if (dto.Quantity <= 0)
                return BadRequest("Quantity must be at least 1");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

            if (product == null)
                return NotFound("Product not found");

            if (product.StockCount < dto.Quantity)
                return BadRequest("Not enough stock");

            // Cart yoxdursa yarat
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart {
                    UserId = userId,
                    CartItems = new List<CartItem>()
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // Məhsul artıq səbətdədirsə miqdarı artır
            var existingItem = cart.CartItems
                .FirstOrDefault(ci => ci.ProductId == dto.ProductId);

            if (existingItem != null)
            {
                var newQuantity = existingItem.Quantity + dto.Quantity;
                if (product.StockCount < newQuantity)
                    return BadRequest("Not enough stock");

                existingItem.Quantity = newQuantity;
            }
            else
            {
                cart.CartItems.Add(new CartItem
                {
                    CartId = cart.Id,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Added to cart" });
        }

        [HttpPut("{cartItemId}")]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, [FromBody] UpdateCartItemDto dto)
        {
            if (dto.Quantity <= 0)
                return BadRequest("Quantity must be at least 1");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart.UserId == userId);

            if (cartItem == null)
                return NotFound();

            if (cartItem.Product.StockCount < dto.Quantity)
                return BadRequest("Not enough stock");

            cartItem.Quantity = dto.Quantity;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cart updated" });
        }

        [HttpDelete("{cartItemId}")]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart.UserId == userId);

            if (cartItem == null)
                return NotFound();

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Item removed from cart" });
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return Ok(new { message = "Cart is already empty" });

            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cart cleared" });
        }

        private static CartResponseDto MapToDto(Cart cart) => new()
        {
            CartId = cart.Id,
            Items = cart.CartItems.Select(ci => new CartItemResponseDto
            {
                CartItemId = ci.Id,
                ProductId = ci.ProductId,
                ProductTitle = ci.Product?.Title ?? "",
                ProductImage = ci.Product?.Images?.FirstOrDefault()?.ImageUrl,
                Price = ci.Product?.Price ?? 0,
                OldPrice = ci.Product?.OldPrice,
                Quantity = ci.Quantity,
                Subtotal = (ci.Product?.Price ?? 0) * ci.Quantity,
                StockCount = ci.Product?.StockCount ?? 0
            }).ToList(),
            Total = cart.CartItems.Sum(ci => (ci.Product?.Price ?? 0) * ci.Quantity)
        };
    }
}
