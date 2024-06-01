using Markdig;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.WebEncoders.Testing;
using Moq;
using Xunit;

namespace Buttercup.Web.HtmlHelpers;

public sealed class MarkdownExtensionsTests
{
    [Fact]
    public void FromMarkdown_RendersMarkdownAsHtml()
    {
        var htmlHelperMock = new Mock<IHtmlHelper>();
        htmlHelperMock
            .Setup(x => x.Raw(It.IsAny<string>()))
            .Returns((string input) => new HtmlString(input));

        using var stringWriter = new StringWriter();

        var pipeline = new MarkdownPipelineBuilder().Build();
        htmlHelperMock.Object.FromMarkdown("Hello _world_", pipeline).WriteTo(
            stringWriter, new HtmlTestEncoder());

        Assert.Equal("<p>Hello <em>world</em></p>\n", stringWriter.ToString());
    }
}
