using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Buttercup.DataAccess;

[CollectionDefinition(nameof(DatabaseCollection))]
[SuppressMessage(
    "Naming",
    "CA1711:IdentifiersShouldNotHaveIncorrectSuffix",
    Justification = "Represents an xUnit test collection")]
public class DatabaseCollection : ICollectionFixture<DatabaseCollectionFixture>
{
}
