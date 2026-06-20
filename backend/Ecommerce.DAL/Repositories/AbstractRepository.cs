using Ecommerce.Contracts.Repositories;
using Ecommerce.DAL.Context;
using Microsoft.EntityFrameworkCore;
namespace  Ecommerce.DAL.Repositories
{
    public class AbstractRepository<K,T> : IRepository<K,T> where T: class
    {
        private readonly AppDbContext _dbContext;
        public AbstractRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<T> Create(T item)
        {
            _dbContext.Set<T>().Add(item);
            await _dbContext.SaveChangesAsync();
            return item;
        }
        public virtual async Task<T?> GetById(K key)
        {
            return await _dbContext.Set<T>().FindAsync(key);
        }
        public virtual async Task<ICollection<T>> GetAll()
        {
            return await _dbContext.Set<T>().ToListAsync();
        }
        public async Task<T?> Update(K key, T item)
        {
            var result = await GetById(key);
            if(result == null)  throw new KeyNotFoundException("Item doesn't exists");
            _dbContext.Set<T>().Update(item);
            await _dbContext.SaveChangesAsync();
            return item;

        }
        public virtual async Task<T?> Delete(K key)
        {
            var result = await GetById(key);
            if(result == null)  throw new KeyNotFoundException("Item doesn't exists");
            _dbContext.Set<T>().Remove(result);
            await _dbContext.SaveChangesAsync();
            return result;
        }
        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}