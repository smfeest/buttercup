using System.Security.Claims;
using Xunit;

namespace Buttercup.Security;

public sealed class ClaimsPrincipalExtensionsTests
{
    #region GetUserId

    [Fact]
    public void GetUserId_ReturnsParsedNameIdentifier()
    {
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity([new(ClaimTypes.NameIdentifier, "7214")]));

        Assert.Equal(7214, principal.GetUserId());
    }

    [Fact]
    public void GetUserId_ThrowsWhenNameIdentifierIsMissing()
    {
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity([new(ClaimTypes.Email, "user@example.com")]));

        Assert.Throws<InvalidOperationException>(() => principal.GetUserId());
    }

    #endregion

    #region HasUserId

    [Theory]
    [InlineData(123, true)]
    [InlineData(456, false)]
    [InlineData(789, true)]
    public void HasUserId_ChecksForMatchingNameIdentifierClaim(long userId, bool expectedResult)
    {
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                [
                    new(ClaimTypes.NameIdentifier, "123"),
                    new(ClaimTypes.SerialNumber, "456"),
                    new(ClaimTypes.NameIdentifier, "789")
                ]));
        Assert.Equal(expectedResult, principal.HasUserId(userId));
    }

    #endregion

    #region TryGetUserId

    [Fact]
    public void TryGetUserId_ReturnsParsedNameIdentifier()
    {
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity([new(ClaimTypes.NameIdentifier, "7214")]));

        Assert.Equal(7214, principal.TryGetUserId());
    }

    [Fact]
    public void TryGetUserId_ReturnsNullWhenNameIdentifierIsMissing()
    {
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity([new(ClaimTypes.Email, "user@example.com")]));

        Assert.Null(principal.TryGetUserId());
    }

    #endregion
}
