using System.Globalization;
using System.Text.Encodings.Web;
using Buttercup.TestUtils;
using Buttercup.Web.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Xunit;

namespace Buttercup.Web.TagHelpers;

public class UserDateTimeTagHelperTests
{
    [Fact]
    public void SuppressesOutputWhenDateTimeIsNull()
    {
        var fixture = new UserDateTimeTagHelperFixture();

        fixture.TagHelper.DateTime = null;

        fixture.Process();

        Assert.Null(fixture.Output.TagName);
        Assert.True(fixture.Output.Content.IsEmptyOrWhiteSpace);
    }

    [Fact]
    public void SetsTagNameToSpan()
    {
        var fixture = new UserDateTimeTagHelperFixture();

        fixture.Process();

        Assert.Equal("span", fixture.Output.TagName);
    }

    [Fact]
    public void SetsContentToFormattedUserDateAndTime()
    {
        var fixture = new UserDateTimeTagHelperFixture();

        fixture.TagHelper.Format = "D";

        fixture.Process();

        Assert.Equal(
            fixture.UserDateTime.ToString("D", CultureInfo.CurrentCulture),
            fixture.Output.Content.GetContent());
    }

    [Fact]
    public void UsesGeneralDateLongTimeFormatByDefault()
    {
        var fixture = new UserDateTimeTagHelperFixture();

        fixture.TagHelper.Format = null;

        fixture.Process();

        Assert.Equal(
            fixture.UserDateTime.ToString("G", CultureInfo.CurrentCulture),
            fixture.Output.Content.GetContent());
    }

    [Fact]
    public void SetsTitleAttributeToUtcDateTime()
    {
        var fixture = new UserDateTimeTagHelperFixture();

        fixture.Process();

        Assert.Equal("2001-02-03 21:22:23Z", (string)fixture.Output.Attributes["Title"].Value);
    }

    private sealed class UserDateTimeTagHelperFixture
    {
        public UserDateTimeTagHelperFixture()
        {
            var httpContext = new DefaultHttpContext();

            httpContext.SetCurrentUser(
                new ModelFactory().BuildUser() with { TimeZone = "Etc/GMT-5" });

            this.TagHelper = new()
            {
                DateTime = this.UtcDateTime,
                ViewContext = new() { HttpContext = httpContext },
            };
        }

        public TagHelperOutput Output { get; } = new("user-date-time", new(), GetChildContent);

        public UserDateTimeTagHelper TagHelper { get; }

        public DateTime UtcDateTime { get; } = new(2001, 2, 3, 21, 22, 23, DateTimeKind.Utc);

        public DateTimeOffset UserDateTime =>
            new DateTimeOffset(this.UtcDateTime).ToOffset(new(5, 0, 0));

        public void Process()
        {
            var context = new TagHelperContext(
                "user-date-time",
                new(),
                new Dictionary<object, object>(),
                "test");

            this.TagHelper.Process(context, this.Output);
        }

        private static Task<TagHelperContent> GetChildContent(
            bool useCachedResult, HtmlEncoder encoder)
        {
            TagHelperContent content = new DefaultTagHelperContent();
            content.SetContent("test-content");
            return Task.FromResult(content);
        }
    }
}
