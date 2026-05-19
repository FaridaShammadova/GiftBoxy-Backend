using GiftBoxy.Application.DTOs.Product;
using GiftBoxy.Domain.Entities;
using GiftBoxy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace GiftBoxy.API.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly Cloudinary _cloudinary;

        public ProductController(AppDbContext context, Cloudinary cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
        }

        // PUBLIC ENDPOINTS (giriş etmədən görünür)

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? category = null,
            [FromQuery] string? search = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null)
        {
            var query = _context.Products
                .Include(p => p.Images)
                .Include(p => p.Category)
                .Include(p => p.SellerProfile)
                .Include(p => p.RecipientTags)
                .Include(p => p.OccasionTags)
                .Include(p => p.InterestTags)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(p => p.Category.Slug == category);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Title.Contains(search) ||
                                         p.Description.Contains(search));

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            var total = await query.CountAsync();

            var productEntities = await query
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                 .AsSplitQuery()
                .ToListAsync();

            var products = productEntities.Select(p => MapToDto(p)).ToList();

            return Ok(new
            {
                total,
                page,
                pageSize,
                data = products
            });
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Category)
                .Include(p => p.SellerProfile)
                .Include(p => p.RecipientTags)
                .Include(p => p.OccasionTags)
                .Include(p => p.InterestTags)
                .FirstOrDefaultAsync(p => p.Slug == slug);

            if (product == null)
                return NotFound();

            return Ok(MapToDto(product));
        }

        [HttpPost("gift-finder")]
        public async Task<IActionResult> GiftFinder([FromBody] GiftFinderDto dto)
        {
            var query = _context.Products
                .Include(p => p.Images)
                .Include(p => p.Category)
                .Include(p => p.SellerProfile)
                .Include(p => p.RecipientTags)
                .Include(p => p.OccasionTags)
                .Include(p => p.InterestTags)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(dto.Occasion))
                query = query.Where(p =>
                    p.OccasionTags.Any(t => t.Name == dto.Occasion));

            if (!string.IsNullOrWhiteSpace(dto.Recipient))
                query = query.Where(p =>
                    p.RecipientTags.Any(t => t.Name == dto.Recipient));

            if (!string.IsNullOrWhiteSpace(dto.Interest))
                query = query.Where(p =>
                    p.InterestTags.Any(t => t.Name == dto.Interest));

            if (dto.MinBudget.HasValue)
                query = query.Where(p => p.Price >= dto.MinBudget.Value);

            if (dto.MaxBudget.HasValue)
                query = query.Where(p => p.Price <= dto.MaxBudget.Value);

            var products = await query
                .Take(20)
                .Select(p => MapToDto(p))
                .ToListAsync();

            return Ok(products);
        }

        // SELLER ENDPOINTS (yalnız seller üçün)

        [Authorize(Roles = "Seller")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ProductCreateDto dto, IFormFile? image)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var sellerProfile = await _context.SellerProfiles
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (sellerProfile == null)
                return NotFound("Seller profile not found");

            var product = new Product
            {
                Title = dto.Title,
                Slug = GenerateSlug(dto.Title),
                Description = dto.Description,
                Price = dto.Price,
                OldPrice = dto.OldPrice,
                StockCount = dto.StockCount,
                IsFeatured = dto.IsFeatured,
                IsBestSeller = dto.IsBestSeller,
                IsPersonalized = dto.IsPersonalized,
                IsNew = dto.IsNew,
                BudgetRange = dto.BudgetRange,
                CategoryId = dto.CategoryId,
                SellerProfileId = sellerProfile.Id,
                UserId = userId,
                RecipientTags = dto.RecipientTags
                    .Select(t => new ProductRecipientTag { Name = t }).ToList(),
                OccasionTags = dto.OccasionTags
                    .Select(t => new ProductOccasionTag { Name = t }).ToList(),
                InterestTags = dto.InterestTags
                    .Select(t => new ProductInterestTag { Name = t }).ToList()
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            if (dto.Images != null && dto.Images.Any())
                await UploadImages(dto.Images, product.Id);

            return CreatedAtAction(nameof(GetBySlug),
                new { slug = product.Slug },
                new { id = product.Id, slug = product.Slug });
        }

        [Authorize(Roles = "Seller")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] ProductUpdateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var product = await _context.Products
                .Include(p => p.RecipientTags)
                .Include(p => p.OccasionTags)
                .Include(p => p.InterestTags)
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (product == null)
                return NotFound();

            if (dto.Title != null)
            {
                product.Title = dto.Title;
                product.Slug = GenerateSlug(dto.Title);
            }
            if (dto.Description != null) product.Description = dto.Description;
            if (dto.Price.HasValue) product.Price = dto.Price.Value;
            if (dto.OldPrice.HasValue) product.OldPrice = dto.OldPrice.Value;
            if (dto.StockCount.HasValue) product.StockCount = dto.StockCount.Value;
            if (dto.IsFeatured.HasValue) product.IsFeatured = dto.IsFeatured.Value;
            if (dto.IsBestSeller.HasValue) product.IsBestSeller = dto.IsBestSeller.Value;
            if (dto.IsPersonalized.HasValue) product.IsPersonalized = dto.IsPersonalized.Value;
            if (dto.IsNew.HasValue) product.IsNew = dto.IsNew.Value;
            if (dto.BudgetRange != null) product.BudgetRange = dto.BudgetRange;
            if (dto.CategoryId.HasValue) product.CategoryId = dto.CategoryId.Value;

            // Tagları yenilə
            if (dto.RecipientTags != null)
            {
                product.RecipientTags.Clear();
                product.RecipientTags = dto.RecipientTags
                    .Select(t => new ProductRecipientTag { Name = t }).ToList();
            }
            if (dto.OccasionTags != null)
            {
                product.OccasionTags.Clear();
                product.OccasionTags = dto.OccasionTags
                    .Select(t => new ProductOccasionTag { Name = t }).ToList();
            }
            if (dto.InterestTags != null)
            {
                product.InterestTags.Clear();
                product.InterestTags = dto.InterestTags
                    .Select(t => new ProductInterestTag { Name = t }).ToList();
            }

            await _context.SaveChangesAsync();

            if (dto.Images != null && dto.Images.Any())
                await UploadImages(dto.Images, product.Id);

            return Ok(new { message = "Product updated" });
        }

        [Authorize(Roles = "Seller")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (product == null)
                return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product deleted" });
        }

        [Authorize(Roles = "Seller")]
        [HttpGet("my-products")]
        public async Task<IActionResult> GetMyProducts()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var products = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Category)
                .Include(p => p.SellerProfile)
                .Include(p => p.RecipientTags)
                .Include(p => p.OccasionTags)
                .Include(p => p.InterestTags)
                .Where(p => p.UserId == userId)
                .Select(p => MapToDto(p))
                .ToListAsync();

            return Ok(products);
        }

        // PRIVATE METODLAR

        private static ProductResponseDto MapToDto(Product p) => new()
        {
            Id = p.Id,
            Title = p.Title,
            Slug = p.Slug,
            Description = p.Description,
            Price = p.Price,
            OldPrice = p.OldPrice,
            Rating = p.Rating,
            StockCount = p.StockCount,
            IsFeatured = p.IsFeatured,
            IsBestSeller = p.IsBestSeller,
            IsPersonalized = p.IsPersonalized,
            IsNew = p.IsNew,
            Badge = p.Badge,
            BudgetRange = p.BudgetRange,
            SellerId = p.SellerProfile?.UserId,
            CategoryName = p.Category?.Name ?? "",
            SellerStoreName = p.SellerProfile?.StoreName ?? "",
            Images = p.Images?.Select(i => i.ImageUrl).ToList() ?? new(),
            RecipientTags = p.RecipientTags?.Select(t => t.Name).ToList() ?? new(),
            OccasionTags = p.OccasionTags?.Select(t => t.Name).ToList() ?? new(),
            InterestTags = p.InterestTags?.Select(t => t.Name).ToList() ?? new()
        };

        private static string GenerateSlug(string title)
        {
            var slug = title.ToLower().Trim();
            slug = Regex.Replace(slug, @"\s+", "-");
            slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
            return slug;
        }

        private async Task UploadImages(List<IFormFile> images, int productId)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            foreach (var image in images)
            {
                if (image.Length == 0) continue;

                var extension = Path.GetExtension(image.FileName).ToLower();
                if (!allowedExtensions.Contains(extension)) continue;
                if (image.Length > 5 * 1024 * 1024) continue;

                using var stream = image.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(image.FileName, stream),
                    Folder = "giftboxy/products"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    _context.ProductImages.Add(new ProductImage
                    {
                        ProductId = productId,
                        ImageUrl = uploadResult.SecureUrl.ToString()
                    });
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
