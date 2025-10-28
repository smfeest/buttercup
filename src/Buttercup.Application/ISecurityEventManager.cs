using System.Net;

namespace Buttercup.Application;

/// <summary>
/// Defines the contract for the security event manager.
/// </summary>
public interface ISecurityEventManager
{
    /// <summary>
    /// Creates a new security event.
    /// </summary>
    /// <param name="eventName">
    /// The event name.
    /// </param>
    /// <param name="ipAddress">
    /// The client IP address, if available.
    /// </param>
    /// <param name="userId">
    /// The user ID, if applicable
    /// </param>
    /// <returns>
    /// A task for the operation. The task result is the ID of the new security event.
    /// </returns>
    Task<long> CreateSecurityEvent(string eventName, IPAddress? ipAddress, long? userId);

    /// <summary>
    /// Creates a new security event with a specific timestamp.
    /// </summary>
    /// <param name="time">
    /// The timestamp.
    /// </param>
    /// <param name="eventName">
    /// The event name.
    /// </param>
    /// <param name="ipAddress">
    /// The client IP address, if available.
    /// </param>
    /// <param name="userId">
    /// The user ID, if applicable
    /// </param>
    /// <returns>
    /// A task for the operation. The task result is the ID of the new security event.
    /// </returns>
    Task<long> CreateSecurityEvent(
        DateTime time, string eventName, IPAddress? ipAddress, long? userId);
}
