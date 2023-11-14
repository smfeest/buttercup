using System.Net;
using System.Security.Claims;
using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Buttercup.Security;

[Collection(nameof(DatabaseCollection))]
public sealed class CookieAuthenticationServiceTests : IAsyncLifetime
{
    private readonly DatabaseCollectionFixture databaseFixture;
    private readonly ModelFactory modelFactory = new();

    private readonly Mock<IAuthenticationService> authenticationServiceMock = new();
    private readonly StoppedClock clock;
    private readonly ListLogger<CookieAuthenticationService> logger = new();
    private readonly Mock<IUserPrincipalFactory> userPrincipalFactoryMock = new();

    private readonly CookieAuthenticationService cookieAuthenticationService;

    public CookieAuthenticationServiceTests(DatabaseCollectionFixture databaseFixture)
    {
        this.databaseFixture = databaseFixture;
        this.clock = new() { UtcNow = this.modelFactory.NextDateTime() };

        this.cookieAuthenticationService = new(
            this.authenticationServiceMock.Object,
            this.clock,
            this.databaseFixture,
            this.logger,
            this.userPrincipalFactoryMock.Object);
    }

    public Task InitializeAsync() => this.databaseFixture.ClearDatabase();

    public Task DisposeAsync() => Task.CompletedTask;

    #region RefreshPrincipal

    [Fact]
    public async Task RefreshPrincipal_Unauthenticated()
    {
        var httpContext = new DefaultHttpContext();

        this.authenticationServiceMock
            .Setup(
                x => x.AuthenticateAsync(
                    httpContext, CookieAuthenticationDefaults.AuthenticationScheme))
            .ReturnsAsync(AuthenticateResult.NoResult());

        // Returns false
        Assert.False(await this.cookieAuthenticationService.RefreshPrincipal(httpContext));
    }

    [Fact]
    public async Task RefreshPrincipal_Authenticated()
    {
        var httpContext = new DefaultHttpContext();
        var authenticationProperties = new AuthenticationProperties();
        var user = this.modelFactory.BuildUser();

        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }

        var originalPrincipal = PrincipalFactory.CreateWithUserId(
            user.Id, new Claim(ClaimTypes.Name, "original-principal"));
        var updatedPrincipal = PrincipalFactory.CreateWithUserId(
            user.Id, new Claim(ClaimTypes.Name, "updated-principal"));

        var ticket = new AuthenticationTicket(
            originalPrincipal,
            authenticationProperties,
            CookieAuthenticationDefaults.AuthenticationScheme);

        this.authenticationServiceMock
            .Setup(
                x => x.AuthenticateAsync(
                    httpContext, CookieAuthenticationDefaults.AuthenticationScheme))
            .ReturnsAsync(AuthenticateResult.Success(ticket));

        this.userPrincipalFactoryMock
            .Setup(x => x.Create(user, CookieAuthenticationDefaults.AuthenticationScheme))
                .Returns(updatedPrincipal);

        var result = await this.cookieAuthenticationService.RefreshPrincipal(httpContext);

        // Signs in updated principal
        this.authenticationServiceMock.Verify(
            x => x.SignInAsync(
                httpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                updatedPrincipal,
                authenticationProperties));

        // Returns true
        Assert.True(result);
    }

    #endregion

    #region SignIn

    [Fact]
    public async Task SignIn()
    {
        var httpContext = new DefaultHttpContext();
        var ipAddress = new IPAddress(this.modelFactory.NextInt());
        var principal = new ClaimsPrincipal();
        var user = this.modelFactory.BuildUser();

        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }

        httpContext.Features.Set<IHttpConnectionFeature>(
            new HttpConnectionFeature { RemoteIpAddress = ipAddress });

        this.userPrincipalFactoryMock
            .Setup(x => x.Create(user, CookieAuthenticationDefaults.AuthenticationScheme))
            .Returns(principal);

        await this.cookieAuthenticationService.SignIn(httpContext, user);

        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
            // Signs in principal
            this.authenticationServiceMock.Verify(
                x => x.SignInAsync(
                    httpContext,
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    null));

            // Inserts security event
            Assert.True(await this.SecurityEventExists(dbContext, "sign_in", ipAddress, user.Id));

            // Logs signed in message
            LogAssert.HasEntry(
                this.logger, LogLevel.Information, 212, $"User {user.Id} ({user.Email}) signed in");
        }
    }

    #endregion

    #region SignOut

    [Fact]
    public async Task SignOut_PreviouslySignedIn()
    {
        var email = this.modelFactory.NextString("email");
        var ipAddress = new IPAddress(this.modelFactory.NextInt());
        var user = this.modelFactory.BuildUser();

        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }

        var httpContext = new DefaultHttpContext()
        {
            User = PrincipalFactory.CreateWithUserId(user.Id, new Claim(ClaimTypes.Email, email)),
        };

        httpContext.Features.Set<IHttpConnectionFeature>(
            new HttpConnectionFeature { RemoteIpAddress = ipAddress });

        await this.cookieAuthenticationService.SignOut(httpContext);

        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
            // Signs out user
            this.authenticationServiceMock.Verify(
                x => x.SignOutAsync(
                    httpContext, CookieAuthenticationDefaults.AuthenticationScheme, null));

            // Inserts security event
            Assert.True(await this.SecurityEventExists(dbContext, "sign_out", ipAddress, user.Id));

            // Logs signed out message
            LogAssert.HasEntry(
                this.logger, LogLevel.Information, 213, $"User {user.Id} ({email}) signed out");
        }
    }

    [Fact]
    public async Task SignOut_NotPreviouslySignedIn()
    {
        var httpContext = new DefaultHttpContext();

        await this.cookieAuthenticationService.SignOut(httpContext);

        // Signs out user
        this.authenticationServiceMock.Verify(
            x => x.SignOutAsync(
                httpContext, CookieAuthenticationDefaults.AuthenticationScheme, null));

        // Does not log
        Assert.Empty(this.logger.Entries);
    }

    #endregion

    private async Task<bool> SecurityEventExists(
        AppDbContext dbContext, string eventName, IPAddress ipAddress, long? userId = null) =>
        await dbContext.SecurityEvents.AnyAsync(
            securityEvent =>
                securityEvent.Time == this.clock.UtcNow &&
                securityEvent.Event == eventName &&
                securityEvent.IpAddress == ipAddress &&
                securityEvent.UserId == userId);
}
