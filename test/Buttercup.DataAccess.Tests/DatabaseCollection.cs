using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Buttercup.DataAccess
{
    [CollectionDefinition("Database collection")]
    [SuppressMessage(
        "Naming",
        "CA1711:IdentifiersShouldNotHaveIncorrectSuffix",
        Justification = "Represents an xUnit test collection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
    }
}
