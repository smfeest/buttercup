using System;
using Microsoft.Extensions.Logging;

namespace Buttercup.Web.TestUtils
{
    public record LogEntry(
        LogLevel LogLevel, EventId EventId, string Message, object? State, Exception Exception);
}