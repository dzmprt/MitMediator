namespace Books.Application.Abstractions.Infrastructure;

/// <summary>
/// Base entity repository.
/// </summary>
/// <typeparam name="TEntity">Entity type.</typeparam>
public interface IBaseRepository<TEntity> : IBaseProvider<TEntity>
{
    /// <summary>
    /// Add entity.
    /// </summary>
    /// <param name="entity">Entity to add.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
    /// <returns>The added entity.</returns>
    ValueTask<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken);
    
    /// <summary>
    /// Update entity.
    /// </summary>
    /// <param name="entity">Entity to update.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
    /// <returns>The updated entity.</returns>
    ValueTask<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken);

    /// <summary>
    /// Delete entity.
    /// </summary>
    /// <param name="entity">Entity to delete.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
    /// <returns>A task that represents the asynchronous remove operation.</returns>
    ValueTask RemoveAsync(TEntity entity, CancellationToken cancellationToken);
}