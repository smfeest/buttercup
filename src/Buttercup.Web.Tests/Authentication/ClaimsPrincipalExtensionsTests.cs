using System.Security.Claims;
using Xunit;

namespace Buttercup.Web.Authentication;

public sealed class ClaimsPrincipalExtensionsTests
{
    #region GetUserId

    [Fact]
    public void GetUserIdReturnsParsedNameIdentifier()
    {
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(new Claim[] { new(ClaimTypes.NameIdentifier, "7214") }));

        Assert.Equal(7214, principal.GetUserId());
    }

    [Fact]
    public void GetUserIdReturnsNullWhenNameIdentifierIsMissing()
    {
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(new Claim[] { new(ClaimTypes.Email, "user@example.com") }));

        Assert.Null(principal.GetUserId());
    }

    #endregion
}
