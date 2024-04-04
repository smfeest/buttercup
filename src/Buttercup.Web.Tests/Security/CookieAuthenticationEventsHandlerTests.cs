using System.Globalization;
using System.Security.Claims;
using Buttercup.Application;
using Buttercup.EntityModel;
using Buttercup.Security;
using Buttercup.TestUtils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Moq;
using Xunit;

namespace Buttercup.Web.Security;

public sealed class CookieAuthenticationEventsHandlerTests
{
    private readonly ModelFactory modelFactory = new();

    private readonly Mock<IAuthenticationService> authenticationServiceMock = new();
    private readonly ListLogger<CookieAuthenticationEventsHandler> logger = new();
    private readonly Mock<IUserManager> userManagerMock = new();
    private readonly Mock<IUserPrincipalFactory> userPrincipalFactoryMock = new();

    private readonly CookieAuthenticationEventsHandler cookieAuthenticationEventsHandler;

    public CookieAuthenticationEventsHandlerTests() =>
        this.cookieAuthenticationEventsHandler = new(
            this.authenticationServiceMock.Object,
            this.logger,
            this.userManagerMock.Object,
            this.userPrincipalFactoryMock.Object);

    #region ValidatePrincipal

    [Fact]
    public async Task ValidatePrincipal_Unauthenticated_DoesNotRejectPrincipal()
    {
        var initialPrincipal = new ClaimsPrincipal();
        var context = this.BuildValidatePrincipalContext(initialPrincipal);

        await this.cookieAuthenticationEventsHandler.ValidatePrincipal(context);

        Assert.Same(initialPrincipal, context.Principal);
    }

    [Fact]
    public async Task ValidatePrincipal_UserNoLongerExists_LogsInfoMessage()
    {
        var user = this.modelFactory.BuildUser();
        var context = this.BuildValidatePrincipalContext(BuildPrincipal(user));
        this.SetupFindUser(user.Id, null);

        await this.cookieAuthenticationEventsHandler.ValidatePrincipal(context);

        LogAssert.HasEntry(
            this.logger, LogLevel.Information, 219, $"User {user.Id} no longer exists");
    }

    [Fact]
    public async Task ValidatePrincipal_UserNoLongerExists_RejectsPrincipal()
    {
        var user = this.modelFactory.BuildUser();
        var context = this.BuildValidatePrincipalContext(BuildPrincipal(user));
        this.SetupFindUser(user.Id, null);

        await this.cookieAuthenticationEventsHandler.ValidatePrincipal(context);

        Assert.Null(context.Principal);
    }

    [Fact]
    public async Task ValidatePrincipal_UserNoLongerExists_SignsUserOut()
    {
        var user = this.modelFactory.BuildUser();
        var context = this.BuildValidatePrincipalContext(BuildPrincipal(user));
        this.SetupFindUser(user.Id, null);

        await this.cookieAuthenticationEventsHandler.ValidatePrincipal(context);

        this.authenticationServiceMock.Verify(
            x => x.SignOutAsync(context.HttpContext, context.Scheme.Name, null));
    }

    [Theory]
    [MemberData(nameof(GetTheoryDataForSecurityStampIsMissingOrStale))]
    public async Task ValidatePrincipal_SecurityStampIsMissingOrStale_LogsInfoMessage(
        Func<User, ClaimsPrincipal> principalFactory)
    {
        var user = this.SetupFindUser();
        var context = this.BuildValidatePrincipalContext(principalFactory(user));

        await this.cookieAuthenticationEventsHandler.ValidatePrincipal(context);

        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            214,
            $"Incorrect security stamp for user {user.Id} ({user.Email})");
    }

    [Theory]
    [MemberData(nameof(GetTheoryDataForSecurityStampIsMissingOrStale))]
    public async Task ValidatePrincipal_SecurityStampIsMissingOrStale_RejectsPrincipal(
        Func<User, ClaimsPrincipal> principalFactory)
    {
        var context = this.BuildValidatePrincipalContext(principalFactory(this.SetupFindUser()));

        await this.cookieAuthenticationEventsHandler.ValidatePrincipal(context);

        Assert.Null(context.Principal);
    }

    [Theory]
    [MemberData(nameof(GetTheoryDataForSecurityStampIsMissingOrStale))]
    public async Task ValidatePrincipal_SecurityStampIsMissingOrStale_SignsUserOut(
        Func<User, ClaimsPrincipal> principalFactory)
    {
        var context = this.BuildValidatePrincipalContext(principalFactory(this.SetupFindUser()));

        await this.cookieAuthenticationEventsHandler.ValidatePrincipal(context);

        this.authenticationServiceMock.Verify(
            x => x.SignOutAsync(context.HttpContext, context.Scheme.Name, null));
    }

    [Fact]
    public async Task ValidatePrincipal_SecurityStampMatches_LogsDebugMessage()
    {
        var user = this.SetupFindUser();
        var context = this.BuildValidatePrincipalContext(BuildPrincipal(user));

        await this.cookieAuthenticationEventsHandler.ValidatePrincipal(context);

        LogAssert.HasEntry(
            this.logger,
            LogLevel.Debug,
            215,
            $"Principal successfully validated for user {user.Id} ({user.Email})");
    }

    [Theory]
    [MemberData(nameof(GetTheoryDataForUserRevisionIsMissingOrStale))]
    public async Task ValidatePrincipal_UserRevisionIsMissingOrStale_ReplacesPrincipal(
         Func<User, ClaimsPrincipal> principalFactory)
    {
        var user = this.SetupFindUser();
        var context = this.BuildValidatePrincipalContext(principalFactory(user));
        var updatedPrincipal = this.SetupCreatePrincipal(user, context.Scheme);

        await this.cookieAuthenticationEventsHandler.ValidatePrincipal(context);

        Assert.Same(updatedPrincipal, context.Principal);
    }

    [Theory]
    [MemberData(nameof(GetTheoryDataForUserRevisionIsMissingOrStale))]
    public async Task ValidatePrincipal_UserRevisionIsMissingOrStale_RenewsCookie(
        Func<User, ClaimsPrincipal> principalFactory)
    {
        var context = this.BuildValidatePrincipalContext(principalFactory(this.SetupFindUser()));

        await this.cookieAuthenticationEventsHandler.ValidatePrincipal(context);

        Assert.True(context.ShouldRenew);
    }

    [Theory]
    [MemberData(nameof(GetTheoryDataForUserRevisionIsMissingOrStale))]
    public async Task ValidatePrincipal_UserRevisionIsMissingOrStale_LogsInfoMessage(
        Func<User, ClaimsPrincipal> principalFactory)
    {
        var user = this.SetupFindUser();
        var context = this.BuildValidatePrincipalContext(principalFactory(user));

        await this.cookieAuthenticationEventsHandler.ValidatePrincipal(context);

        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            216,
            $"Refreshed claims principal for user {user.Id} ({user.Email})");
    }

    [Fact]
    public async Task ValidatePrincipal_UserRevisionMatches_RetainsPrincipal()
    {
        var initialPrincipal = BuildPrincipal(this.SetupFindUser());
        var context = this.BuildValidatePrincipalContext(initialPrincipal);

        await this.cookieAuthenticationEventsHandler.ValidatePrincipal(context);

        Assert.Same(initialPrincipal, context.Principal);
    }

    private static ClaimsPrincipal BuildPrincipal(
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

    private CookieValidatePrincipalContext BuildValidatePrincipalContext(ClaimsPrincipal principal)
    {
        var scheme = new AuthenticationScheme(
            this.modelFactory.NextString("authentication-scheme"),
            null,
            typeof(CookieAuthenticationHandler));
        var ticket = new AuthenticationTicket(principal, scheme.Name);

        return new(new DefaultHttpContext(), scheme, new(), ticket);
    }

    public static TheoryData<Func<User, ClaimsPrincipal>> GetTheoryDataForSecurityStampIsMissingOrStale()
    {
        ClaimsPrincipal SetupPrincipalWithoutSecurityStamp(User user) =>
            BuildPrincipal(user, claims => claims.Remove(CustomClaimTypes.SecurityStamp));
        ClaimsPrincipal SetupPrincipalWithStaleSecurityStamp(User user) =>
            BuildPrincipal(user with { SecurityStamp = "stale-security-stamp" });

        return new([SetupPrincipalWithoutSecurityStamp, SetupPrincipalWithStaleSecurityStamp]);
    }

    public static TheoryData<Func<User, ClaimsPrincipal>> GetTheoryDataForUserRevisionIsMissingOrStale()
    {
        ClaimsPrincipal SetupPrincipalWithoutUserRevision(User user) =>
            BuildPrincipal(user, claims => claims.Remove(CustomClaimTypes.UserRevision));
        ClaimsPrincipal SetupPrincipalWithStaleUserRevision(User user) =>
            BuildPrincipal(user with { Revision = user.Revision - 1 });

        return new([SetupPrincipalWithoutUserRevision, SetupPrincipalWithStaleUserRevision]);
    }

    private ClaimsPrincipal SetupCreatePrincipal(User user, AuthenticationScheme scheme)
    {
        var updatedPrincipal = new ClaimsPrincipal();

        this.userPrincipalFactoryMock
            .Setup(x => x.Create(user, scheme.Name))
            .Returns(updatedPrincipal);

        return updatedPrincipal;
    }

    private User SetupFindUser()
    {
        var user = this.modelFactory.BuildUser();
        this.SetupFindUser(user.Id, user);
        return user;
    }

    private void SetupFindUser(long id, User? user) =>
        this.userManagerMock.Setup(x => x.FindUser(id)).ReturnsAsync(user);

    #endregion
}
