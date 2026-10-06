using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Buttercup.Web.TagHelpers;

[HtmlTargetElement("icon", TagStructure = TagStructure.WithoutEndTag)]
public sealed partial class IconTagHelper(HtmlEncoder htmlEncoder) : TagHelper
{
    public required string Name { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "svg";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("icon", htmlEncoder);
        output.AddClass($"icon--{this.Name}", htmlEncoder);
        output.Attributes.Add("aria-hidden", "true");

        var useTag = new TagBuilder("use")
        {
            TagRenderMode = TagRenderMode.SelfClosing
        };
        useTag.Attributes.Add("href", $"#icon-{this.Name}");
        output.Content.AppendHtml(useTag);
    }
}
