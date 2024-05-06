using System.Globalization;
using System.Security.Claims;
using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.Security;

public sealed class ClaimsIdentityFactoryTests
{
    private const string AuthenticationType = "sample-authentication-type";

    private readonly ModelFactory modelFactory = new();

    #region CreateIdentityForUser

    [Fact]
    public void CreateIdentityForUser_SetsNameIdClaim()
    {
        var user = this.modelFactory.BuildUser();
        Assert.True(
            CreateIdentityForUser(user).HasClaim(
                ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)));
    }

    [Fact]
    public void CreateIdentityForUser_SetsNameClaim()
    {
        var user = this.modelFactory.BuildUser();
        Assert.True(CreateIdentityForUser(user).HasClaim(ClaimTypes.Name, user.Name));
    }

    [Fact]
    public void CreateIdentityForUser_SetsEmailClaim()
    {
        var user = this.modelFactory.BuildUser();
        Assert.True(CreateIdentityForUser(user).HasClaim(ClaimTypes.Email, user.Email));
    }

    [Fact]
    public void CreateIdentityForUser_SetsSecurityStampClaim()
    {
        var user = this.modelFactory.BuildUser();
        Assert.True(
            CreateIdentityForUser(user).HasClaim(
                CustomClaimTypes.SecurityStamp, user.SecurityStamp));
    }

    [Fact]
    public void CreateIdentityForUser_SetsTimeZoneClaim()
    {
        var user = this.modelFactory.BuildUser();
        Assert.True(
            CreateIdentityForUser(user).HasClaim(CustomClaimTypes.TimeZone, user.TimeZone));
    }

    [Fact]
    public void CreateIdentityForUser_SetsRevisionClaim()
    {
        var user = this.modelFactory.BuildUser();
        Assert.True(
            CreateIdentityForUser(user).HasClaim(
                CustomClaimTypes.UserRevision,
                user.Revision.ToString(CultureInfo.InvariantCulture)));
    }

    [Fact]
    public void CreateIdentityForUser_SetsAuthenticationType()
    {
        var identity = CreateIdentityForUser(this.modelFactory.BuildUser());
        Assert.Equal(AuthenticationType, identity.AuthenticationType);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void IdentityHasAdminRoleIfUserIsAdmin(bool userIsAdmin)
    {
        var user = this.modelFactory.BuildUser() with { IsAdmin = userIsAdmin };
        Assert.Equal(
            userIsAdmin, CreateIdentityForUser(user).HasClaim(ClaimTypes.Role, RoleNames.Admin));
    }

    private static ClaimsIdentity CreateIdentityForUser(User user) =>
        new ClaimsIdentityFactory().CreateIdentityForUser(user, AuthenticationType);

    #endregion
}
