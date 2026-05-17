using GiftBoxy.Application.Services.Interfaces;
using GiftBoxy.Domain.Entities;
using GiftBoxy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GiftBoxy.Application.Services.Implementations
{
    public class GenericService<T> : IGenericService<T> where T : BaseEntity
    {
        protected readonly AppDbContext _context;
        private readonly DbSet<T> _table;

        public GenericService(AppDbContext context)
        {
            _context = context;
            _table = _context.Set<T>();
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _table.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _table.FindAsync(id);
        }

        public async Task CreateAsync(T entity)
        {
            await _table.AddAsync(entity);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _table.Update(entity);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _table.FindAsync(id);

            if (entity == null)
                throw new Exception("Entity not found");

            _table.Remove(entity);

            await _context.SaveChangesAsync();
        }
    }
}
