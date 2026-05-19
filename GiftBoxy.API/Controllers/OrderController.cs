using GiftBoxy.Application.DTOs.Order;
using GiftBoxy.Domain.Entities;
using GiftBoxy.Domain.Enums;
using GiftBoxy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GiftBoxy.API.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        // BUYER ENDPOINTS

        // Səbətdəki məhsullardan sifariş yarat
        [Authorize(Roles = "Buyer")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Səbəti götür
            var cart = await _context.Carts
     .Include(c => c.CartItems)
         .ThenInclude(ci => ci.Product)
     .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
                return BadRequest("Cart is empty");

            var cartItems = cart.CartItems.ToList();

            if (!cartItems.Any())
                return BadRequest("Cart is empty");

            // Stok yoxlanışı
            foreach (var item in cartItems)
            {
                if (item.Product.StockCount < item.Quantity)
                    return BadRequest($"'{item.Product.Title}' üçün kifayət qədər stok yoxdur");
            }

            var totalPrice = cartItems.Sum(c => c.Product.Price * c.Quantity);

            // Kupon tətbiq et
            if (!string.IsNullOrWhiteSpace(dto.CouponCode))
            {
                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c =>
                        c.Code == dto.CouponCode.ToUpper() &&
                        c.IsActive &&
                        c.ExpiryDate > DateTime.UtcNow &&
                        c.UsedCount < c.UsageLimit);

                if (coupon == null)
                    return BadRequest("Invalid or expired coupon");

                if (totalPrice < coupon.MinimumAmount)
                    return BadRequest($"Minimum cart amount is {coupon.MinimumAmount} AZN");

                var discount = totalPrice * (coupon.DiscountPercent / 100);
                totalPrice -= discount;

                // Kupon istifadə sayını artır
                coupon.UsedCount++;
            }

            // Sifarişi yarat
            var order = new Order
            {
                UserId = userId,
                ShippingAddress = dto.ShippingAddress,
                PaymentMethod = dto.PaymentMethod,
                TotalPrice = Math.Round(totalPrice, 2),
                Status = OrderStatus.Pending,
                PaymentStatus = dto.PaymentMethod == PaymentMethod.Card
                    ? PaymentStatus.Paid
                    : PaymentStatus.Pending,
                OrderItems = cartItems.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    Quantity = c.Quantity,
                    Price = c.Product.Price
                }).ToList()
            };

            _context.Orders.Add(order);

            // Stokları azalt
            foreach (var item in cartItems)
            {
                item.Product.StockCount -= item.Quantity;
            }

            // Səbəti təmizlə
            _context.CartItems.RemoveRange(cartItems);
            _context.Carts.Remove(cart);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Order created", orderId = order.Id });
        }

        [Authorize(Roles = "Buyer")]
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Images)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => MapToDto(o))
                .ToListAsync();

            return Ok(orders);
        }

        [Authorize(Roles = "Buyer")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
                return NotFound();

            return Ok(MapToDto(order));
        }

        [Authorize(Roles = "Buyer")]
        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
                return NotFound();

            // Yalnız Pending və Confirmed statusda ləğv edilə bilər
            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
                return BadRequest("Order cannot be cancelled at this stage");

            order.Status = OrderStatus.Cancelled;

            // Stokları geri qaytar
            foreach (var item in order.OrderItems!)
            {
                item.Product.StockCount += item.Quantity;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Order cancelled" });
        }

        // SELLER ENDPOINTS

        [Authorize(Roles = "Seller")]
        [HttpGet("seller-orders")]
        public async Task<IActionResult> GetSellerOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Images)
                .Where(o => o.OrderItems!.Any(oi => oi.Product.UserId == userId))
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => MapToDto(o))
                .ToListAsync();

            return Ok(orders);
        }

        [Authorize(Roles = "Seller")]
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Bu sifarişdə seller-in məhsulu varmı?
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o =>
                    o.Id == id &&
                    o.OrderItems!.Any(oi => oi.Product.UserId == userId));

            if (order == null)
                return NotFound();

            // Ləğv edilmiş sifarişin statusu dəyişdirilə bilməz
            if (order.Status == OrderStatus.Cancelled)
                return BadRequest("Cancelled orders cannot be updated");

            // Delivered-dan geri qayıda bilməz
            if (order.Status == OrderStatus.Delivered)
                return BadRequest("Delivered orders cannot be updated");

            order.Status = dto.Status;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Order status updated", status = dto.Status.ToString() });
        }

        // PRIVATE METODLAR

        private static OrderResponseDto MapToDto(Order o) => new()
        {
            Id = o.Id,
            TotalPrice = o.TotalPrice,
            Status = o.Status,
            PaymentStatus = o.PaymentStatus,
            PaymentMethod = o.PaymentMethod,
            ShippingAddress = o.ShippingAddress,
            CreatedAt = o.CreatedAt,
            Items = o.OrderItems?.Select(oi => new OrderItemResponseDto
            {
                ProductId = oi.ProductId,
                ProductTitle = oi.Product?.Title ?? "",
                ProductImage = oi.Product?.Images?.FirstOrDefault()?.ImageUrl,
                Quantity = oi.Quantity,
                Price = oi.Price,
                Subtotal = oi.Price * oi.Quantity
            }).ToList() ?? new()
        };
    }
}
