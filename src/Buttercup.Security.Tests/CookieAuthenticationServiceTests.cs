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

public sealed class CookieAuthenticationServiceTests
{
    #region RefreshPrincipal

    [Fact]
    public async Task RefreshPrincipal_Unauthenticated_ReturnsFalse()
    {
        using var fixture = new RefreshPrincipalFixture();

        fixture.SetupUnauthenticated();

        Assert.False(await fixture.RefreshPrincipal());
    }

    [Fact]
    public async Task RefreshPrincipal_Authenticated_SignsInUpdatedPrincipal()
    {
        using var fixture = new RefreshPrincipalFixture();

        fixture.SetupAuthenticated();

        await fixture.RefreshPrincipal();

        fixture.MockAuthenticationService.Verify(
            x => x.SignInAsync(
                fixture.HttpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                fixture.UpdatedPrincipal,
                fixture.AuthenticationProperties));
    }

    [Fact]
    public async Task RefreshPrincipal_Authenticated_ReturnsTrue()
    {
        using var fixture = new RefreshPrincipalFixture();

        fixture.SetupAuthenticated();

        Assert.True(await fixture.RefreshPrincipal());
    }

    private sealed class RefreshPrincipalFixture : CookieAuthenticationServiceFixture
    {
        public DefaultHttpContext HttpContext { get; } = new();

        public AuthenticationProperties AuthenticationProperties { get; } = new();

        public User User { get; } = new ModelFactory().BuildUser();

        public ClaimsPrincipal UpdatedPrincipal { get; } = new();

        public void SetupUnauthenticated() =>
            this.SetupAuthenticateAsync(AuthenticateResult.NoResult());

        public void SetupAuthenticated()
        {
            var ticket = new AuthenticationTicket(
                PrincipalFactory.CreateWithUserId(this.User.Id),
                this.AuthenticationProperties,
                CookieAuthenticationDefaults.AuthenticationScheme);
            var result = AuthenticateResult.Success(ticket);

            this.SetupAuthenticateAsync(result);

            this.MockUserDataProvider
                .Setup(x => x.GetUser(this.DbContextFactory.FakeDbContext, this.User.Id))
                .ReturnsAsync(this.User);

            this.MockUserPrincipalFactory
                .Setup(x => x.Create(this.User, CookieAuthenticationDefaults.AuthenticationScheme))
                .Returns(this.UpdatedPrincipal);
        }

        public Task<bool> RefreshPrincipal() =>
            this.CookieAuthenticationService.RefreshPrincipal(this.HttpContext);

        private void SetupAuthenticateAsync(AuthenticateResult authenticateResult) =>
            this.MockAuthenticationService
                .Setup(x => x.AuthenticateAsync(
                    this.HttpContext, CookieAuthenticationDefaults.AuthenticationScheme))
                .ReturnsAsync(authenticateResult);
    }

    #endregion

    #region SignIn

    [Fact]
    public async Task SignIn_SignsInPrincipal()
    {
        using var fixture = new SignInFixture();

        await fixture.SignIn();

        fixture.MockAuthenticationService.Verify(x => x.SignInAsync(fixture.HttpContext,
                CookieAuthenticationDefaults.AuthenticationScheme, fixture.UserPrincipal, null));
    }

    [Fact]
    public async Task SignIn_LogsEvent()
    {
        using var fixture = new SignInFixture();

        await fixture.SignIn();

        Assert.Contains(
            fixture.Logger.Entries,
            entry =>
                entry.LogLevel == LogLevel.Information &&
                entry.Message == $"User {fixture.User.Id} ({fixture.User.Email}) signed in");

        fixture.AssertAuthenticationEventLogged("sign_in", fixture.User.Id);
    }

    private sealed class SignInFixture : CookieAuthenticationServiceFixture
    {
        public SignInFixture() =>
            this.MockUserPrincipalFactory
                .Setup(x => x.Create(this.User, CookieAuthenticationDefaults.AuthenticationScheme))
                .Returns(this.UserPrincipal);

        public DefaultHttpContext HttpContext { get; } = new();

        public User User { get; } = new ModelFactory().BuildUser();

        public ClaimsPrincipal UserPrincipal { get; } = new();

        public Task SignIn() => this.CookieAuthenticationService.SignIn(this.HttpContext, this.User);
    }

    #endregion

    #region SignOut

    [Fact]
    public async Task SignOut_SignsOutUser()
    {
        using var fixture = SignOutFixture.ForUserSignedIn();

        await fixture.SignOut();

        fixture.MockAuthenticationService.Verify(x => x.SignOutAsync(
            fixture.HttpContext, CookieAuthenticationDefaults.AuthenticationScheme, null));
    }

    [Fact]
    public async Task SignOut_PreviouslySignedIn_LogsSignOutEvent()
    {
        using var fixture = SignOutFixture.ForUserSignedIn();

        await fixture.SignOut();

        Assert.Contains(
            fixture.Logger.Entries,
            entry =>
                entry.LogLevel == LogLevel.Information &&
                entry.Message == $"User {fixture.UserId} ({fixture.Email}) signed out");

        fixture.AssertAuthenticationEventLogged("sign_out", fixture.UserId);
    }

    [Fact]
    public async Task SignOut_NotPreviouslySignedIn_DoesNotLogSignOutEvent()
    {
        using var fixture = SignOutFixture.ForNoUserSignedIn();

        await fixture.SignOut();

        fixture.MockAuthenticationEventDataProvider.Verify(
            x => x.LogEvent(fixture.DbContextFactory.FakeDbContext, "sign_out", null, null),
            Times.Never);
    }

    private sealed class SignOutFixture : CookieAuthenticationServiceFixture
    {
        private SignOutFixture(long? userId) => this.UserId = userId;

        public DefaultHttpContext HttpContext { get; } = new();

        public long? UserId { get; }

        public string Email { get; } = "sample@example.com";

        public static SignOutFixture ForNoUserSignedIn() => new(null);

        public static SignOutFixture ForUserSignedIn()
        {
            using var fixture = new SignOutFixture(76);

            var claims = new Claim[]
            {
                new(ClaimTypes.NameIdentifier, "76"),
                new(ClaimTypes.Email, fixture.Email),
            };

            fixture.HttpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

            return fixture;
        }

        public Task SignOut() => this.CookieAuthenticationService.SignOut(this.HttpContext);
    }

    #endregion

    private class CookieAuthenticationServiceFixture : IDisposable
    {
        public CookieAuthenticationServiceFixture() =>
            this.CookieAuthenticationService = new(
                this.MockAuthenticationEventDataProvider.Object,
                this.MockAuthenticationService.Object,
                this.DbContextFactory,
                this.Logger,
                this.MockUserDataProvider.Object,
                this.MockUserPrincipalFactory.Object);

        public Mock<IAuthenticationEventDataProvider> MockAuthenticationEventDataProvider { get; } = new();

        public CookieAuthenticationService CookieAuthenticationService { get; }

        public FakeDbContextFactory DbContextFactory { get; } = new();

        public ListLogger<CookieAuthenticationService> Logger { get; } = new();

        public Mock<IAuthenticationService> MockAuthenticationService { get; } = new();

        public Mock<IUserDataProvider> MockUserDataProvider { get; } = new();

        public Mock<IUserPrincipalFactory> MockUserPrincipalFactory { get; } = new();

        public void AssertAuthenticationEventLogged(
            string eventName, long? userId = null, string? email = null) =>
            this.MockAuthenticationEventDataProvider.Verify(x => x.LogEvent(
                this.DbContextFactory.FakeDbContext, eventName, userId, email));

        public void Dispose() => this.DbContextFactory.Dispose();
    }
}
