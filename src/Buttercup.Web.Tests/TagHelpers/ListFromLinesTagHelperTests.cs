using Microsoft.AspNetCore.Razor.TagHelpers;
using Xunit;

namespace Buttercup.Web.TagHelpers;

public sealed class ListFromLinesTagHelperTests
{
    [Theory]
    [InlineData(null)]
    [InlineData(" \n \t\n")]
    public void SuppressesOutputWhenListIsEmpty(string? input)
    {
        var output = Process(input);

        Assert.Null(output.TagName);
        Assert.True(output.Content.IsEmptyOrWhiteSpace);
    }

    [Fact]
    public void InsertsListItemForEachNonEmptyLine()
    {
        var output = Process(" \nAlpha\r\nBeta & Gamma\n  \nDelta");

        Assert.Equal("ul", output.TagName);
        Assert.Equal(
            "<li>Alpha</li><li>Beta &amp; Gamma</li><li>Delta</li>",
            output.Content.GetContent());
    }

    private static TagHelperOutput Process(string? input)
    {
        var output = new TagHelperOutput(
            "ul",
            [],
            (_, _) => Task.FromResult(new DefaultTagHelperContent().SetContent("content")));

        var context = new TagHelperContext("ul", [], new Dictionary<object, object>(), "test");

        var tagHelper = new ListFromLinesTagHelper { Input = input };

        tagHelper.Process(context, output);

        return output;
    }
}
