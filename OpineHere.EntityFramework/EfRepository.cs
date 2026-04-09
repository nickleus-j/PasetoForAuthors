using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Linq.Dynamic.Core;
using OpineHere.Data;

namespace OpineHere.EntityFramework;

public class EfRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly DbContext Context;

    public EfRepository(DbContext context)
    {
        Context = context;
    }
    public async Task AddAsync(TEntity entity)
    {
        await Context.Set<TEntity>().AddAsync(entity);
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
        await Context.Set<TEntity>().AddRangeAsync(entities);
    }

    public async Task<IList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await Context.Set<TEntity>().Where(predicate).ToListAsync();
    }

    public async Task<IList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, int page, int pageSize=10)
    {
        return await Context.Set<TEntity>().Where(predicate).Skip((page-1)*pageSize).Take(pageSize).ToListAsync();
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await Context.Set<TEntity>().ToListAsync();
    }

    public async Task<TEntity> GetAsync(int id)
    {
        return await Context.Set<TEntity>().FindAsync(id);
    }
    /// <summary>
    /// Get Content divided into pages.
    /// </summary>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <param name="orderByColumn"></param>
    /// <param name="sortOrder"></param>
    /// <returns></returns>
    public async Task<IEnumerable<TEntity>> GetFromPageAsync(int page, int pageSize, string orderByColumn, string sortOrder = "asc")
    {
        // Simple validation
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10; // Default size

        var totalCount = await Context.Set<TEntity>().CountAsync();
        IQueryable<TEntity> source;
        if (!string.IsNullOrEmpty(orderByColumn))
        {
            var orderString = $"{orderByColumn} {sortOrder}";
            source = Context.Set<TEntity>().OrderBy<TEntity>(orderString);
        }
        else
        {
            // Fallback to a default order if none is specified, EF requires an OrderBy
            // Here is a default property, e.g., "Id"
            source = Context.Set<TEntity>().OrderBy("Id asc");
        }

        return await source.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    }

    public void Remove(TEntity entity)
    {
        Context.Set<TEntity>().Remove(entity);
    }

    public void RemoveRange(IEnumerable<TEntity> entities)
    {
        Context.Set<TEntity>().RemoveRange(entities);
    }

    public async Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await Context.Set<TEntity>().SingleAsync(predicate);
    }
    public TEntity Update(TEntity entity)
    {
        return Context.Update(entity).Entity;
    }
}