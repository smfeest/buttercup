using System.Diagnostics.CodeAnalysis;
using Buttercup.Web.TestUtils;
using Xunit;

namespace Buttercup.Web;

[CollectionDefinition(nameof(EndToEndCollection))]
[SuppressMessage(
    "Naming",
    "CA1711:IdentifiersShouldNotHaveIncorrectSuffix",
    Justification = "Represents an xUnit test collection")]
public sealed class EndToEndCollection : ICollectionFixture<AppFactory>
{
}
