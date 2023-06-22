using System.Security.Claims;
using Buttercup.Security;
using Xunit;

namespace Buttercup.Web.Localization;

public sealed class HttpContextExtensionsTests
{
    #region ToUserTime

    [Fact]
    public void ToUserTimeReturnsTimeInUserTimeZone()
    {
        var identity = new ClaimsIdentity(
            new Claim[] { new(CustomClaimTypes.TimeZone, "Etc/GMT+10") });

        var httpContext = new DefaultHttpContext { User = new(identity) };

        var utcDateTime = new DateTime(2010, 11, 12, 13, 14, 15, DateTimeKind.Utc);

        var userDateTime = httpContext.ToUserTime(utcDateTime);

        Assert.Equal(utcDateTime, userDateTime.UtcDateTime);
        Assert.Equal(new TimeSpan(-10, 0, 0), userDateTime.Offset);
    }

    [Fact]
    public void ToUserTimeReturnsUtcTimeWhenUnauthenticated()
    {
        var httpContext = new DefaultHttpContext();

        var utcDateTime = new DateTime(2010, 11, 12, 13, 14, 15, DateTimeKind.Utc);

        var userDateTime = httpContext.ToUserTime(utcDateTime);

        Assert.Equal(utcDateTime, userDateTime.UtcDateTime);
        Assert.Equal(TimeSpan.Zero, userDateTime.Offset);
    }

    [Theory]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void ToUserTimeThrowsWhenDateTimeKindIsNotUtc(DateTimeKind kind) =>
        Assert.Throws<ArgumentException>(() => new DefaultHttpContext().ToUserTime(new(0, kind)));

    #endregion
}
