using Xunit;

namespace Buttercup.TestUtils;

/// <summary>
/// A base class for test classes that require a test database.
/// </summary>
/// <remarks>
/// <para>
/// This class is intended to be used in conjunction with <see cref="DatabaseFixture{T}"/> as a
/// class or collection fixture.
/// </para>
/// <para>
/// It provides a default implementation of <see cref="IAsyncLifetime.InitializeAsync" /> that
/// clears the database before each test.
/// </para>
/// </remarks>
/// <typeparam name="T">
/// The collection class if <see cref="DatabaseFixture{T}"/> is being used as a collection fixture,
/// or test class if <see cref="DatabaseFixture{T}"/> is being used as a class fixture.
/// </typeparam>
/// <param name="databaseFixture">
/// The database fixture.
/// </param>
public abstract class DatabaseTests<T>(DatabaseFixture<T> databaseFixture) : IAsyncLifetime
{
    /// <summary>
    /// Gets the database fixture.
    /// </summary>
    protected DatabaseFixture<T> DatabaseFixture => databaseFixture;

    /// <inheritdoc/>
    public virtual Task InitializeAsync() => this.DatabaseFixture.ClearDatabase();

    /// <inheritdoc/>
    public virtual Task DisposeAsync() => Task.CompletedTask;
}