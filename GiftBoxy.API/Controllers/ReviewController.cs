using GiftBoxy.Application.DTOs.Review;
using GiftBoxy.Domain.Entities;
using GiftBoxy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GiftBoxy.API.Controllers
{
    [Route("api/reviews")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReviewController(AppDbContext context)
        {
            _context = context;
        }

        // -----------------------------------------------
        // PUBLIC ENDPOINTS
        // -----------------------------------------------

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetByProduct(int productId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewResponseDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    UserName = r.User.Name,
                    UserAvatar = r.User.Avatar,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            var averageRating = reviews.Count > 0
                ? Math.Round(reviews.Average(r => r.Rating), 1)
                : 0;

            return Ok(new
            {
                averageRating,
                totalReviews = reviews.Count,
                data = reviews
            });
        }

        // -----------------------------------------------
        // BUYER ENDPOINTS
        // -----------------------------------------------

        [Authorize(Roles = "Buyer")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReviewDto dto)
        {
            if (dto.Rating < 1 || dto.Rating > 5)
                return BadRequest("Rating must be between 1 and 5");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Məhsul mövcuddurmu?
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

            if (product == null)
                return NotFound("Product not found");

            // Bu user artıq rəy yazıbmı?
            var existing = await _context.Reviews
                .AnyAsync(r => r.ProductId == dto.ProductId && r.UserId == userId);

            if (existing)
                return BadRequest("You have already reviewed this product");

            // Yalnız alan rəy yaza bilər — Order yoxlanışı
            var hasPurchased = await _context.Orders
                .Include(o => o.OrderItems)
                .AnyAsync(o => o.UserId == userId &&
                               o.OrderItems.Any(oi => oi.ProductId == dto.ProductId) &&
                               o.Status == Domain.Enums.OrderStatus.Delivered);

            if (!hasPurchased)
                return BadRequest("You can only review products you have purchased");

            var review = new Review
            {
                Rating = dto.Rating,
                Comment = dto.Comment,
                ProductId = dto.ProductId,
                UserId = userId
            };

            _context.Reviews.Add(review);

            // Məhsulun ortalama reytinqini yenilə
            await _context.SaveChangesAsync();
            await UpdateProductRating(dto.ProductId);

            return Ok(new { message = "Review added" });
        }

        [Authorize(Roles = "Buyer")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateReviewDto dto)
        {
            if (dto.Rating < 1 || dto.Rating > 5)
                return BadRequest("Rating must be between 1 and 5");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (review == null)
                return NotFound();

            review.Rating = dto.Rating;
            review.Comment = dto.Comment;

            await _context.SaveChangesAsync();
            await UpdateProductRating(review.ProductId);

            return Ok(new { message = "Review updated" });
        }

        [Authorize(Roles = "Buyer")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (review == null)
                return NotFound();

            var productId = review.ProductId;

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            await UpdateProductRating(productId);

            return Ok(new { message = "Review deleted" });
        }

        // -----------------------------------------------
        // PRIVATE METODLAR
        // -----------------------------------------------

        private async Task UpdateProductRating(int productId)
        {
            var product = await _context.Products
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null) return;

            product.Rating = product.Reviews.Count > 0
                ? Math.Round(product.Reviews.Average(r => r.Rating), 1)
                : 0;

            await _context.SaveChangesAsync();
        }
    }
}
