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
    /// Updates a user's password.
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
    Task UpdatePassword(
        AppDbContext dbContext, long userId, string hashedPassword, string securityStamp);

    /// <summary>
    /// Updates a user's preferences.
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
    Task UpdatePreferences(AppDbContext dbContext, long userId, string timeZone);
}
