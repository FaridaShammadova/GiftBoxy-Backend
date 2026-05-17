using GiftBoxy.Application.DTOs.Product;
using GiftBoxy.Application.Services.Interfaces;
using GiftBoxy.Domain.Entities;
using GiftBoxy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GiftBoxy.Application.Services.Implementations
{
    public class ProductService : GenericService<Product>, IProductService
    {
        public ProductService(AppDbContext context)
            : base(context)
        {
        }

        public async Task<Product> CreateProductAsync(ProductCreateDto dto, string userId)
        {
            var sellerProfile = await _context.SellerProfiles
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (sellerProfile is null)
                throw new Exception("Seller profile tapılmadı");

            var product = new Product
            {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                StockCount = dto.StockCount,
                IsPersonalized = dto.IsPersonalized,
                SellerProfileId = sellerProfile.Id
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return product;
        }
    }
}
