using System.Linq.Expressions;
using Application.Abstractions.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class BaseRepository<TEntity> : BaseProvider<TEntity>, IBaseRepository<TEntity> where TEntity : class
{
    
    public BaseRepository(DbContext dbContext) : base(dbContext)
    {
    }

    public async ValueTask<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        DbSet.Add(entity);
        await DbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async ValueTask<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        DbSet.Update(entity);
        await DbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async ValueTask RemoveAsync(TEntity entity, CancellationToken cancellationToken)
    {
        DbSet.Remove(entity);
        await DbContext.SaveChangesAsync(cancellationToken);
    }
}