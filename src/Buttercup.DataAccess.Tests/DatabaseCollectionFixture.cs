using Xunit;

namespace Buttercup.DataAccess;

/// <summary>
/// A fixture that recreates the test database before tests in <see cref="DatabaseCollection" /> are
/// executed.
/// </summary>
public class DatabaseCollectionFixture : IAsyncLifetime
{
    public Task InitializeAsync() => TestDatabase.Recreate();

    public Task DisposeAsync() => Task.CompletedTask;
}
