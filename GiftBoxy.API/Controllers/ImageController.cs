using GiftBoxy.Domain.Entities;
using GiftBoxy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GiftBoxy.API.Controllers
{
    [Route("api/images")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ImageController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [Authorize(Roles = "Seller")]
        [HttpPost("upload/{productId}")]
        public async Task<IActionResult> Upload(int productId, IFormFile file)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Məhsul bu seller-ə məxsusdurmu?
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.UserId == userId);

            if (product == null)
                return NotFound("Product not found");

            // Fayl yoxlanışı
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                return BadRequest("Only jpg, jpeg, png, webp files are allowed");

            // Max 5MB
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("File size must be less than 5MB");

            // Qovluq yarat
            var uploadFolder = Path.Combine(_env.WebRootPath, "uploads", "products");
            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            // Unikal fayl adı
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadFolder, fileName);

            // Faylı saxla
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // DB-yə əlavə et
            var imageUrl = $"/uploads/products/{fileName}";

            var productImage = new ProductImage
            {
                ProductId = productId,
                ImageUrl = imageUrl
            };

            _context.ProductImages.Add(productImage);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = productImage.Id,
                imageUrl = imageUrl
            });
        }

        [Authorize(Roles = "Seller")]
        [HttpDelete("{imageId}")]
        public async Task<IActionResult> Delete(int imageId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var image = await _context.ProductImages
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == imageId && i.Product.UserId == userId);

            if (image == null)
                return NotFound();

            // Diskdən sil
            var filePath = Path.Combine(_env.WebRootPath, image.ImageUrl.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            // DB-dən sil
            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Image deleted" });
        }
    }
}
