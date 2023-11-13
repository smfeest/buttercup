using Buttercup.EntityModel;

namespace Buttercup.DataAccess;

/// <summary>
/// Defines the contract for the user data provider.
/// </summary>
public interface IUserDataProvider
{
    /// <summary>
    /// Tries to find a user by email address.
    /// </summary>
    /// <param name="dbContext">
    /// The database context.
    /// </param>
    /// <param name="email">
    /// The email address.
    /// </param>
    /// <returns>
    /// A task for the operation. The result is the user, or a null reference if no matching
    /// user is found.
    /// </returns>
    Task<User?> FindUserByEmail(AppDbContext dbContext, string email);

    /// <summary>
    /// Gets a user.
    /// </summary>
    /// <param name="dbContext">
    /// The database context.
    /// </param>
    /// <param name="id">
    /// The user ID.
    /// </param>
    /// <returns>
    /// A task for the operation.
    /// </returns>
    /// <exception cref="NotFoundException">
    /// No matching user was found.
    /// </exception>
    Task<User> GetUser(AppDbContext dbContext, long id);

    /// <summary>
    /// Updates the user attributes affected by a password change.
    /// </summary>
    /// <param name="dbContext">
    /// The database context.
    /// </param>
    /// <param name="userId">
    /// The user ID.
    /// </param>
    /// <param name="hashedPassword">
    /// The hashed password.
    /// </param>
    /// <param name="securityStamp">
    /// The new security stamp.
    /// </param>
    /// <returns>
    /// A task for the operation.
    /// </returns>
    /// <exception cref="NotFoundException">
    /// No matching user was found.
    /// </exception>
    Task SaveNewPassword(
        AppDbContext dbContext, long userId, string hashedPassword, string securityStamp);

    /// <summary>
    /// Updates the user attributes affected by the rehashing of an existing password.
    /// </summary>
    /// <param name="dbContext">
    /// The database context.
    /// </param>
    /// <param name="userId">
    /// The user ID.
    /// </param>
    /// <param name="baseRevision">
    /// The base revision. Used for concurrency checking.
    /// </param>
    /// <param name="rehashedPassword">
    /// The rehashed password.
    /// </param>
    /// <param name="timestamp">
    /// The timestamp for the operation.
    /// </param>
    /// <returns>
    /// A task for the operation. The result is <b>true</b> if the user was updated, <b>false</b> if
    /// the user does not exist or <paramref name="baseRevision"/> does not match the revision
    /// currently in the database.
    /// </returns>
    Task<bool> SaveRehashedPassword(
        AppDbContext dbContext,
        long userId,
        int baseRevision,
        string rehashedPassword,
        DateTime timestamp);

    /// <summary>
    /// Sets a user's time zone.
    /// </summary>
    /// <param name="dbContext">
    /// The database context.
    /// </param>
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
    Task SetTimeZone(AppDbContext dbContext, long userId, string timeZone);
}
