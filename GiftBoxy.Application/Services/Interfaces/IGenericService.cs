using GiftBoxy.Domain.Entities;

namespace GiftBoxy.Application.Services.Interfaces
{
    public interface IGenericService<T> where T : BaseEntity
    {
        Task<List<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task CreateAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
    }
}
