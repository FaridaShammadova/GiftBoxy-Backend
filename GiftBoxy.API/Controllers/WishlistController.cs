using GiftBoxy.Domain.Entities;
using GiftBoxy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GiftBoxy.API.Controllers
{
    [Route("api/wishlist")]
    [ApiController]
    [Authorize(Roles = "Buyer")]
    public class WishlistController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WishlistController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetWishlist()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var wishlist = await _context.Wishlists
                .Include(w => w.WishlistItems)
                    .ThenInclude(wi => wi.Product)
                        .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wishlist == null)
                return Ok(new { items = new List<object>() });

            var items = wishlist.WishlistItems.Select(wi => new
            {
                wishlistItemId = wi.Id,
                productId = wi.ProductId,
                productTitle = wi.Product?.Title ?? "",
                productImage = wi.Product?.Images?.FirstOrDefault()?.ImageUrl,
                price = wi.Product?.Price ?? 0,
                oldPrice = wi.Product?.OldPrice,
                isPersonalized = wi.Product?.IsPersonalized ?? false
            }).ToList();

            return Ok(new
            {
                wishlistId = wishlist.Id,
                name = wishlist.Name,
                items
            });
        }

        [HttpPost("{productId}")]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                return NotFound("Product not found");

            // Wishlist yoxdursa yarat
            var wishlist = await _context.Wishlists
                .Include(w => w.WishlistItems)
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wishlist == null)
            {
                wishlist = new Wishlist {
                    UserId = userId,
                    WishlistItems = new List<WishlistItem>()
                };
                _context.Wishlists.Add(wishlist);
                await _context.SaveChangesAsync();
            }

            // Artıq əlavə edilibmi?
            var exists = wishlist.WishlistItems
                .Any(wi => wi.ProductId == productId);

            if (exists)
                return BadRequest("Product already in wishlist");

            wishlist.WishlistItems.Add(new WishlistItem
            {
                WishlistId = wishlist.Id,
                ProductId = productId
            });

            await _context.SaveChangesAsync();

            return Ok(new { message = "Added to wishlist" });
        }

        [HttpDelete("{wishlistItemId}")]
        public async Task<IActionResult> RemoveFromWishlist(int wishlistItemId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var wishlistItem = await _context.WishlistItems
                .Include(wi => wi.Wishlist)
                .FirstOrDefaultAsync(wi =>
                    wi.Id == wishlistItemId &&
                    wi.Wishlist.UserId == userId);

            if (wishlistItem == null)
                return NotFound();

            _context.WishlistItems.Remove(wishlistItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Removed from wishlist" });
        }
    }
}
