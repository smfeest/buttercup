using System.Globalization;
using System.Security.Claims;
using Buttercup.Models;
using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.Web.Authentication;

public class UserPrincipalFactoryTests
{
    private const string AuthenticationType = "sample-authentication-type";

    private readonly User user = ModelFactory.CreateUser();

    [Fact]
    public void PrincipalHasNameIdClaim() => Assert.Equal(
        this.user.Id.ToString(CultureInfo.InvariantCulture),
        this.CreatePrincipal().FindFirstValue(ClaimTypes.NameIdentifier));

    [Fact]
    public void PrincipalHasEmailClaim() =>
        Assert.Equal(this.user.Email, this.CreatePrincipal().FindFirstValue(ClaimTypes.Email));

    [Fact]
    public void PrincipalHasEmailAsIdentityName() =>
        Assert.Equal(this.user.Email, this.CreatePrincipal().Identity!.Name);

    [Fact]
    public void PrincipalHasAuthenticationType() =>
        Assert.Equal(AuthenticationType, this.CreatePrincipal().Identity!.AuthenticationType);

    private ClaimsPrincipal CreatePrincipal() =>
        new UserPrincipalFactory().Create(this.user, AuthenticationType);
}
