using Buttercup.EntityModel;

namespace Buttercup.Application;

/// <summary>
/// Defines the contract for the user manager.
/// </summary>
public interface IUserManager
{
    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="attributes">
    /// The user attributes.
    /// </param>
    /// <returns>
    /// A task for the operation. The task result is the ID of the new user.
    /// </returns>
    Task<long> CreateUser(NewUserAttributes attributes);

    /// <summary>
    /// Finds a user.
    /// </summary>
    /// <param name="id">
    /// The user ID.
    /// </param>
    /// <returns>
    /// A task for the operation. The result is the user, or null if the user does not exist.
    /// </returns>
    Task<User?> FindUser(long id);

    /// <summary>
    /// Sets a user's time zone.
    /// </summary>
    /// <param name="userId">
    /// The user ID.
    /// </param>
    /// <param name="timeZone">
    /// The TZ ID of the time zone.
    /// </param>
    /// <returns>
    /// A task for the operation.
    /// </returns>
    /// <exception cref="NotFoundException">
    /// No matching user was found.
    /// </exception>
    Task SetTimeZone(long userId, string timeZone);
}
