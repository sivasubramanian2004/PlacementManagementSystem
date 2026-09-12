
using Microsoft.EntityFrameworkCore;
//using EMS.Core.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace PMS.Data.Repositories;

public class Repository<T>(AppDbContext context) : IRepository<T> where T : class
{

    private readonly AppDbContext _context = context;
    private DbSet<T> _entities = null!;

    /// <summary>
    /// Gets the entities set.
    /// </summary>
    protected virtual DbSet<T> Entities
    {
        get
        {
            if (_entities == null)
                _entities = _context.Set<T>();

            return _entities;
        }
    }

    /// <summary>
    /// Gets an entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns>The entity.</returns>
    public virtual T GetById(object id)
    {
        return this.Entities.Find(id)!;
    }

    /// <summary>
    // /// Gets an entity by its identifier asynchronously.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns>The entity.</returns>
    public async Task<T?> GetByIdAsync(object id)
    {
        return await this.Entities.FindAsync(id);
    }

    /// <summary>
    /// Inserts a new entity.
    /// </summary>
    /// <param name="entity">The entity to insert.</param>
    /// <returns>The inserted entity.</returns>
    public virtual T Insert(T entity)
    {
        this.Entities.Add(entity);
        this._context.SaveChanges();

        return entity;
    }

    /// <summary>
    /// Inserts a new entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to insert.</param>
    /// <returns>The inserted entity.</returns>
    public virtual async Task<T> InsertAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await this.Entities.AddAsync(entity);
        await this._context.SaveChangesAsync();

        return entity;
    }

    /// <summary>
    /// Inserts multiple entities.
    /// </summary>
    /// <param name="entities">The entities to insert.</param>
    /// <returns>The inserted entities.</returns>
    public virtual IEnumerable<T> Insert(IEnumerable<T> entities)
    {
        return this.InsertAsync(entities).Result;
    }

    /// <summary>
    /// Inserts multiple entities asynchronously.
    /// </summary>
    /// <param name="entities">The entities to insert.</param>
    /// <returns>The inserted entities.</returns>
    public virtual async Task<IEnumerable<T>> InsertAsync(IEnumerable<T> entities)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        try
        {
            var validationResults = new List<ValidationResult>();
            foreach (var entity in entities)
            {
                if (!Validator.TryValidateObject(entity, new ValidationContext(entity), validationResults))
                {
                    // Handle validation failure
                }
            }

            await this.Entities.AddRangeAsync(entities);
            await this._context.SaveChangesAsync();
            return entities;
        }
        catch (DbUpdateException)
        {
            throw;
        }
    }

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>The updated entity.</returns>
    public virtual T Update(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var entry = _context.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            this.Entities.Update(entity);
        }

        this._context.SaveChanges();
        return entity;
    }

    /// <summary>
    /// Updates an existing entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>The updated entity.</returns>
    public virtual async Task<T> UpdateAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var primaryKey = _context.Model.FindEntityType(typeof(T))
            .FindPrimaryKey()
            .Properties
            .Select(x => x.Name)
            .Single();

        var keyValue = typeof(T).GetProperty(primaryKey)?.GetValue(entity);

        var existingEntity = await Entities.FindAsync(keyValue);

        if (existingEntity != null)
        {
            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
        }
        else
        {
            Entities.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        await _context.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Deletes an entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier.</param>
    public virtual void Delete(object id)
    {
        var entity = GetById(id);

        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        this.Entities.Remove(entity);
        this._context.SaveChanges();
    }

    /// <summary>
    /// Deletes an entity by its identifier asynchronously.
    /// </summary>
    /// <param name="id">The identifier.</param>
    public virtual async Task DeleteAsync(object id)
    {
        var entity = await GetByIdAsync(id);

        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        this.Entities.Remove(entity);
        await this._context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes multiple entities.
    /// </summary>
    /// <param name="entities">The entities to delete.</param>
    public virtual void Delete(IEnumerable<T> entities)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        this.Entities.RemoveRange(entities);
        this._context.SaveChanges();
    }

    /// <summary>
    /// Deletes multiple entities asynchronously.
    /// </summary>
    /// <param name="entities">The entities to delete.</param>
    public virtual async Task DeleteAsync(IEnumerable<T> entities)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        this.Entities.RemoveRange(entities);
        await this._context.SaveChangesAsync();
    }

    /// <summary>
    /// Saves changes to the context.
    /// </summary>
    public virtual void Save()
    {
        this._context.SaveChanges();
    }

    /// <summary>
    /// Saves changes to the context asynchronously.
    /// </summary>
    public virtual async Task SaveAsync()
    {
        await this._context.SaveChangesAsync();
    }



    /// <summary>
    /// Gets a queryable table.
    /// </summary>
    public virtual IQueryable<T> Table
    {
        get
        {
            return this.Entities;
        }
    }

    /// <summary>
    /// Gets a queryable table with "no tracking" enabled (EF feature). Use it only for read-only operations.
    /// </summary>
    public virtual IQueryable<T> TableNoTracking
    {
        get
        {
            return this.Entities.AsNoTracking();
        }
    }

    public async Task UpdateRange(IEnumerable<T> entities)
    {
        _context.Set<T>().UpdateRange(entities);
        await _context.SaveChangesAsync();
    }


    #region Unit Of Work Support

    public virtual async Task AddAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await Entities.AddAsync(entity);
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        await Entities.AddRangeAsync(entities);
    }

    public virtual void UpdateEntity(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        Entities.Update(entity);
    }

    public virtual void UpdateRangeEntity(IEnumerable<T> entities)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        Entities.UpdateRange(entities);
    }

    public virtual void DeleteEntity(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        Entities.Remove(entity);
    }

    public virtual void DeleteRangeEntity(IEnumerable<T> entities)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        Entities.RemoveRange(entities);
    }

    #endregion

















    /// <summary>
    /// Retrieves filtered data asynchronously based on the provided query parameters.
    /// </summary>
    /// <typeparam name="TItem">The type of the entity.</typeparam>
    /// <param name="source">The source queryable.</param>
    /// <param name="parameters">The query parameters.</param>
    /// <returns>A paged result containing the filtered data.</returns>
    /// 
    /*
    public async Task<PagedResult<TItem>> GetFilteredDataAsync<TItem>(IQueryable<TItem> source, QueryParameters parameters)
    {
        var filterBy = source.ApplyFilters(parameters.Filters);

        // Apply OrFilters if present
        if (parameters.OrFilters != null && parameters.OrFilters.Count > 0)
        {
            filterBy = filterBy.ApplyOrFilters(parameters.OrFilters);
        }


        var totalItems = await filterBy.CountAsync();

        var filteredQuery = filterBy.ApplyQueryParameters(parameters);

        var items = await filteredQuery.ToListAsync();

        return new PagedResult<TItem>
        {
            Data = items,
            TotalRecords = totalItems,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
    }
    */

}


