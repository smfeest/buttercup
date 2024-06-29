using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.Web.TestUtils;

/// <summary>
/// A base class for end to end tests.
/// </summary>
/// <remarks>
/// <para>
/// This class includes the tests in <see cref="EndToEndCollection"/>, which uses <see
/// cref="TestUtils.AppFactory" /> as a collection fixture to create a test database and bootstrap
/// the application before the first end to end test is run.
/// </para>
/// <para>
/// It also inherits from <see cref="DatabaseTests{T}"/> to provide a base implementation of <see
/// cref="IAsyncLifetime.InitializeAsync" /> that clears the database before each test.
/// </para>
/// </remarks>
/// <param name="appFactory">
/// The application factory.
/// </param>
[Collection(nameof(EndToEndCollection))]
public abstract class EndToEndTests(AppFactory appFactory)
    : DatabaseTests<AppFactory>(appFactory.DatabaseFixture)
{
    /// <summary>
    /// Gets the application factory.
    /// </summary>
    protected AppFactory AppFactory { get; } = appFactory;

    /// <summary>
    /// Gets the model factory.
    /// </summary>
    protected ModelFactory ModelFactory { get; } = new();
}
