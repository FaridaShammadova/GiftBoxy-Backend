using GiftBoxy.Application.DTOs.Coupon;
using GiftBoxy.Domain.Entities;
using GiftBoxy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GiftBoxy.API.Controllers
{
    [Route("api/coupons")]
    [ApiController]
    public class CouponController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CouponController(AppDbContext context)
        {
            _context = context;
        }

        // SELLER ENDPOINTS

        [Authorize(Roles = "Seller")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCouponDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Eyni kod artıq varmı?
            var exists = await _context.Coupons
                .AnyAsync(c => c.Code == dto.Code.ToUpper() && c.SellerId == userId);

            if (exists)
                return BadRequest("Coupon code already exists");

            if (dto.DiscountPercent <= 0 || dto.DiscountPercent > 100)
                return BadRequest("Discount percent must be between 1 and 100");

            if (dto.ExpiryDate <= DateTime.UtcNow)
                return BadRequest("Expiry date must be in the future");

            var coupon = new Coupon
            {
                Code = dto.Code.ToUpper().Trim(),
                DiscountPercent = dto.DiscountPercent,
                MinimumAmount = dto.MinimumAmount,
                UsageLimit = dto.UsageLimit,
                ExpiryDate = dto.ExpiryDate,
                SellerId = userId,
                IsActive = true
            };

            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Coupon created", code = coupon.Code });
        }

        [Authorize(Roles = "Seller")]
        [HttpGet("my-coupons")]
        public async Task<IActionResult> GetMyCoupons()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var coupons = await _context.Coupons
                .Where(c => c.SellerId == userId)
                .Select(c => new CouponResponseDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    DiscountPercent = c.DiscountPercent,
                    MinimumAmount = c.MinimumAmount,
                    UsageLimit = c.UsageLimit,
                    UsedCount = c.UsedCount,
                    ExpiryDate = c.ExpiryDate,
                    IsActive = c.IsActive
                })
                .ToListAsync();

            return Ok(coupons);
        }

        [Authorize(Roles = "Seller")]
        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> Toggle(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.Id == id && c.SellerId == userId);

            if (coupon == null)
                return NotFound();

            coupon.IsActive = !coupon.IsActive;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = coupon.IsActive ? "Coupon activated" : "Coupon deactivated",
                isActive = coupon.IsActive
            });
        }

        [Authorize(Roles = "Seller")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.Id == id && c.SellerId == userId);

            if (coupon == null)
                return NotFound();

            _context.Coupons.Remove(coupon);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Coupon deleted" });
        }

        // BUYER ENDPOINTS

        [Authorize(Roles = "Buyer")]
        [HttpPost("apply")]
        public async Task<IActionResult> Apply([FromBody] ApplyCouponDto dto)
        {
            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.Code == dto.Code.ToUpper() && c.IsActive);

            if (coupon == null)
                return BadRequest("Invalid or inactive coupon code");

            if (coupon.ExpiryDate < DateTime.UtcNow)
                return BadRequest("Coupon has expired");

            if (coupon.UsedCount >= coupon.UsageLimit)
                return BadRequest("Coupon usage limit reached");

            if (dto.CartTotal < coupon.MinimumAmount)
                return BadRequest($"Minimum cart amount is {coupon.MinimumAmount} AZN");

            var discountAmount = dto.CartTotal * (coupon.DiscountPercent / 100);
            var finalAmount = dto.CartTotal - discountAmount;

            return Ok(new
            {
                code = coupon.Code,
                discountPercent = coupon.DiscountPercent,
                discountAmount = Math.Round(discountAmount, 2),
                finalAmount = Math.Round(finalAmount, 2)
            });
        }
    }
}
