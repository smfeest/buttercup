using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Xunit;

namespace Buttercup.Web.TagHelpers;

public sealed class IconTagHelperTests
{
    [Theory]
    [InlineData(TagMode.SelfClosing)]
    [InlineData(TagMode.StartTagOnly)]
    public async Task OutputsSvgElementContainingIconReference(TagMode initialTagMode)
    {
        var tagHelper = new IconTagHelper(HtmlEncoder.Default) { Name = "foo-bar" };

        var output = await Render(tagHelper, [], initialTagMode);

        Assert.Equal(
            "<svg class=\"icon icon--foo-bar\" aria-hidden=\"true\"><use href=\"#icon-foo-bar\" /></svg>",
            output);
    }

    [Fact]
    public async Task PreservesExistingClasses()
    {
        var tagHelper = new IconTagHelper(HtmlEncoder.Default) { Name = "qux" };

        var output = await Render(tagHelper, [new("class", "baz")], TagMode.SelfClosing);

        Assert.Equal(
            "<svg class=\"baz icon icon--qux\" aria-hidden=\"true\"><use href=\"#icon-qux\" /></svg>",
            output);
    }

    private static async Task<string> Render(
        IconTagHelper tagHelper, TagHelperAttributeList attributes, TagMode initialTagMode)
    {
        var context = new TagHelperContext(
            "icon", attributes, new Dictionary<object, object>(), "test");

        var output = new TagHelperOutput(
            "icon",
            attributes,
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()))
        {
            TagMode = initialTagMode,
        };

        await tagHelper.ProcessAsync(context, output);

        using var writer = new StringWriter();
        output.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }
}
