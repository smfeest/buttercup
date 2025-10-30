using System.Net;
using System.Security.Claims;
using Buttercup.Application;
using Buttercup.TestUtils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using Xunit;

namespace Buttercup.Security;

[Collection(nameof(DatabaseCollection))]
public sealed class CookieAuthenticationServiceTests : DatabaseTests<DatabaseCollection>
{
    private readonly ModelFactory modelFactory = new();

    private readonly Mock<IAuthenticationService> authenticationServiceMock = new();
    private readonly Mock<IClaimsIdentityFactory> claimsIdentityFactoryMock = new();
    private readonly FakeLogger<CookieAuthenticationService> logger = new();
    private readonly Mock<ISecurityEventManager> securityEventManagerMock = new();

    private readonly CookieAuthenticationService cookieAuthenticationService;

    public CookieAuthenticationServiceTests(DatabaseFixture<DatabaseCollection> databaseFixture)
        : base(databaseFixture) =>
        this.cookieAuthenticationService = new(
            this.authenticationServiceMock.Object,
            this.claimsIdentityFactoryMock.Object,
            this.DatabaseFixture,
            this.logger,
            this.securityEventManagerMock.Object);

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
        var user = this.modelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(user);

        var httpContext = new DefaultHttpContext();
        var authenticationProperties = new AuthenticationProperties();

        var originalPrincipal = PrincipalFactory.CreateWithUserId(
            user.Id, new Claim(ClaimTypes.Name, "original-principal"));

        var ticket = new AuthenticationTicket(
            originalPrincipal,
            authenticationProperties,
            CookieAuthenticationDefaults.AuthenticationScheme);

        this.authenticationServiceMock
            .Setup(
                x => x.AuthenticateAsync(
                    httpContext, CookieAuthenticationDefaults.AuthenticationScheme))
            .ReturnsAsync(AuthenticateResult.Success(ticket));

        this.claimsIdentityFactoryMock
            .Setup(x => x.CreateIdentityForUser(user, CookieAuthenticationDefaults.AuthenticationScheme))
                .Returns(new ClaimsIdentity([new Claim(ClaimTypes.Name, "updated-principal")]));

        var result = await this.cookieAuthenticationService.RefreshPrincipal(httpContext);

        // Signs in updated principal
        this.authenticationServiceMock.Verify(
            x => x.SignInAsync(
                httpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                It.Is<ClaimsPrincipal>(p => p.HasClaim(ClaimTypes.Name, "updated-principal")),
                authenticationProperties));

        // Returns true
        Assert.True(result);
    }

    #endregion

    #region SignIn

    [Fact]
    public async Task SignIn()
    {
        var user = this.modelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(user);

        var httpContext = new DefaultHttpContext();
        var ipAddress = new IPAddress(this.modelFactory.NextInt());
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "user-identity")]);

        httpContext.Features.Set<IHttpConnectionFeature>(
            new HttpConnectionFeature { RemoteIpAddress = ipAddress });

        this.claimsIdentityFactoryMock
            .Setup(x => x.CreateIdentityForUser(user, CookieAuthenticationDefaults.AuthenticationScheme))
            .Returns(identity);

        await this.cookieAuthenticationService.SignIn(httpContext, user);

        // Signs in principal
        this.authenticationServiceMock.Verify(
            x => x.SignInAsync(
                httpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                It.Is<ClaimsPrincipal>(p => p.Identity == identity),
                null));

        // Inserts security event
        this.securityEventManagerMock.Verify(
            x => x.CreateSecurityEvent("sign_in", ipAddress, user.Id));

        // Logs signed in message
        LogAssert.SingleEntry(this.logger)
            .HasId(1)
            .HasLevel(LogLevel.Information)
            .HasMessage($"User {user.Id} ({user.Email}) signed in");
    }

    #endregion

    #region SignOut

    [Fact]
    public async Task SignOut_PreviouslySignedIn()
    {
        var user = this.modelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(user);

        var email = this.modelFactory.NextString("email");
        var ipAddress = new IPAddress(this.modelFactory.NextInt());

        var httpContext = new DefaultHttpContext()
        {
            User = PrincipalFactory.CreateWithUserId(user.Id, new Claim(ClaimTypes.Email, email)),
        };

        httpContext.Features.Set<IHttpConnectionFeature>(
            new HttpConnectionFeature { RemoteIpAddress = ipAddress });

        await this.cookieAuthenticationService.SignOut(httpContext);

        // Signs out user
        this.authenticationServiceMock.Verify(
            x => x.SignOutAsync(
                httpContext, CookieAuthenticationDefaults.AuthenticationScheme, null));

        // Inserts security event
        this.securityEventManagerMock.Verify(
            x => x.CreateSecurityEvent("sign_out", ipAddress, user.Id));

        // Logs signed out message
        LogAssert.SingleEntry(this.logger)
            .HasId(2)
            .HasLevel(LogLevel.Information)
            .HasMessage($"User {user.Id} ({email}) signed out");
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
        Assert.Equal(0, this.logger.Collector.Count);
    }

    #endregion
}
