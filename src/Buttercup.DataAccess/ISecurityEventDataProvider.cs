using Buttercup.EntityModel;

namespace Buttercup.DataAccess;

/// <summary>
/// Defines the contract for the security event data provider.
/// </summary>
public interface ISecurityEventDataProvider
{
    /// <summary>
    /// Logs a security event.
    /// </summary>
    /// <param name="dbContext">
    /// The database context.
    /// </param>
    /// <param name="eventName">
    /// The event name.
    /// </param>
    /// <param name="userId">
    /// The user ID, if applicable.
    /// </param>
    /// <param name="email">
    /// The email address, if applicable.
    /// </param>
    /// <returns>
    /// A task for the operation. The result is the event ID.
    /// </returns>
    Task<long> LogEvent(
        AppDbContext dbContext, string eventName, long? userId = null, string? email = null);
}
