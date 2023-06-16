using System.Globalization;
using System.Security.Claims;
using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.Security;

public sealed class UserPrincipalFactoryTests
{
    private const string AuthenticationType = "sample-authentication-type";

    private readonly User user = new ModelFactory().BuildUser();

    [Fact]
    public void PrincipalHasNameIdClaim() => Assert.Equal(
        this.user.Id.ToString(CultureInfo.InvariantCulture),
        this.CreatePrincipal().FindFirstValue(ClaimTypes.NameIdentifier));

    [Fact]
    public void PrincipalHasEmailClaim() =>
        Assert.Equal(this.user.Email, this.CreatePrincipal().FindFirstValue(ClaimTypes.Email));

    [Fact]
    public void PrincipalHasEmailAsIdentityName()
    {
        var identity = this.CreatePrincipal().Identity;
        Assert.NotNull(identity);
        Assert.Equal(this.user.Email, identity.Name);
    }

    [Fact]
    public void PrincipalHasSecurityStampClaim() =>
        Assert.Equal(
            this.user.SecurityStamp,
            this.CreatePrincipal().FindFirstValue(CustomClaimTypes.SecurityStamp));

    [Fact]
    public void PrincipalHasAuthenticationType()
    {
        var identity = this.CreatePrincipal().Identity;
        Assert.NotNull(identity);
        Assert.Equal(AuthenticationType, identity.AuthenticationType);
    }

    private ClaimsPrincipal CreatePrincipal() =>
        new UserPrincipalFactory().Create(this.user, AuthenticationType);
}
