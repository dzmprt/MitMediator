using System.Linq.Expressions;
using Application.Abstractions.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

/// <summary>
/// Base entity repository.
/// </summary>
public class BaseRepository<TEntity> : BaseProvider<TEntity>, IBaseRepository<TEntity> where TEntity : class
{
    
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="dbContext"><see cref="DbContext"/>.</param>
    public BaseRepository(DbContext dbContext) : base(dbContext)
    {
    }

    /// <inheritdoc/>
    public async ValueTask<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        DbSet.Add(entity);
        await DbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    /// <inheritdoc/>
    public async ValueTask<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        DbSet.Update(entity);
        await DbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    /// <inheritdoc/>
    public async ValueTask RemoveAsync(TEntity entity, CancellationToken cancellationToken)
    {
        DbSet.Remove(entity);
        await DbContext.SaveChangesAsync(cancellationToken);
    }
}