using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Xunit;

namespace Buttercup.Web.TagHelpers
{
    public class ListFromLinesTagHelperTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData(" \n \t\n")]
        public void SuppressesOutputWhenListIsEmpty(string input)
        {
            var context = new Context();

            context.TagHelper.Input = input;

            context.Process();

            Assert.Null(context.Output.TagName);
            Assert.True(context.Output.Content.IsEmptyOrWhiteSpace);
        }

        [Fact]
        public void InsertsListItemForEachNonEmptyLine()
        {
            var context = new Context();

            context.TagHelper.Input = " \nAlpha\r\nBeta & Gamma\n  \nDelta";

            context.Process();

            Assert.Equal("ul", context.Output.TagName);
            Assert.Equal(
                "<li>Alpha</li><li>Beta &amp; Gamma</li><li>Delta</li>",
                context.Output.Content.GetContent());
        }

        private class Context
        {
            public Context()
            {
                this.Output = new TagHelperOutput(
                    "ul", new TagHelperAttributeList(), GetChildContent);

                this.TagHelper = new ListFromLinesTagHelper();
            }

            public TagHelperOutput Output { get; }

            public ListFromLinesTagHelper TagHelper { get; }

            public void Process()
            {
                var context = new TagHelperContext(
                    "ul", new TagHelperAttributeList(), new Dictionary<object, object>(), "test");

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
