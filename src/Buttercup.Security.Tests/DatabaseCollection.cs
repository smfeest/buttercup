using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Buttercup.Security;

[CollectionDefinition(nameof(DatabaseCollection))]
[SuppressMessage(
    "Naming",
    "CA1711:IdentifiersShouldNotHaveIncorrectSuffix",
    Justification = "Represents an xUnit test collection")]
public sealed class DatabaseCollection : ICollectionFixture<DatabaseCollectionFixture>
{
}