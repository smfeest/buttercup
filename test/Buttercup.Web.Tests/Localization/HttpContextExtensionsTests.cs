using System;
using Buttercup.Models;
using Buttercup.Web.Authentication;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Buttercup.Web.Localization
{
    public class HttpContextExtensionsTests
    {
        #region ToUserTime

        [Fact]
        public void ToUserTimeReturnsTimeInUserTimeZone()
        {
            var httpContext = new DefaultHttpContext();

            httpContext.SetCurrentUser(new User { TimeZone = "Etc/GMT+10" });

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
        public void ToUserTimeThrowsWhenDateTimeKindIsNotUtc(DateTimeKind kind)
        {
            Assert.Throws<ArgumentException>(
                () => new DefaultHttpContext().ToUserTime(new DateTime(0, kind)));
        }

        #endregion
    }
}
