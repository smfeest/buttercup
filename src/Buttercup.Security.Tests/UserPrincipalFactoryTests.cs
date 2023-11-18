using System.Globalization;
using System.Security.Claims;
using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.Security;

public sealed class UserPrincipalFactoryTests
{
    private const string AuthenticationType = "sample-authentication-type";

    private readonly ModelFactory modelFactory = new();

    [Fact]
    public void PrincipalHasNameIdClaim()
    {
        var user = this.modelFactory.BuildUser();
        Assert.Equal(
            user.Id.ToString(CultureInfo.InvariantCulture),
            this.CreatePrincipal(user).FindFirstValue(ClaimTypes.NameIdentifier));
    }

    [Fact]
    public void PrincipalHasNameClaim()
    {
        var user = this.modelFactory.BuildUser();
        Assert.Equal(user.Name, this.CreatePrincipal(user).FindFirstValue(ClaimTypes.Name));
    }

    [Fact]
    public void PrincipalHasEmailClaim()
    {
        var user = this.modelFactory.BuildUser();
        Assert.Equal(user.Email, this.CreatePrincipal(user).FindFirstValue(ClaimTypes.Email));
    }

    [Fact]
    public void PrincipalHasSecurityStampClaim()
    {
        var user = this.modelFactory.BuildUser();
        Assert.Equal(
            user.SecurityStamp,
            this.CreatePrincipal(user).FindFirstValue(CustomClaimTypes.SecurityStamp));
    }

    [Fact]
    public void PrincipalHasTimeZoneClaim()
    {
        var user = this.modelFactory.BuildUser();
        Assert.Equal(
            user.TimeZone, this.CreatePrincipal(user).FindFirstValue(CustomClaimTypes.TimeZone));
    }

    [Fact]
    public void PrincipalHasRevisionClaim()
    {
        var user = this.modelFactory.BuildUser();
        Assert.Equal(
            user.Revision.ToString(CultureInfo.InvariantCulture),
            this.CreatePrincipal(user).FindFirstValue(CustomClaimTypes.UserRevision));
    }

    [Fact]
    public void PrincipalHasAuthenticationType()
    {
        var identity = this.CreatePrincipal(this.modelFactory.BuildUser()).Identity;
        Assert.NotNull(identity);
        Assert.Equal(AuthenticationType, identity.AuthenticationType);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PrincipalHasAdminRoleIfUserIsAdmin(bool userIsAdmin)
    {
        var user = this.modelFactory.BuildUser() with { IsAdmin = userIsAdmin };
        Assert.Equal(userIsAdmin, this.CreatePrincipal(user).IsInRole(RoleNames.Admin));
    }

    [Fact]
    public void IdentityGetsNameFromNameClaim()
    {
        var user = this.modelFactory.BuildUser();
        var identity = this.CreatePrincipal(user).Identity;
        Assert.NotNull(identity);
        Assert.Equal(user.Name, identity.Name);
    }

    private ClaimsPrincipal CreatePrincipal(User user) =>
        new UserPrincipalFactory().Create(user, AuthenticationType);
}
