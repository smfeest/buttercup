using Buttercup.EntityModel;
using Buttercup.Security;
using Buttercup.TestUtils;
using Buttercup.Web.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Options;
using Xunit;

namespace Buttercup.Web.TestUtils;

/// <summary>
/// A fixture for bootstrapping the application in memory for end-to-end tests.
/// </summary>
public sealed class AppFactory : WebApplicationFactory<HomeController>, IAsyncLifetime
{
    /// <summary>
    /// Gets the database fixture.
    /// </summary>
    public DatabaseFixture<AppFactory> DatabaseFixture { get; } = new();

    /// <inheritdoc/>
    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder
            .UseSetting("ConnectionStrings:AppDb", this.DatabaseFixture.ConnectionString)
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

    /// <summary>
    /// Gets a configured <typeparamref name="T"/> instance.
    /// </summary>
    /// <typeparam name="T">The options type.</typeparam>
    /// <returns>The configured <typeparamref name="T"/> instance.</returns>
    public T GetOptions<T>() where T : class =>
        this.Services.GetRequiredService<IOptions<T>>().Value;

    public ValueTask InitializeAsync() => this.DatabaseFixture.InitializeAsync();

    public override async ValueTask DisposeAsync()
    {
        await this.DatabaseFixture.DisposeAsync();
        await base.DisposeAsync();
    }
}
