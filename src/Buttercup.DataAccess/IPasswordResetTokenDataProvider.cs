using Buttercup.EntityModel;

namespace Buttercup.DataAccess;

/// <summary>
/// Defines the contract for the password reset token data provider.
/// </summary>
public interface IPasswordResetTokenDataProvider
{
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
    /// Tries to get the user ID associated with an unexpired password reset token.
    /// </summary>
    /// <param name="dbContext">
    /// The database context.
    /// </param>
    /// <param name="token">
    /// The password reset token.
    /// </param>
    /// <param name="maxAge">
    /// The maximum age of tokens.
    /// </param>
    /// <returns>
    /// A task for the operation. The result is the user ID, or a null reference if no matching
    /// token is found.
    /// </returns>
    Task<long?> GetUserIdForUnexpiredToken(AppDbContext dbContext, string token, TimeSpan maxAge);

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
