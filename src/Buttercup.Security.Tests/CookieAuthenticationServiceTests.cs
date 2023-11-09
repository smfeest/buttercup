using System.Net;
using System.Security.Claims;
using Buttercup.DataAccess;
using Buttercup.TestUtils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Buttercup.Security;

public sealed class CookieAuthenticationServiceTests : IDisposable
{
    private readonly ModelFactory modelFactory = new();

    private readonly Mock<IAuthenticationService> authenticationServiceMock = new();
    private readonly FakeDbContextFactory dbContextFactory = new();
    private readonly ListLogger<CookieAuthenticationService> logger = new();
    private readonly Mock<ISecurityEventDataProvider> securityEventDataProviderMock = new();
    private readonly Mock<IUserDataProvider> userDataProviderMock = new();
    private readonly Mock<IUserPrincipalFactory> userPrincipalFactoryMock = new();

    private readonly CookieAuthenticationService cookieAuthenticationService;

    public CookieAuthenticationServiceTests() =>
        this.cookieAuthenticationService = new(
            this.authenticationServiceMock.Object,
            this.dbContextFactory,
            this.logger,
            this.securityEventDataProviderMock.Object,
            this.userDataProviderMock.Object,
            this.userPrincipalFactoryMock.Object);

    public void Dispose() => this.dbContextFactory.Dispose();

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

        this.userDataProviderMock
            .Setup(x => x.GetUser(this.dbContextFactory.FakeDbContext, user.Id))
            .ReturnsAsync(user);

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

        httpContext.Features.Set<IHttpConnectionFeature>(
            new HttpConnectionFeature { RemoteIpAddress = ipAddress });

        this.userPrincipalFactoryMock
            .Setup(x => x.Create(user, CookieAuthenticationDefaults.AuthenticationScheme))
            .Returns(principal);

        await this.cookieAuthenticationService.SignIn(httpContext, user);

        // Signs in principal
        this.authenticationServiceMock.Verify(
            x => x.SignInAsync(
                httpContext, CookieAuthenticationDefaults.AuthenticationScheme, principal, null));

        // Inserts security event
        this.securityEventDataProviderMock.Verify(x => x.LogEvent(
            this.dbContextFactory.FakeDbContext, "sign_in", ipAddress, user.Id));

        // Logs signed in message
        LogAssert.HasEntry(
            this.logger, LogLevel.Information, 212, $"User {user.Id} ({user.Email}) signed in");
    }

    #endregion

    #region SignOut

    [Fact]
    public async Task SignOut_PreviouslySignedIn_LogsSignedOut()
    {
        var email = this.modelFactory.NextString("email");
        var ipAddress = new IPAddress(this.modelFactory.NextInt());
        var userId = this.modelFactory.NextInt();

        var httpContext = new DefaultHttpContext()
        {
            User = PrincipalFactory.CreateWithUserId(userId, new Claim(ClaimTypes.Email, email)),
        };

        httpContext.Features.Set<IHttpConnectionFeature>(
            new HttpConnectionFeature { RemoteIpAddress = ipAddress });

        await this.cookieAuthenticationService.SignOut(httpContext);

        // Signs out user
        this.authenticationServiceMock.Verify(
            x => x.SignOutAsync(
                httpContext, CookieAuthenticationDefaults.AuthenticationScheme, null));

        // Inserts security event
        this.securityEventDataProviderMock.Verify(x => x.LogEvent(
            this.dbContextFactory.FakeDbContext, "sign_out", ipAddress, userId));

        // Logs signed out message
        LogAssert.HasEntry(
            this.logger, LogLevel.Information, 213, $"User {userId} ({email}) signed out");
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
}
