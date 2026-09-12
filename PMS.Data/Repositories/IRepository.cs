
using Microsoft.EntityFrameworkCore;
using System.Linq;
namespace PMS.Data.Repositories
{
    public interface IRepository<T> where T : class
    {
        // Get
        T GetById(object id);
        Task<T?> GetByIdAsync(object id);

        // Insert
        T Insert(T entity);
        Task<T> InsertAsync(T entity);

        IEnumerable<T> Insert(IEnumerable<T> entities);
        Task<IEnumerable<T>> InsertAsync(IEnumerable<T> entities);

        // Update
        T Update(T entity);
        Task<T> UpdateAsync(T entity);

        Task UpdateRange(IEnumerable<T> entities);

        // Delete
        void Delete(object id);
        Task DeleteAsync(object id);

        void Delete(IEnumerable<T> entities);
        Task DeleteAsync(IEnumerable<T> entities);

        // Save
        void Save();
        Task SaveAsync();

        // Query
        IQueryable<T> Table { get; }

        IQueryable<T> TableNoTracking { get; }



        // New Methods for UnitOfWork
        // ===========================

        Task AddAsync(T entity);

        Task AddRangeAsync(IEnumerable<T> entities);

        void UpdateEntity(T entity);

        void UpdateRangeEntity(IEnumerable<T> entities);

        void DeleteEntity(T entity);

        void DeleteRangeEntity(IEnumerable<T> entities);
    }
}