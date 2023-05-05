using Buttercup.EntityModel;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.TestUtils;

/// <summary>
/// A fake implementation of <see cref="IDbContextFactory{TContext}" /> that always returns an
/// unconfigured, singleton instance of <see cref="AppDbContext" />.
/// </summary>
/// <remarks>
/// This fake is intended to be used in tests that need to pass an <see cref="AppDbContext" /> to
/// mock data provider methods.
/// </remarks>
public sealed class FakeDbContextFactory : IDbContextFactory<AppDbContext>, IDisposable
{
    /// <summary>
    /// Gets the unconfigured, singleton instance of <see cref="AppDbContext" /> that's returned by
    /// <see cref="CreateDbContext()" />.
    /// </summary>
    public AppDbContext FakeDbContext { get; } = new();

    /// <inheritdoc/>
    public AppDbContext CreateDbContext() => this.FakeDbContext;

    /// <inheritdoc/>
    public void Dispose() => this.FakeDbContext.Dispose();
}
