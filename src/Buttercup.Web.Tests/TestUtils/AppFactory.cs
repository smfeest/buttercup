using Buttercup.TestUtils;
using Buttercup.Web.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Buttercup.Web.TestUtils;

/// <summary>
/// A fixture for bootstrapping the application in memory for end to end tests.
/// </summary>
/// <typeparam name="T">
/// The test class.
/// </typeparam>
public class AppFactory<T> : WebApplicationFactory<HomeController>, IAsyncLifetime
{
    /// <summary>
    /// Gets the database fixture.
    /// </summary>
    public DatabaseFixture<T> DatabaseFixture { get; } = new();

    /// <inheritdoc/>
    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder
            .UseSetting("ConnectionStrings:AppDb", this.DatabaseFixture.ConnectionString)
            .UseSetting("Email:ApiKey", "fake-key")
            .UseSetting("HostBuilder:ReloadConfigOnChange", bool.FalseString)
            .UseSetting("Logging:LogLevel:Default", "Warning")
            .UseSetting("Logging:LogLevel:Microsoft.EntityFrameworkCore", "Warning");

    Task IAsyncLifetime.InitializeAsync() =>
        ((IAsyncLifetime)this.DatabaseFixture).InitializeAsync();

    Task IAsyncLifetime.DisposeAsync() =>
        ((IAsyncLifetime)this.DatabaseFixture).DisposeAsync();
}
