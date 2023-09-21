using System.Globalization;
using System.Security.Claims;
using Buttercup.Security;
using Buttercup.TestUtils;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Xunit;

namespace Buttercup.Web.TagHelpers;

public sealed class UserDateTimeTagHelperTests
{
    private readonly ModelFactory modelFactory = new();

    private readonly UserDateTimeTagHelper tagHelper;

    public UserDateTimeTagHelperTests()
    {
        var identity = new ClaimsIdentity(
            new Claim[] { new(CustomClaimTypes.TimeZone, "Etc/GMT-5") });

        this.tagHelper = new()
        {
            ViewContext = new() { HttpContext = new DefaultHttpContext { User = new(identity) } },
        };
    }

    [Fact]
    public void DateTimeNull_SuppressesOutput()
    {
        this.tagHelper.DateTime = null;

        var output = this.Process();

        Assert.Null(output.TagName);
        Assert.True(output.Content.IsEmptyOrWhiteSpace);
    }

    [Fact]
    public void DateTimeNotNull_SetsTagNameToSpan()
    {
        this.tagHelper.DateTime = this.modelFactory.NextDateTime();

        var output = this.Process();

        Assert.Equal("span", output.TagName);
    }

    [Fact]
    public void DateTimeNotNull_SetsContentToFormattedUserDateAndTime()
    {
        var utcDateTime = this.modelFactory.NextDateTime();

        this.tagHelper.DateTime = utcDateTime;
        this.tagHelper.Format = "D";

        var output = this.Process();

        Assert.Equal(
            ConvertToUserTimeZone(utcDateTime).ToString("D", CultureInfo.CurrentCulture),
            output.Content.GetContent());
    }

    [Fact]
    public void DateTimeNotNull_UsesGeneralDateLongTimeFormatByDefault()
    {
        var utcDateTime = this.modelFactory.NextDateTime();

        this.tagHelper.DateTime = utcDateTime;
        this.tagHelper.Format = null;

        var output = this.Process();

        Assert.Equal(
            ConvertToUserTimeZone(utcDateTime).ToString("G", CultureInfo.CurrentCulture),
            output.Content.GetContent());
    }

    [Fact]
    public void DateTimeNotNull_SetsTitleAttributeToUtcDateTime()
    {
        var utcDateTime = this.modelFactory.NextDateTime();

        this.tagHelper.DateTime = utcDateTime;

        var output = this.Process();

        Assert.Equal(
            utcDateTime.ToString("u", CultureInfo.CurrentCulture),
            (string)output.Attributes["Title"].Value);
    }

    private static DateTimeOffset ConvertToUserTimeZone(DateTime dateTime) =>
        new DateTimeOffset(dateTime).ToOffset(new(5, 0, 0));

    private TagHelperOutput Process()
    {
        var context = new TagHelperContext(
            "user-date-time",
            new(),
            new Dictionary<object, object>(),
            "test");
        var output = new TagHelperOutput(
            "user-date-time",
            new(),
            (_, _) => Task.FromResult(new DefaultTagHelperContent().SetContent("content")));

        this.tagHelper.Process(context, output);

        return output;
    }
}
