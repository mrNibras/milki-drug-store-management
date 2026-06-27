using MilkiDrugStore.Domain.Entities;
using System.Linq;

namespace MilkiDrugStore.Domain.Interfaces.Repositories;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IQueryable<T>> GetAllAsync();
    Task<IQueryable<T>> FindAsync(System.Linq.Expressions.Expression<System.Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task DeleteRangeAsync(IEnumerable<T> entities);
    Task<bool> ExistsAsync(int id);
}
