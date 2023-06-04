using Xunit;

namespace Buttercup.Web.TestUtils;

public sealed class ListLogger<T> : ILogger<T>
{
    private readonly List<LogEntry> entries = new();

    public IReadOnlyList<LogEntry> Entries => this.entries;

    public LogEntry AssertSingleEntry(LogLevel logLevel, string message)
    {
        var entry = Assert.Single(this.entries);

        Assert.Equal(logLevel, entry.LogLevel);
        Assert.Equal(message, entry.Message);

        return entry;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter) =>
        this.entries.Add(new(logLevel, eventId, formatter(state, exception), state, exception));
}
