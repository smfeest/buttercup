using System.Globalization;
using System.Security.Claims;
using Buttercup.Security;
using Buttercup.TestUtils;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.WebEncoders.Testing;
using Moq;
using Xunit;

namespace Buttercup.Web.HtmlHelpers;

public sealed class UserDateTimeExtensionsTests
{
    private readonly ModelFactory modelFactory = new();
    private readonly IHtmlHelper htmlHelper;

    public UserDateTimeExtensionsTests()
    {
        var identity = new ClaimsIdentity([new Claim(CustomClaimTypes.TimeZone, "Etc/GMT-5")]);

        var viewContext = new ViewContext()
        {
            HttpContext = new DefaultHttpContext { User = new(identity) }
        };

        this.htmlHelper = Mock.Of<IHtmlHelper>(x => x.ViewContext == viewContext);
    }

    [Fact]
    public void UserDateTime_ReturnsTimeTag()
    {
        var output = this.htmlHelper.UserDateTime(this.modelFactory.NextDateTime());

        var builder = Assert.IsType<TagBuilder>(output);
        Assert.Equal("time", builder.TagName);
    }

    [Fact]
    public void UserDateTime_SetsTagContentToFormattedUserDateAndTime()
    {
        var utcDateTime = this.modelFactory.NextDateTime();

        var output = this.htmlHelper.UserDateTime(utcDateTime, "D");

        var builder = Assert.IsType<TagBuilder>(output);
        Assert.Equal(
            $"HtmlEncode[[{ConvertToUserTimeZone(utcDateTime):D}]]", GetTagContent(builder));
    }

    [Fact]
    public void UserDateTime_UsesGeneralDateLongTimeFormatByDefault()
    {
        var utcDateTime = this.modelFactory.NextDateTime();

        var output = this.htmlHelper.UserDateTime(utcDateTime);

        var builder = Assert.IsType<TagBuilder>(output);
        Assert.Equal(
            $"HtmlEncode[[{ConvertToUserTimeZone(utcDateTime):G}]]", GetTagContent(builder));
    }

    [Fact]
    public void UserDateTime_SetsTitleAttributeToUtcDateTime()
    {
        var utcDateTime = this.modelFactory.NextDateTime();

        var output = this.htmlHelper.UserDateTime(utcDateTime);

        var builder = Assert.IsType<TagBuilder>(output);
        Assert.Equal(
            utcDateTime.ToString("u", CultureInfo.CurrentCulture),
            builder.Attributes["title"]);
    }

    private static DateTimeOffset ConvertToUserTimeZone(DateTime dateTime) =>
        new DateTimeOffset(dateTime).ToOffset(new(5, 0, 0));

    private static string GetTagContent(TagBuilder tagBuilder)
    {
        using var stringWriter = new StringWriter();
        tagBuilder.InnerHtml.WriteTo(stringWriter, new HtmlTestEncoder());
        return stringWriter.ToString();
    }
}
