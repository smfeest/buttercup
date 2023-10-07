using System.Net;
using System.Security.Claims;
using Buttercup.DataAccess;
using Buttercup.EntityModel;
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
    public async Task RefreshPrincipal_Unauthenticated_ReturnsFalse()
    {
        var httpContext = new DefaultHttpContext();

        this.SetupAuthenticateAsync(httpContext, AuthenticateResult.NoResult());

        Assert.False(await this.cookieAuthenticationService.RefreshPrincipal(httpContext));
    }

    [Fact]
    public async Task RefreshPrincipal_Authenticated_SignsInUpdatedPrincipal()
    {
        var values = this.SetupRefreshPrincipal_Authenticated();

        await this.cookieAuthenticationService.RefreshPrincipal(
            values.HttpContext);

        this.authenticationServiceMock.Verify(
            x => x.SignInAsync(
                values.HttpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                values.UpdatedPrincipal,
                values.AuthenticationProperties));
    }

    [Fact]
    public async Task RefreshPrincipal_Authenticated_ReturnsTrue()
    {
        var values = this.SetupRefreshPrincipal_Authenticated();

        Assert.True(await this.cookieAuthenticationService.RefreshPrincipal(values.HttpContext));
    }

    private sealed record RefreshPrincipalValues(
        HttpContext HttpContext,
        AuthenticationProperties AuthenticationProperties,
        User User,
        ClaimsPrincipal UpdatedPrincipal);

    private RefreshPrincipalValues SetupRefreshPrincipal_Authenticated()
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

        this.SetupAuthenticateAsync(httpContext, AuthenticateResult.Success(ticket));

        this.userDataProviderMock
            .Setup(x => x.GetUser(this.dbContextFactory.FakeDbContext, user.Id))
            .ReturnsAsync(user);

        this.userPrincipalFactoryMock
            .Setup(x => x.Create(user, CookieAuthenticationDefaults.AuthenticationScheme))
                .Returns(updatedPrincipal);

        return new(httpContext, authenticationProperties, user, updatedPrincipal);
    }

    private void SetupAuthenticateAsync(
        HttpContext httpContext, AuthenticateResult authenticateResult) =>
        this.authenticationServiceMock
            .Setup(x => x.AuthenticateAsync(
                httpContext, CookieAuthenticationDefaults.AuthenticationScheme))
            .ReturnsAsync(authenticateResult);

    #endregion

    #region SignIn

    [Fact]
    public async Task SignIn_SignsInPrincipal()
    {
        var values = this.SetupSignIn();

        await this.cookieAuthenticationService.SignIn(values.HttpContext, values.User);

        this.authenticationServiceMock.Verify(
            x => x.SignInAsync(
                values.HttpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                values.Principal,
                null));
    }

    [Fact]
    public async Task SignIn_LogsSignedIn()
    {
        var values = this.SetupSignIn();

        await this.cookieAuthenticationService.SignIn(values.HttpContext, values.User);

        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            212,
            $"User {values.User.Id} ({values.User.Email}) signed in");
    }

    [Fact]
    public async Task SignIn_InsertsSecurityEvent()
    {
        var values = this.SetupSignIn();

        await this.cookieAuthenticationService.SignIn(values.HttpContext, values.User);

        this.securityEventDataProviderMock.Verify(x => x.LogEvent(
            this.dbContextFactory.FakeDbContext, "sign_in", values.IpAddress, values.User.Id));
    }

    private sealed record SignInValues(
        HttpContext HttpContext, IPAddress IpAddress, ClaimsPrincipal Principal, User User);

    private SignInValues SetupSignIn()
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

        return new(httpContext, ipAddress, principal, user);
    }

    #endregion

    #region SignOut

    [Fact]
    public async Task SignOut_SignsOutUser()
    {
        var httpContext = new DefaultHttpContext();

        await this.cookieAuthenticationService.SignOut(httpContext);

        this.authenticationServiceMock.Verify(
            x => x.SignOutAsync(
                httpContext, CookieAuthenticationDefaults.AuthenticationScheme, null));
    }

    [Fact]
    public async Task SignOut_PreviouslySignedIn_LogsSignedOut()
    {
        var values = this.SetupSignOut_PreviouslySignedIn();

        await this.cookieAuthenticationService.SignOut(values.HttpContext);

        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            213,
            $"User {values.UserId} ({values.Email}) signed out");
    }

    [Fact]
    public async Task SignOut_PreviouslySignedIn_InsertsSecurityEvent()
    {
        var values = this.SetupSignOut_PreviouslySignedIn();

        await this.cookieAuthenticationService.SignOut(values.HttpContext);

        this.securityEventDataProviderMock.Verify(x => x.LogEvent(
            this.dbContextFactory.FakeDbContext, "sign_out", values.IpAddress, values.UserId));
    }

    [Fact]
    public async Task SignOut_NotPreviouslySignedIn_DoesNotLog()
    {
        var httpContext = new DefaultHttpContext();

        await this.cookieAuthenticationService.SignOut(httpContext);

        Assert.Empty(this.logger.Entries);
    }

    private sealed record SignOutPreviouslySignedInValues(
        string Email, HttpContext HttpContext, IPAddress IpAddress, long UserId);

    private SignOutPreviouslySignedInValues SetupSignOut_PreviouslySignedIn()
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

        return new(email, httpContext, ipAddress, userId);
    }

    #endregion
}
