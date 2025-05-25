using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Xunit;
using Xunit.Sdk;

namespace Buttercup.TestUtils;

/// <summary>
/// Contains static methods that can be used to verify logging.
/// </summary>
public static class LogAssert
{
    /// <summary>
    /// Verifies that a fake logger has an entry with the specified log level, event ID, message and
    /// exception.
    /// </summary>
    /// <typeparam name="T">The type whose name is used for the logger category name.</typeparam>
    /// <param name="fakeLogger">The fake logger.</param>
    /// <param name="logLevel">The expected log level.</param>
    /// <param name="eventId">The expected event ID.</param>
    /// <param name="message">The expected message.</param>
    /// <param name="exception">The expected exception.</param>
    /// <exception cref="ContainsException">When no matching entry is found.</exception>
    public static void HasEntry<T>(
        FakeLogger<T> fakeLogger,
        LogLevel logLevel,
        EventId eventId,
        string message,
        Exception? exception = null) =>
        Assert.Contains(
            fakeLogger.Collector.GetSnapshot(),
            record => record.Level == logLevel &&
                record.Id == eventId &&
                record.Message == message &&
                record.Exception == exception);
}
