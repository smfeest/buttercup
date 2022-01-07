using MySqlConnector;

namespace Buttercup.DataAccess;

/// <summary>
/// Defines the contract for the authentication event data provider.
/// </summary>
public interface IAuthenticationEventDataProvider
{
    /// <summary>
    /// Logs an authentication event.
    /// </summary>
    /// <param name="connection">
    /// The database connection.
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
        MySqlConnection connection,
        string eventName,
        long? userId = null,
        string? email = null);
}
