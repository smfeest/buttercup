using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Xunit;
using Xunit.Sdk;

namespace Buttercup.TestUtils;

/// <summary>
/// Provides methods to verify a <see cref="FakeLogRecord"/>.
/// </summary>
/// <param name="record">The record.</param>
public class FakeLogRecordAssert(FakeLogRecord record)
{
    /// <summary>
    /// The record.
    /// </summary>
    public FakeLogRecord Record { get; } = record;

    /// <summary>
    /// Asserts that the record has the expected exception.
    /// </summary>
    /// <param name="exception">The expected exception.</param>
    /// <returns>The object, for chaining.</returns>
    /// <exception cref="EqualException">
    /// The record does not have the expected exception.
    /// </exception>
    public FakeLogRecordAssert HasException(Exception exception)
    {
        Assert.Equal(exception, this.Record.Exception);
        return this;
    }

    /// <summary>
    /// Asserts that the record has the expected event ID.
    /// </summary>
    /// <param name="id">The expected event ID.</param>
    /// <returns>The object, for chaining.</returns>
    /// <exception cref="EqualException">The record does not have the expected event ID.</exception>
    public FakeLogRecordAssert HasId(EventId id)
    {
        Assert.Equal(id, this.Record.Id);
        return this;
    }

    /// <summary>
    /// Asserts that the record has the expected level.
    /// </summary>
    /// <param name="level">The expected level.</param>
    /// <returns>The object, for chaining.</returns>
    /// <exception cref="EqualException">The record does not have the expected level.</exception>
    public FakeLogRecordAssert HasLevel(LogLevel level)
    {
        Assert.Equal(level, this.Record.Level);
        return this;
    }

    /// <summary>
    /// Asserts that the record has the expected message.
    /// </summary>
    /// <param name="message">The expected message.</param>
    /// <returns>The object, for chaining.</returns>
    /// <exception cref="EqualException">The record does not have the expected message.</exception>
    public FakeLogRecordAssert HasMessage(string message)
    {
        Assert.Equal(message, this.Record.Message);
        return this;
    }
}
