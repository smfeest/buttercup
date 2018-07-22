using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Buttercup.Web.TagHelpers
{
    /// <summary>
    /// A tag helper that adds the required suffix to the label for a required field.
    /// </summary>
    /// <remarks>
    /// The tag helper is intended to be used in conjunction with <see cref="LabelTagHelper" />.
    /// </remarks>
    [HtmlTargetElement("label", Attributes = ForAttributeName)]
    public class RequiredLabelTagHelper : TagHelper
    {
        [SuppressMessage("Microsoft.Performance", "CA1823", Justification = "Used in attributes")]
        private const string ForAttributeName = "asp-for";

        /// <summary>
        /// Gets or sets the model expression.
        /// </summary>
        /// <value>
        /// The model expression.
        /// </value>
        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (this.For.Metadata.IsRequired)
            {
                var span = new TagBuilder("span");
                span.AddCssClass("form-field__required-label");
                span.InnerHtml.Append("required");

                output.Content.AppendHtml(span);
            }
        }
    }
}
