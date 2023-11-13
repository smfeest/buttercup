using Buttercup.TestUtils;

namespace Buttercup.Application;

/// <summary>
/// A fixture that recreates the test database before tests in <see cref="DatabaseCollection" /> are
/// executed.
/// </summary>
public sealed class DatabaseCollectionFixture : DatabaseFixture<DatabaseCollection>
{
}
