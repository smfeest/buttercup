using Markdig;

namespace Buttercup.Web.HtmlHelpers;

/// <summary>
/// Provides markdown pipelines for specific uses.
/// </summary>
public static class MarkdownPipelines
{
    /// <summary>
    /// Gets the markdown pipeline for rendering comments.
    /// </summary>
    /// <remarks>
    /// HTML tags are escaped and rendered as literal strings to prevent XSS attacks, headings are
    /// disabled to enforce a single font size, and auto links are enabled to simplify linking.
    /// </remarks>
    public static MarkdownPipeline Comments { get; } =
        new MarkdownPipelineBuilder().DisableHtml().DisableHeadings().UseAutoLinks().Build();
}
