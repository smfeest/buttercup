using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.EntityModel;

/// <summary>
/// Provides extension methods for <see cref="IQueryable{T}" />.
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Conditionally includes related data in a query.
    /// </summary>
    /// <typeparam name="TEntity">
    /// The type of entity being queried.
    /// </typeparam>
    /// <typeparam name="TProperty">
    /// The type of the related entity to be included.
    /// </typeparam>
    /// <param name="source">
    /// The source query.
    /// </param>
    /// <param name="navigationPropertyPath"></param>
    /// <param name="include">
    /// <b>true</b> if the related data identified by <paramref name="navigationPropertyPath"/> is
    /// to be included, <b>false</b> otherwise.
    /// </param>
    /// <returns>
    /// <paramref name="source"/> if <paramref name="include"/> is false, otherwise a new query with
    /// the related data included.
    /// </returns>
    public static IQueryable<TEntity> IncludeIf<TEntity, TProperty>(
        this IQueryable<TEntity> source,
        Expression<Func<TEntity, TProperty>> navigationPropertyPath,
        bool include) where TEntity : class =>
        include ? source.Include(navigationPropertyPath) : source;

    /// <summary>
    /// Finds an entity by ID.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="GetAsync{T}" />, this method returns null if no matching entity is found.
    /// </remarks>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="source">The queryable source.</param>
    /// <param name="id">The entity ID.</param>
    /// <returns>
    /// A task for the operation. The result is the entity, or a null reference if no matching
    /// entity is found.
    /// </returns>
    public static Task<T?> FindAsync<T>(this IQueryable<T> source, long id) where T : IEntityId =>
        source.Where(x => x.Id == id).FirstOrDefaultAsync();

    /// <summary>
    /// Gets an entity by ID.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="FindAsync{T}" />, this method throws an exception if no matching entity is
    /// found.
    /// </remarks>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="source">The queryable source.</param>
    /// <param name="id">The entity ID.</param>
    /// <exception cref="NotFoundException">No matching entity was found.</exception>
    /// <returns>
    /// A task for the operation. The result is the entity.
    /// </returns>
    public static async Task<T> GetAsync<T>(this IQueryable<T> source, long id)
        where T : IEntityId =>
        await source.FindAsync(id) ??
            throw new NotFoundException($"{typeof(T).Name}/{id} not found");
}
