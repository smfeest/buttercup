using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Xunit;
using Xunit.Sdk;

namespace Buttercup.TestUtils;

/// <summary>
/// Contains static methods that can be used to verify the events logged to a <see
/// cref="FakeLogger"/>.
/// </summary>
public static class LogAssert
{
    /// <summary>
    /// Verifies that the collector associated with a fake logger contains exactly one record.
    /// </summary>
    /// <param name="logger">The fake logger.</param>
    /// <returns>
    /// An instance of <see cref="FakeLogRecordAssert"/> that can be used to make assertions about
    /// the record.
    /// </returns>
    /// <exception cref="SingleException">
    /// When the collector associated with the fake logger does not contain exactly one record with
    /// the specified ID.
    /// </exception>
    public static FakeLogRecordAssert SingleEntry(FakeLogger logger)
    {
        var record = Assert.Single(logger.Collector.GetSnapshot());
        return new(record);
    }

    /// <summary>
    /// Verifies that the collector associated with a fake logger contains exactly one record with a
    /// particular event ID.
    /// </summary>
    /// <param name="logger">The fake logger.</param>
    /// <param name="id">The event ID</param>
    /// <returns>
    /// An instance of <see cref="FakeLogRecordAssert"/> that can be used to make assertions about
    /// the record.
    /// </returns>
    /// <exception cref="SingleException">
    /// When the collector associated with the fake logger does not contain exactly one record with
    /// the specified ID.
    /// </exception>
    public static FakeLogRecordAssert SingleEntry(FakeLogger logger, EventId id)
    {
        var record = Assert.Single(logger.Collector.GetSnapshot(), r => r.Id == id);
        return new(record);
    }
}
