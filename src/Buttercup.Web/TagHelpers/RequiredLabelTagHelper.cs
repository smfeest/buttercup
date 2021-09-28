using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;

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

        public RequiredLabelTagHelper(IStringLocalizer<RequiredLabelTagHelper> localizer) =>
            this.Localizer = localizer;

        /// <summary>
        /// Gets or sets the model expression.
        /// </summary>
        /// <value>
        /// The model expression.
        /// </value>
        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression? For { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the required suffix should be added to the field
        /// label.
        /// </summary>
        /// <value>
        /// <b>true</b> if the required suffix should be added to the field label; <b>false</b> if
        /// it shouldn't; <b>null</b> if the suffix should be added if and only if the model value
        /// identified by <see cref="For" /> is required. Default is <b>null</b>.
        /// </value>
        [HtmlAttributeName("show-required-label")]
        public bool? ShowRequiredLabel { get; set; }

        private IStringLocalizer<RequiredLabelTagHelper> Localizer { get; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (this.ShowRequiredLabel.HasValue ?
                this.ShowRequiredLabel.Value :
                this.For!.Metadata.IsRequired)
            {
                var span = new TagBuilder("span");
                span.AddCssClass("form-field__required-label");
                span.InnerHtml.Append(this.Localizer["Label_Required"]!);

                output.Content.AppendHtml(span);
            }
        }
    }
}
