using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Sdk;

namespace Buttercup.TestUtils;

/// <summary>
/// Contains static methods that can be used to verify logging.
/// </summary>
public static class LogAssert
{
    /// <summary>
    /// Verifies that a list logger has an entry with the specified log level and message.
    /// </summary>
    /// <typeparam name="T">The type whose name is used for the logger category name.</typeparam>
    /// <param name="listLogger">The list logger.</param>
    /// <param name="logLevel">The expected log level.</param>
    /// <param name="eventId">The expected event ID.</param>
    /// <param name="message">The expected message.</param>
    /// <param name="exception">The expected exception.</param>
    /// <exception cref="ContainsException">When no matching entry is found.</exception>
    public static void HasEntry<T>(
        ListLogger<T> listLogger,
        LogLevel logLevel,
        EventId eventId,
        string message,
        Exception? exception = null) =>
        Assert.Contains(new(logLevel, eventId, message, exception), listLogger.Entries);
}
