using Buttercup.Web.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Buttercup.Web.TagHelpers;

[HtmlTargetElement("icon-sprite", TagStructure = TagStructure.WithoutEndTag)]
public sealed partial class IconSpriteTagHelper(IIconSpriteProvider iconSpriteProvider) : TagHelper
{
    private readonly IIconSpriteProvider iconSpriteProvider = iconSpriteProvider;

    public required string Name { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", "icon-sprite");
        output.Content.SetHtmlContent(await this.iconSpriteProvider.GetSprite(this.Name));
    }
}
