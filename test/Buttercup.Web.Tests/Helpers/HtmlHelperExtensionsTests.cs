using System;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using Xunit;

namespace Buttercup.Web.Helpers
{
    public class HtmlHelperExtensionsTests
    {
        #region FormatAsHoursAndMinutes

        [Theory]
        [InlineData(0, "0 minutes")]
        [InlineData(1, "1 minute")]
        [InlineData(59, "59 minutes")]
        [InlineData(60, "1 hour")]
        [InlineData(120, "2 hours")]
        [InlineData(121, "2 hours 1 minute")]
        [InlineData(125, "2 hours 5 minutes")]
        [InlineData(1500, "25 hours")]
        public void FormatAsHoursAndMinutesReturnsHoursAndMinutesInWords(
            int minutes, string expectedOutput)
        {
            var mockHtmlHelper = new Mock<IHtmlHelper>();

            mockHtmlHelper
                .Setup(x => x.FormatValue(It.IsAny<int>(), It.IsAny<string>()))
                .Returns((int value, string format) =>
                    string.Format(CultureInfo.InvariantCulture, format, value));

            Assert.Equal(expectedOutput, mockHtmlHelper.Object.FormatAsHoursAndMinutes(minutes));
        }

        #endregion
    }
}
