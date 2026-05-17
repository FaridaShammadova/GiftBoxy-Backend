using GiftBoxy.Application.DTOs.Product;
using GiftBoxy.Domain.Entities;

namespace GiftBoxy.Application.Services.Interfaces
{
    public interface IProductService : IGenericService<Product>
    {
        Task<Product> CreateProductAsync(ProductCreateDto dto, string userId);

        //Task<List<Product>> GetFeaturedAsync();
        //Task<List<Product>> GetBestSellersAsync();
        //Task<Product> GetBySlugAsync(string slug);
    }
}
