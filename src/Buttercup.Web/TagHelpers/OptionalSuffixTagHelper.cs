using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;

namespace Buttercup.Web.TagHelpers;

/// <summary>
/// A tag helper that adds the optional suffix to the label for an optional field.
/// </summary>
/// <remarks>
/// The tag helper is intended to be used in conjunction with <see cref="LabelTagHelper" />.
/// </remarks>
[HtmlTargetElement("label", Attributes = ForAttributeName)]
public class OptionalSuffixTagHelper : TagHelper
{
    private const string ForAttributeName = "asp-for";

    private readonly IStringLocalizer<OptionalSuffixTagHelper> localizer;

    public OptionalSuffixTagHelper(IStringLocalizer<OptionalSuffixTagHelper> localizer) =>
        this.localizer = localizer;

    /// <summary>
    /// Gets or sets the model expression.
    /// </summary>
    /// <value>
    /// The model expression.
    /// </value>
    [HtmlAttributeName(ForAttributeName)]
    public required ModelExpression For { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (!this.For.Metadata.IsRequired)
        {
            var span = new TagBuilder("span");
            span.AddCssClass("form-field__optional-label");
            span.InnerHtml.Append(this.localizer["Label_Optional"]!);

            output.Content.AppendHtml(span);
        }
    }
}
