using System.Linq.Expressions;

namespace Books.Application.Abstractions.Infrastructure;

/// <summary>
/// Base provider.
/// </summary>
/// <typeparam name="TEntity">Entity type.</typeparam>
public interface IBaseProvider<TEntity>
{
    /// <summary>
    /// Get entity by key.
    /// </summary>
    /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
    /// <returns>The first entity matching the predicate, or null if not found.</returns>
    ValueTask<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>>? predicate, CancellationToken cancellationToken);
    
    /// <summary>
    /// Search entities.
    /// </summary>
    /// <param name="predicate">Filter predicate.</param>
    /// <param name="limit">Limit.</param>
    /// <param name="offset">Offset.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
    /// <returns>An array of entities matching the search criteria.</returns>
    ValueTask<TEntity[]> SearchAsync<TKey>(Expression<Func<TEntity, bool>>? predicate, Expression<Func<TEntity, TKey>>? orderBy, int? limit, int? offset, CancellationToken cancellationToken);
    
    /// <summary>
    /// Get all entities.
    /// </summary>
    /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
    /// <returns>An array of all entities.</returns>
    ValueTask<TEntity[]> GetAllAsync( CancellationToken cancellationToken);
    
    /// <summary>
    /// Any entity exists by predicate.
    /// </summary>
    /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
    /// <returns>True if any entity matches the predicate; otherwise, false.</returns>
    ValueTask<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate, CancellationToken cancellationToken);
}