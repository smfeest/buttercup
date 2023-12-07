using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.Web.TestUtils;

/// <summary>
/// A base class for end to end tests.
/// </summary>
/// <remarks>
/// <para>
/// This class adds <see cref="AppFactory{T}" /> as a class fixture to create a test database and
/// bootstrap the application before the first test in the class is run.
/// </para>
/// <para>
/// It inherits from <see cref="DatabaseTests{T}"/> that provides a default implementation of <see
/// cref="IAsyncLifetime.InitializeAsync" /> that clears the database before each test.
/// </para>
/// <para>
/// Type parameter <typeparamref name="T" /> is used to generate a unique name for the database so
/// that multiple end to end test classes can safely run in parallel.
/// </para>
/// </remarks>
/// <typeparam name="T">
/// The test class.
/// </typeparam>
/// <param name="appFactory">
/// The application factory.
/// </param>
public abstract class EndToEndTests<T>(AppFactory<T> appFactory)
    : DatabaseTests<T>(appFactory.DatabaseFixture), IClassFixture<AppFactory<T>>
{
    /// <summary>
    /// Gets the application factory.
    /// </summary>
    protected AppFactory<T> AppFactory { get; } = appFactory;

    /// <summary>
    /// Gets the model factory.
    /// </summary>
    protected ModelFactory ModelFactory { get; } = new();
}
