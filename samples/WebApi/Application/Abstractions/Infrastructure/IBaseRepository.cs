namespace Application.Abstractions.Infrastructure;

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
    /// <returns>Added entity.</returns>
    ValueTask<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken);
    
    /// <summary>
    /// Update entity.
    /// </summary>
    /// <param name="entity">Entity to update.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
    /// <returns>Updated entity.</returns>
    ValueTask<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken);

    /// <summary>
    /// Delete entity.
    /// </summary>
    /// <param name="entity">Entity to delete.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
    /// <returns><see cref="ValueTask"/></returns>
    ValueTask RemoveAsync(TEntity entity, CancellationToken cancellationToken);
}