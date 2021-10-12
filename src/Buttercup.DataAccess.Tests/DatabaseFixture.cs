using System.Threading.Tasks;
using Xunit;

namespace Buttercup.DataAccess
{
    /// <summary>
    /// A fixture that recreates the test database before tests are executed.
    /// </summary>
    public class DatabaseFixture : IAsyncLifetime
    {
        public Task InitializeAsync() => TestDatabase.Recreate();

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
