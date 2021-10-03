using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Buttercup.Web.TagHelpers
{
    /// <summary>
    /// A tag helper that adds a list item for each non-empty line in a string.
    /// </summary>
    [HtmlTargetElement(Attributes = InputAttributeName)]
    public class ListFromLinesTagHelper : TagHelper
    {
        private const string InputAttributeName = "lines-in";

        /// <summary>
        /// Gets or sets the input string.
        /// </summary>
        /// <value>
        /// The input string.
        /// </value>
        [HtmlAttributeName(InputAttributeName)]
        public string? Input { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrWhiteSpace(this.Input))
            {
                output.SuppressOutput();
                return;
            }

            foreach (var line in this.Input.Split(new[] { '\n', '\r' }))
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    output.Content.AppendHtml("<li>").Append(line).AppendHtml("</li>");
                }
            }
        }
    }
}
