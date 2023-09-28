using Buttercup.EntityModel;
using Buttercup.Security;
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
public sealed class AppFactory<T> : WebApplicationFactory<HomeController>, IAsyncLifetime
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

    /// <summary>
    /// Creates an <see cref="HttpClient" /> with a default `Authorization` header that contains an
    /// access token for the specified user.
    /// </summary>
    /// <param name="user">
    /// The user.
    /// </param>
    /// <returns>
    /// The <see cref="HttpClient" />.
    /// </returns>
    public async Task<HttpClient> CreateClientForApiUser(User user)
    {
        var accessToken = await this.Services.GetRequiredService<ITokenAuthenticationService>()
            .IssueAccessToken(user, null);

        var client = this.CreateClient();

        client.DefaultRequestHeaders.Authorization = new("Bearer", accessToken);

        return client;
    }

    Task IAsyncLifetime.InitializeAsync() =>
        ((IAsyncLifetime)this.DatabaseFixture).InitializeAsync();

    Task IAsyncLifetime.DisposeAsync() =>
        ((IAsyncLifetime)this.DatabaseFixture).DisposeAsync();
}
