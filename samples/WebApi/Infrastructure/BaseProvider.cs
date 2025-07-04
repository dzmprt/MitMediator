using System.Linq.Expressions;
using Application.Abstractions.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

/// <summary>
/// Base provider.
/// </summary>
public class BaseProvider<TEntity> : IBaseProvider<TEntity> where TEntity : class
{
    protected readonly DbContext DbContext;

    protected readonly DbSet<TEntity> DbSet;
    
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="dbContext"><see cref="DbContext"/>.</param>
    public BaseProvider(DbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = DbContext.Set<TEntity>();
    }
    
    /// <inheritdoc/>
    public async ValueTask<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>>? predicate, CancellationToken cancellationToken)
    {
        if (predicate is null)
        {
            return await DbSet.FirstOrDefaultAsync(cancellationToken: cancellationToken);
        }
        
        return await DbSet.FirstOrDefaultAsync(predicate, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async ValueTask<TEntity[]> SearchAsync<TKey>(Expression<Func<TEntity, bool>>? predicate, Expression<Func<TEntity, TKey>>? orderBy, int? limit, int? offset, CancellationToken cancellationToken)
    {
        var query = DbSet.AsQueryable();
        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        if (orderBy is not null)
        {
            query = query.OrderBy(orderBy);
        }

        if (offset is not null)
        {
            query = query.Skip(offset.Value);
        }
        
        if (limit is not null)
        {
            query = query.Take(limit.Value);
        }

        
        return await query.ToArrayAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async ValueTask<TEntity[]> GetAllAsync(CancellationToken cancellationToken)
    {
        return await DbSet.ToArrayAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async ValueTask<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate, CancellationToken cancellationToken)
    {
        if (predicate is null)
        {
            return await DbSet.AnyAsync(cancellationToken: cancellationToken);
        }
        return await DbSet.AnyAsync(predicate, cancellationToken);
    }
}