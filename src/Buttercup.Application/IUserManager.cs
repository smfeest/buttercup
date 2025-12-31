using System.Net;
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
    /// <param name="currentUserId">
    /// The current user ID.
    /// </param>
    /// <param name="ipAddress">
    /// The IP address of the current user.
    /// </param>
    /// <returns>
    /// A task for the operation. The task result is the ID of the new user.
    /// </returns>
    Task<long> CreateUser(NewUserAttributes attributes, long currentUserId, IPAddress? ipAddress);

    /// <summary>
    /// Creates a new user for automated end-to-end testing.
    /// </summary>
    /// <remarks>
    /// This method creates a new user with a unique email address and randomly generated password.
    /// It should only be used to set up users for automated end-to-end testing in development.
    /// </remarks>
    /// <returns>
    /// A task for the operation. The task result is the ID and password of the new user.
    /// </returns>
    Task<(long Id, string Password)> CreateTestUser();

    /// <summary>
    /// Deactivates a user.
    /// </summary>
    /// <param name="id">
    /// The user ID.
    /// </param>
    /// <param name="currentUserId">
    /// The current user ID.
    /// </param>
    /// <param name="ipAddress">
    /// The IP address of the current user.
    /// </param>
    /// <returns>
    /// A task for the operation. The task result is <b>true</b> if the user was deactivated, or
    /// <b>false</b> if the user is already deactivated.
    /// </returns>
    /// <exception cref="NotFoundException">
    /// No matching user was found.
    /// </exception>
    Task<bool> DeactivateUser(long id, long currentUserId, IPAddress? ipAddress);

    /// <summary>
    /// Reactivates a user.
    /// </summary>
    /// <param name="id">
    /// The user ID.
    /// </param>
    /// <param name="currentUserId">
    /// The current user ID.
    /// </param>
    /// <param name="ipAddress">
    /// The IP address of the current user.
    /// </param>
    /// <returns>
    /// A task for the operation. The task result is <b>true</b> if the user was reactivated, or
    /// <b>false</b> if the user is already active.
    /// </returns>
    /// <exception cref="NotFoundException">
    /// No matching user was found.
    /// </exception>
    Task<bool> ReactivateUser(long id, long currentUserId, IPAddress? ipAddress);

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
    /// Hard-deletes a user and associated records.
    /// </summary>
    /// <remarks>
    /// This method should only be used to clean-up users that have been specifically created for
    /// testing in development.
    /// </remarks>
    /// <param name="id">
    /// The user ID.
    /// </param>
    /// <returns>
    /// A task for the operation. The task result is <b>true</b> on success, <b>false</b> if the
    /// user does not exist.
    /// </returns>
    Task<bool> HardDeleteTestUser(long id);

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
