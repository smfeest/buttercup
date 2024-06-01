using Markdig;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Buttercup.Web.HtmlHelpers;

public static class MarkdownExtensions
{
    /// <summary>
    /// Renders markdown as HTML.
    /// </summary>
    /// <param name="helper">
    /// The HTML helper.
    /// </param>
    /// <param name="markdown">
    /// The markdown text.
    /// </param>
    /// <param name="pipeline">
    /// The markdown pipeline to use.
    /// </param>
    /// <returns>
    /// The markdown rendered as HTML.
    /// </returns>
    public static IHtmlContent FromMarkdown(
        this IHtmlHelper helper, string markdown, MarkdownPipeline pipeline) =>
        helper.Raw(Markdown.ToHtml(markdown, pipeline));
}
