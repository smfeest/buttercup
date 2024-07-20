using Microsoft.EntityFrameworkCore;

namespace Buttercup.EntityModel;

/// <summary>
/// Provides extension methods for <see cref="IQueryable{T}" />.
/// </summary>
public static class QueryableExtensions
{
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

    /// <summary>
    /// Filters a queryable source to exclude soft-deleted entities.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="source">The queryable source.</param>
    /// <returns>
    /// A queryable that excludes elements from <paramref name="source"/> that have been
    /// soft-deleted.
    /// </returns>
    public static IQueryable<T> WhereNotSoftDeleted<T>(this IQueryable<T> source)
        where T : ISoftDeletable =>
        source.Where(x => !x.Deleted.HasValue);

    /// <summary>
    /// Filters a queryable source to only include soft-deleted entities.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="source">The queryable source.</param>
    /// <returns>
    /// A queryable that only includes elements from <paramref name="source"/> that have been
    /// soft-deleted.
    /// </returns>
    public static IQueryable<T> WhereSoftDeleted<T>(this IQueryable<T> source)
        where T : ISoftDeletable =>
        source.Where(x => x.Deleted.HasValue);
}
