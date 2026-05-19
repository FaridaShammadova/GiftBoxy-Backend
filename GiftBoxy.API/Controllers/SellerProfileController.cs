using GiftBoxy.Application.DTOs.SellerProfile;
using GiftBoxy.Domain.Entities;
using GiftBoxy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace GiftBoxy.API.Controllers
{
    [Route("api/seller-profiles")]
    [ApiController]
    public class SellerProfileController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly Cloudinary _cloudinary;

        public SellerProfileController(AppDbContext context, Cloudinary cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
        }

        // PUBLIC ENDPOINTS

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var profile = await _context.SellerProfiles
                .Include(s => s.SellerCategories)
                    .ThenInclude(sc => sc.Category)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (profile == null)
                return NotFound();

            return Ok(MapToDto(profile));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var profiles = await _context.SellerProfiles
                .Include(s => s.SellerCategories)
                    .ThenInclude(sc => sc.Category)
                .Select(s => MapToDto(s))
                .ToListAsync();

            return Ok(profiles);
        }

        // SELLER ENDPOINTS

        [Authorize(Roles = "Seller")]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var profile = await _context.SellerProfiles
                .Include(s => s.SellerCategories)
                    .ThenInclude(sc => sc.Category)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (profile == null)
                return NotFound();

            return Ok(MapToDto(profile));
        }

        [Authorize(Roles = "Seller")]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateSellerProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var profile = await _context.SellerProfiles
                .Include(s => s.SellerCategories)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (profile == null)
                return NotFound();

            if (dto.StoreName != null) profile.StoreName = dto.StoreName;
            if (dto.ShopUrl != null) profile.ShopUrl = dto.ShopUrl;
            if (dto.Bio != null) profile.Bio = dto.Bio;
            if (dto.Location != null) profile.Location = dto.Location;

            // Kateqoriyaları yenilə
            if (dto.Categories != null)
            {
                // Köhnələri sil
                _context.SellerCategories.RemoveRange(profile.SellerCategories);

                // Yenilərini əlavə et
                foreach (var categoryName in dto.Categories)
                {
                    var category = await _context.Categories
                        .FirstOrDefaultAsync(c => c.Name == categoryName);

                    if (category == null) continue;

                    _context.SellerCategories.Add(new SellerCategory
                    {
                        SellerProfileId = profile.Id,
                        CategoryId = category.Id
                    });
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated" });
        }

        [Authorize(Roles = "Seller")]
        [HttpPost("me/avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var profile = await _context.SellerProfiles
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (profile == null)
                return NotFound();

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                return BadRequest("Only jpg, jpeg, png, webp files are allowed");

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("File size must be less than 5MB");

            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "giftboxy/avatars"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                return BadRequest("Şəkil yüklənmədi");

            profile.Avatar = uploadResult.SecureUrl.ToString();
            await _context.SaveChangesAsync();

            return Ok(new { avatarUrl = profile.Avatar });
        }
     
        // PRIVATE METODLAR

        private static SellerProfileResponseDto MapToDto(SellerProfile s) => new()
        {
            Id = s.Id,
            StoreName = s.StoreName,
            ShopUrl = s.ShopUrl,
            Avatar = s.Avatar,
            Bio = s.Bio,
            Location = s.Location,
            Rating = s.Rating,
            TotalSales = s.TotalSales,
            Followers = s.Followers,
            Categories = s.SellerCategories?
                .Select(sc => sc.Category?.Name ?? "")
                .ToList() ?? new()
        };
    }
}
