using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Buttercup.Models;
using Buttercup.Web.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Xunit;

namespace Buttercup.Web.TagHelpers
{
    public class UserDateTimeTagHelperTests
    {
        [Fact]
        public void SuppressesOutputWhenDateTimeIsNull()
        {
            var context = new Context();

            context.TagHelper.DateTime = null;

            context.Process();

            Assert.Null(context.Output.TagName);
            Assert.True(context.Output.Content.IsEmptyOrWhiteSpace);
        }

        [Fact]
        public void SetsTagNameToSpan()
        {
            var context = new Context();

            context.Process();

            Assert.Equal("span", context.Output.TagName);
        }

        [Fact]
        public void SetsContentToFormattedUserDateAndTime()
        {
            var context = new Context();

            context.TagHelper.Format = "D";

            context.Process();

            Assert.Equal(
                context.UserDateTime.ToString("D", CultureInfo.CurrentCulture),
                context.Output.Content.GetContent());
        }

        [Fact]
        public void UsesGeneralDateLongTimeFormatByDefault()
        {
            var context = new Context();

            context.TagHelper.Format = null;

            context.Process();

            Assert.Equal(
                context.UserDateTime.ToString("G", CultureInfo.CurrentCulture),
                context.Output.Content.GetContent());
        }

        [Fact]
        public void SetsTitleAttributeToUtcDateTime()
        {
            var context = new Context();

            context.Process();

            Assert.Equal("2001-02-03 21:22:23Z", (string)context.Output.Attributes["Title"].Value);
        }

        private class Context
        {
            public Context()
            {
                var httpContext = new DefaultHttpContext();

                httpContext.SetCurrentUser(new() { TimeZone = "Etc/GMT-5" });

                this.TagHelper = new()
                {
                    DateTime = new(2001, 2, 3, 21, 22, 23, DateTimeKind.Utc),
                    ViewContext = new() { HttpContext = httpContext },
                };
            }

            public TagHelperOutput Output { get; } = new("user-date-time", new(), GetChildContent);

            public UserDateTimeTagHelper TagHelper { get; }

            public DateTimeOffset UserDateTime =>
                new DateTimeOffset(this.TagHelper.DateTime.Value).ToOffset(new(5, 0, 0));

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
}
