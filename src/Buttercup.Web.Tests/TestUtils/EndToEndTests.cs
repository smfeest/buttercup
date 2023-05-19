using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.Web.TestUtils;

/// <summary>
/// A base class for end to end tests.
/// </summary>
/// <remarks>
/// <para>
/// This class adds <see cref="AppFactory{T}" /> as a class fixture to create the database and
/// bootstrap the application before the first test in the class is run and a default implementation
/// of <see cref="IAsyncLifetime.DisposeAsync" /> that cleans the database after each test.
/// </para>
/// <para>
/// Type parameter <typeparamref name="T" /> is used to generate a unique name for the database so
/// that multiple end to end test classes can safely run in parallel.
/// </para>
/// </remarks>
/// <typeparam name="T">
/// The test class.
/// </typeparam>
public abstract class EndToEndTests<T> : IAsyncLifetime, IClassFixture<AppFactory<T>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppFactory{T}" /> class.
    /// </summary>
    /// <param name="appFactory">
    /// The application factory.
    /// </param>
    public EndToEndTests(AppFactory<T> appFactory) => this.AppFactory = appFactory;

    /// <summary>
    /// Gets the application factory.
    /// </summary>
    protected AppFactory<T> AppFactory { get; }

    /// <summary>
    /// Gets the database fixture.
    /// </summary>
    protected DatabaseFixture<T> DatabaseFixture => this.AppFactory.DatabaseFixture;

    /// <summary>
    /// Gets the model factory.
    /// </summary>
    protected ModelFactory ModelFactory { get; } = new();

    /// <inheritdoc/>
    public virtual Task InitializeAsync() => this.DatabaseFixture.ClearDatabase();

    /// <inheritdoc/>
    public virtual Task DisposeAsync() => Task.CompletedTask;

}
