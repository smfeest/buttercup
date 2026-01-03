using Buttercup.EntityModel;

namespace Buttercup.Web.Areas.Admin.Controllers.Queries;

/// <summary>
/// Provides database queries for <see cref="UsersController"/>.
/// </summary>
public interface IUsersControllerQueries
{
    /// <summary>
    /// Gets all users ordered by name and email.
    /// </summary>
    /// <param name="dbContext">
    /// The database context.
    /// </param>
    /// <returns>
    /// A task for the operation.
    /// </returns>
    Task<User[]> GetUsersForIndex(AppDbContext dbContext);
}
