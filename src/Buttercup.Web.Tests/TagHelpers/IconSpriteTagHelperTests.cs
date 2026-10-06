using System.Text.Encodings.Web;
using Buttercup.Web.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;
using Xunit;

namespace Buttercup.Web.TagHelpers;

public sealed class IconSpriteTagHelperTests
{
    [Theory]
    [InlineData(TagMode.SelfClosing)]
    [InlineData(TagMode.StartTagOnly)]
    public async Task OutputsDivElementContainingSprite(TagMode initialTagMode)
    {
        var iconSpriteProviderMock = new Mock<IIconSpriteProvider>();

        iconSpriteProviderMock
            .Setup(x => x.GetSprite("foo"))
            .ReturnsAsync("<svg>[sprite foo]</svg>");

        var tagHelper = new IconSpriteTagHelper(iconSpriteProviderMock.Object)
        {
            Name = "foo",
        };
        var context = new TagHelperContext(
            "icon-sprite", [], new Dictionary<object, object>(), "test");
        var output = new TagHelperOutput(
            "icon-sprite",
            [],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()))
        {
            TagMode = initialTagMode,
        };

        await tagHelper.ProcessAsync(context, output);

        using var writer = new StringWriter();
        output.WriteTo(writer, HtmlEncoder.Default);

        Assert.Equal(
            "<div class=\"icon-sprite\"><svg>[sprite foo]</svg></div>",
            writer.ToString());
    }
}
