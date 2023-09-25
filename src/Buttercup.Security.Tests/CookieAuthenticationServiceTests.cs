using System.Security.Claims;
using Buttercup.DataAccess;
using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Buttercup.Security;

public sealed class CookieAuthenticationServiceTests : IDisposable
{
    private readonly ModelFactory modelFactory = new();

    private readonly Mock<IAuthenticationEventDataProvider> authenticationEventDataProviderMock =
        new();
    private readonly Mock<IAuthenticationService> authenticationServiceMock = new();
    private readonly FakeDbContextFactory dbContextFactory = new();
    private readonly ListLogger<CookieAuthenticationService> logger = new();
    private readonly Mock<IUserDataProvider> userDataProviderMock = new();
    private readonly Mock<IUserPrincipalFactory> userPrincipalFactoryMock = new();

    private readonly CookieAuthenticationService cookieAuthenticationService;

    public CookieAuthenticationServiceTests() =>
        this.cookieAuthenticationService = new(
            this.authenticationEventDataProviderMock.Object,
            this.authenticationServiceMock.Object,
            this.dbContextFactory,
            this.logger,
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
    public async Task SignIn_LogsEvent()
    {
        var values = this.SetupSignIn();

        await this.cookieAuthenticationService.SignIn(values.HttpContext, values.User);

        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            $"User {values.User.Id} ({values.User.Email}) signed in");

        this.authenticationEventDataProviderMock.Verify(x => x.LogEvent(
            this.dbContextFactory.FakeDbContext, "sign_in", values.User.Id, null));
    }

    private sealed record SignInValues(
        HttpContext HttpContext, User User, ClaimsPrincipal Principal);

    private SignInValues SetupSignIn()
    {
        var httpContext = new DefaultHttpContext();
        var user = this.modelFactory.BuildUser();
        var principal = new ClaimsPrincipal();

        this.userPrincipalFactoryMock
            .Setup(x => x.Create(user, CookieAuthenticationDefaults.AuthenticationScheme))
            .Returns(principal);

        return new(httpContext, user, principal);
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
    public async Task SignOut_PreviouslySignedIn_LogsSignOutEvent()
    {
        var userId = this.modelFactory.NextInt();
        var email = this.modelFactory.NextString("email");
        var httpContext = new DefaultHttpContext()
        {
            User = PrincipalFactory.CreateWithUserId(userId, new Claim(ClaimTypes.Email, email)),
        };

        await this.cookieAuthenticationService.SignOut(httpContext);

        LogAssert.HasEntry(
            this.logger, LogLevel.Information, $"User {userId} ({email}) signed out");

        this.authenticationEventDataProviderMock.Verify(x => x.LogEvent(
            this.dbContextFactory.FakeDbContext, "sign_out", userId, null));
    }

    [Fact]
    public async Task SignOut_NotPreviouslySignedIn_DoesNotLogSignOutEvent()
    {
        var httpContext = new DefaultHttpContext();

        await this.cookieAuthenticationService.SignOut(httpContext);

        this.authenticationEventDataProviderMock.Verify(
            x => x.LogEvent(this.dbContextFactory.FakeDbContext, "sign_out", null, null),
            Times.Never);
    }

    #endregion
}
