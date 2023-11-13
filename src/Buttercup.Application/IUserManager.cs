using Buttercup.EntityModel;

namespace Buttercup.Application;

/// <summary>
/// Defines the contract for the user manager.
/// </summary>
public interface IUserManager
{
    /// <summary>
    /// Gets a user.
    /// </summary>
    /// <param name="id">
    /// The user ID.
    /// </param>
    /// <returns>
    /// A task for the operation.
    /// </returns>
    /// <exception cref="NotFoundException">
    /// No matching user was found.
    /// </exception>
    Task<User> GetUser(long id);

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
