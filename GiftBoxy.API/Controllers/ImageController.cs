using CloudinaryDotNet.Actions;
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
        private readonly CloudinaryDotNet.Cloudinary _cloudinary;

        public ImageController(AppDbContext context, CloudinaryDotNet.Cloudinary cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
        }

        [Authorize(Roles = "Seller")]
        [HttpPost("upload/{productId}")]
        public async Task<IActionResult> Upload(int productId, IFormFile file)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.UserId == userId);

            if (product == null)
                return NotFound("Product not found");

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
                File = new CloudinaryDotNet.FileDescription(file.FileName, stream),
                Folder = "giftboxy/products"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                return BadRequest("Şəkil yüklənmədi");

            var productImage = new ProductImage
            {
                ProductId = productId,
                ImageUrl = uploadResult.SecureUrl.ToString()
            };

            _context.ProductImages.Add(productImage);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = productImage.Id,
                imageUrl = productImage.ImageUrl
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

            // Cloudinary-dən sil
            var publicId = ExtractPublicId(image.ImageUrl);
            if (!string.IsNullOrEmpty(publicId))
                await _cloudinary.DestroyAsync(new DeletionParams(publicId));

            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Image deleted" });
        }

        private static string ExtractPublicId(string imageUrl)
        {
            // https://res.cloudinary.com/cloud/image/upload/giftboxy/products/filename.jpg
            // → giftboxy/products/filename
            try
            {
                var uri = new Uri(imageUrl);
                var path = uri.AbsolutePath; // /cloud/image/upload/giftboxy/products/filename.jpg
                var uploadIndex = path.IndexOf("/upload/") + "/upload/".Length;
                var withoutVersion = path[uploadIndex..];
                var publicId = Path.ChangeExtension(withoutVersion, null);
                return publicId;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}