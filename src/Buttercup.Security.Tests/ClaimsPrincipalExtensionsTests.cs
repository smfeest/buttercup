using System.Security.Claims;
using Xunit;

namespace Buttercup.Security;

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
    public void GetUserIdThrowsWhenNameIdentifierIsMissing()
    {
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(new Claim[] { new(ClaimTypes.Email, "user@example.com") }));

        Assert.Throws<InvalidOperationException>(() => principal.GetUserId());
    }

    #endregion

    #region TryGetUserId

    [Fact]
    public void TryGetUserIdReturnsParsedNameIdentifier()
    {
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(new Claim[] { new(ClaimTypes.NameIdentifier, "7214") }));

        Assert.Equal(7214, principal.TryGetUserId());
    }

    [Fact]
    public void TryGetUserIdReturnsNullWhenNameIdentifierIsMissing()
    {
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(new Claim[] { new(ClaimTypes.Email, "user@example.com") }));

        Assert.Null(principal.TryGetUserId());
    }

    #endregion
}
