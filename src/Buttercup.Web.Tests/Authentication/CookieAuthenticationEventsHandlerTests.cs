using System.Globalization;
using System.Security.Claims;
using Buttercup.DataAccess;
using Buttercup.EntityModel;
using Buttercup.Security;
using Buttercup.TestUtils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Moq;
using Xunit;

namespace Buttercup.Web.Authentication;

public sealed class CookieAuthenticationEventsHandlerTests : IDisposable
{
    private readonly ModelFactory modelFactory = new();

    private readonly Mock<IAuthenticationService> authenticationServiceMock = new();
    private readonly FakeDbContextFactory dbContextFactory = new();
    private readonly ListLogger<CookieAuthenticationEventsHandler> logger = new();
    private readonly Mock<IUserDataProvider> userDataProviderMock = new();
    private readonly Mock<IUserPrincipalFactory> userPrincipalFactoryMock = new();
    private readonly CookieAuthenticationEventsHandler cookieAuthenticationEventsHandler;

    public CookieAuthenticationEventsHandlerTests() =>
        this.cookieAuthenticationEventsHandler = new(
            this.authenticationServiceMock.Object,
            this.dbContextFactory,
            this.logger,
            this.userDataProviderMock.Object,
            this.userPrincipalFactoryMock.Object);

    public void Dispose() => this.dbContextFactory.Dispose();

    #region ValidatePrincipal

    [Fact]
    public async Task ValidatePrincipal_Unauthenticated_DoesNotRejectPrincipal()
    {
        var initialPrincipal = new ClaimsPrincipal();
        var context = this.SetupValidatePrincipalContext(initialPrincipal);

        await this.cookieAuthenticationEventsHandler.ValidatePrincipal(context);

        Assert.Same(initialPrincipal, context.Principal);
    }

    [Theory]
    [MemberData(nameof(GetTheoryDataForSecurityStampIsMissingOrStale))]
    public async Task ValidatePrincipal_SecurityStampIsMissingOrStale_LogsInfoMessage(
        Func<User, ClaimsPrincipal> principalFactory)
    {
        var user = this.SetupGetUser();
        var context = this.SetupValidatePrincipalContext(principalFactory(user));

        await this.cookieAuthenticationEventsHandler.ValidatePrincipal(context);

        Assert.Contains(
            this.logger.Entries,
            entry =>
                entry.LogLevel == LogLevel.Information &&
                entry.Message == $"Incorrect security stamp for user {user.Id} ({user.Email})");
    }

    [Theory]
    [MemberData(nameof(GetTheoryDataForSecurityStampIsMissingOrStale))]
    public async Task ValidatePrincipal_SecurityStampIsMissingOrStale_RejectsPrincipal(
        Func<User, ClaimsPrincipal> principalFactory)
    {
        var context = this.SetupValidatePrincipalContext(principalFactory(this.SetupGetUser()));

        await this.cookieAuthenticationEventsHandler.ValidatePrincipal(context);

        Assert.Null(context.Principal);
    }

    [Theory]
    [MemberData(nameof(GetTheoryDataForSecurityStampIsMissingOrStale))]
    public async Task ValidatePrincipal_SecurityStampIsMissingOrStale_SignsUserOut(
        Func<User, ClaimsPrincipal> principalFactory)
    {
        var context = this.SetupValidatePrincipalContext(principalFactory(this.SetupGetUser()));

        await this.cookieAuthenticationEventsHandler.ValidatePrincipal(context);

        this.authenticationServiceMock.Verify(
            x => x.SignOutAsync(context.HttpContext, context.Scheme.Name, null));
    }

    [Fact]
    public async Task ValidatePrincipal_SecurityStampMatches_LogsDebugMessage()
    {
        var user = this.SetupGetUser();
        var context = this.SetupValidatePrincipalContext(SetupPrincipal(user));

        await this.cookieAuthenticationEventsHandler.ValidatePrincipal(context);

        Assert.Contains(
            this.logger.Entries,
            entry =>
                entry.LogLevel == LogLevel.Debug &&
                entry.Message == $"Principal successfully validated for user {user.Id} ({user.Email})");
    }

    [Theory]
    [MemberData(nameof(GetTheoryDataForUserRevisionIsMissingOrStale))]
    public async Task ValidatePrincipal_UserRevisionIsMissingOrStale_ReplacesPrincipal(
         Func<User, ClaimsPrincipal> principalFactory)
    {
        var user = this.SetupGetUser();
        var context = this.SetupValidatePrincipalContext(principalFactory(user));
        var updatedPrincipal = this.SetupCreatePrincipal(user, context.Scheme);

        await this.cookieAuthenticationEventsHandler.ValidatePrincipal(context);

        Assert.Same(updatedPrincipal, context.Principal);
    }

    [Theory]
    [MemberData(nameof(GetTheoryDataForUserRevisionIsMissingOrStale))]
    public async Task ValidatePrincipal_UserRevisionIsMissingOrStale_RenewsCookie(
        Func<User, ClaimsPrincipal> principalFactory)
    {
        var context = this.SetupValidatePrincipalContext(principalFactory(this.SetupGetUser()));

        await this.cookieAuthenticationEventsHandler.ValidatePrincipal(context);

        Assert.True(context.ShouldRenew);
    }

    [Theory]
    [MemberData(nameof(GetTheoryDataForUserRevisionIsMissingOrStale))]
    public async Task ValidatePrincipal_UserRevisionIsMissingOrStale_LogsInfoMessage(
        Func<User, ClaimsPrincipal> principalFactory)
    {
        var user = this.SetupGetUser();
        var context = this.SetupValidatePrincipalContext(principalFactory(user));

        await this.cookieAuthenticationEventsHandler.ValidatePrincipal(context);

        Assert.Contains(
            this.logger.Entries,
            entry =>
                entry.LogLevel == LogLevel.Information &&
                entry.Message == $"Refreshed claims principal for user {user.Id} ({user.Email})");
    }

    [Fact]
    public async Task ValidatePrincipal_UserRevisionMatches_RetainsPrincipal()
    {
        var initialPrincipal = SetupPrincipal(this.SetupGetUser());
        var context = this.SetupValidatePrincipalContext(initialPrincipal);

        await this.cookieAuthenticationEventsHandler.ValidatePrincipal(context);

        Assert.Same(initialPrincipal, context.Principal);
    }

    public static object[][] GetTheoryDataForSecurityStampIsMissingOrStale()
    {
        ClaimsPrincipal SetupPrincipalWithoutSecurityStamp(User user) =>
            SetupPrincipal(user, claims => claims.Remove(CustomClaimTypes.SecurityStamp));
        ClaimsPrincipal SetupPrincipalWithStaleSecurityStamp(User user) =>
            SetupPrincipal(user with { SecurityStamp = "stale-security-stamp" });

        return new object[][]
        {
            new[] { SetupPrincipalWithoutSecurityStamp },
            new[] { SetupPrincipalWithStaleSecurityStamp }
        };
    }

    public static object[][] GetTheoryDataForUserRevisionIsMissingOrStale()
    {
        ClaimsPrincipal SetupPrincipalWithoutUserRevision(User user) =>
            SetupPrincipal(user, claims => claims.Remove(CustomClaimTypes.UserRevision));
        ClaimsPrincipal SetupPrincipalWithStaleUserRevision(User user) =>
            SetupPrincipal(user with { Revision = user.Revision - 1 });

        return new object[][]
        {
            new[] { SetupPrincipalWithoutUserRevision },
            new[] { SetupPrincipalWithStaleUserRevision }
        };
    }

    private ClaimsPrincipal SetupCreatePrincipal(User user, AuthenticationScheme scheme)
    {
        var updatedPrincipal = new ClaimsPrincipal();

        this.userPrincipalFactoryMock
            .Setup(x => x.Create(user, scheme.Name))
            .Returns(updatedPrincipal);

        return updatedPrincipal;
    }

    private User SetupGetUser()
    {
        var user = this.modelFactory.BuildUser();

        this.userDataProviderMock
            .Setup(x => x.GetUser(this.dbContextFactory.FakeDbContext, user.Id))
            .ReturnsAsync(user);

        return user;
    }

    private static ClaimsPrincipal SetupPrincipal(
        User user, Action<Dictionary<string, string>>? configureClaims = null)
    {
        var claims = new Dictionary<string, string>
        {
            [ClaimTypes.NameIdentifier] = user.Id.ToString(CultureInfo.InvariantCulture),
            [CustomClaimTypes.SecurityStamp] = user.SecurityStamp,
            [CustomClaimTypes.UserRevision] = user.Revision.ToString(CultureInfo.InvariantCulture),
        };

        configureClaims?.Invoke(claims);

        return new(new ClaimsIdentity(claims.Select((kvp) => new Claim(kvp.Key, kvp.Value))));
    }

    private CookieValidatePrincipalContext SetupValidatePrincipalContext(ClaimsPrincipal principal)
    {
        var scheme = new AuthenticationScheme(
            this.modelFactory.NextString("authentication-scheme"),
            null,
            typeof(CookieAuthenticationHandler));
        var ticket = new AuthenticationTicket(principal, scheme.Name);

        return new(new DefaultHttpContext(), scheme, new(), ticket);
    }

    #endregion
}
