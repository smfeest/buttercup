using Buttercup.EntityModel;

namespace Buttercup.DataAccess;

/// <summary>
/// Defines the contract for the password reset token data provider.
/// </summary>
public interface IPasswordResetTokenDataProvider
{
    /// <summary>
    /// Deletes all password reset tokens that were created before a specific date and time.
    /// </summary>
    /// <param name="dbContext">
    /// The database context.
    /// </param>
    /// <param name="cutOff">
    /// The cut off date and time.
    /// </param>
    /// <returns>
    /// A task for the operation.
    /// </returns>
    Task DeleteExpiredTokens(AppDbContext dbContext, DateTime cutOff);

    /// <summary>
    /// Deletes all password reset tokens belonging to a user.
    /// </summary>
    /// <param name="dbContext">
    /// The database context.
    /// </param>
    /// <param name="userId">
    /// The user ID.
    /// </param>
    /// <returns>
    /// A task for the operation.
    /// </returns>
    Task DeleteTokensForUser(AppDbContext dbContext, long userId);

    /// <summary>
    /// Tries to get the user ID associated with a password reset token.
    /// </summary>
    /// <param name="dbContext">
    /// The database context.
    /// </param>
    /// <param name="token">
    /// The password reset token.
    /// </param>
    /// <returns>
    /// A task for the operation. The result is the user ID, or a null reference if no matching
    /// token is found.
    /// </returns>
    Task<long?> GetUserIdForToken(AppDbContext dbContext, string token);

    /// <summary>
    /// Inserts a password reset token.
    /// </summary>
    /// <param name="dbContext">
    /// The database context.
    /// </param>
    /// <param name="userId">
    /// The user ID.
    /// </param>
    /// <param name="token">
    /// The token.
    /// </param>
    /// <returns>
    /// A task for the operation.
    /// </returns>
    Task InsertToken(AppDbContext dbContext, long userId, string token);
}
