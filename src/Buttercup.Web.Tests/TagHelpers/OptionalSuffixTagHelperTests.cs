using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;
using Moq;
using Xunit;

namespace Buttercup.Web.TagHelpers;

public class OptionalSuffixTagHelperTests
{
    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void AddsOptionalSuffixWhenExpected(bool fieldIsRequired, bool optionalSuffixIsExpected)
    {
        var localizer = Mock.Of<IStringLocalizer<OptionalSuffixTagHelper>>(
            x => x["Label_Optional"] == new LocalizedString(string.Empty, "optional"));

        var mockMetadata = new Mock<ModelMetadata>(
            ModelMetadataIdentity.ForType(typeof(object)));
        mockMetadata.SetupGet(x => x.IsRequired).Returns(fieldIsRequired);

        var modelExplorer = new ModelExplorer(mockMetadata.Object, mockMetadata.Object, null);

        var tagHelper = new OptionalSuffixTagHelper(localizer)
        {
            For = new("SampleProperty", modelExplorer),
        };

        var context = new TagHelperContext(
            "label", new(), new Dictionary<object, object>(), "test");

        var output = new TagHelperOutput(
            "label",
            new(),
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        output.Content.Append("initial-content");

        tagHelper.Process(context, output);

        var expected = optionalSuffixIsExpected ?
            "initial-content<span class=\"form-field__optional-label\">optional</span>" :
            "initial-content";

        Assert.Equal(expected, output.Content.GetContent());
    }
}
