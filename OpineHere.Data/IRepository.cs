using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
namespace OpineHere.Data;

public interface IRepository<TEntity> where TEntity : class
{
    Task<TEntity> GetAsync(int id);
    Task<IEnumerable<TEntity>> GetFromPageAsync(int page, int pageSize,string orderByColumn,string sortOrder = "asc");
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<IList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
    Task<IList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate,int page,int pageSize=10);
    Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> predicate);
    Task AddAsync(TEntity entity);
    Task AddRangeAsync(IEnumerable<TEntity> entities);
    TEntity Update(TEntity entity);
    void Remove(TEntity entity);
    void RemoveRange(IEnumerable<TEntity> entities);
}