using Markdig;
using Xunit;

namespace Buttercup.Web.HtmlHelpers;

public sealed class MarkdownPipelinesTests
{
    #region Comments

    [Fact]
    public void Comments_EscapesHtmlTags()
    {
        var output = Markdown.ToHtml("<script>evil();</script>", MarkdownPipelines.Comments);
        Assert.Contains("&lt;script&gt;evil();&lt;/script&gt;", output);
    }

    [Theory]
    [InlineData("## Hello")]
    [InlineData("Hello\n---")]
    public void Comments_DisablesHeadings(string input)
    {
        var output = Markdown.ToHtml(input, MarkdownPipelines.Comments);
        Assert.Contains(input, output);
    }

    [Fact]
    public void Comments_EnablesAutoLinks()
    {
        var output = Markdown.ToHtml("Visit https://example.com", MarkdownPipelines.Comments);
        Assert.Contains("Visit <a href=\"https://example.com\">https://example.com</a>", output);
    }

    #endregion
}
