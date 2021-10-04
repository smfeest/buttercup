using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;
using Moq;
using Xunit;

namespace Buttercup.Web.TagHelpers
{
    public class RequiredLabelTagHelperTests
    {
        [Theory]
        [InlineData(null, false, false)]
        [InlineData(null, true, true)]
        [InlineData(false, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, false, true)]
        [InlineData(true, true, true)]
        public void AddsRequiredLabelWhenModelValueIsRequired(
            bool? showRequiredLabel, bool isRequired, bool requiredLabelExpected)
        {
            var localizer = Mock.Of<IStringLocalizer<RequiredLabelTagHelper>>(
                x => x["Label_Required"] == new LocalizedString(string.Empty, "required"));

            var mockMetadata = new Mock<ModelMetadata>(
                ModelMetadataIdentity.ForType(typeof(object)));
            mockMetadata.SetupGet(x => x.IsRequired).Returns(isRequired);

            var modelExplorer = new ModelExplorer(mockMetadata.Object, mockMetadata.Object, null);

            var tagHelper = new RequiredLabelTagHelper(localizer)
            {
                For = new("SampleProperty", modelExplorer),
                ShowRequiredLabel = showRequiredLabel,
            };

            var context = new TagHelperContext(
                "label", new(), new Dictionary<object, object>(), "test");

            var output = new TagHelperOutput("label", new(), GetChildContent);

            output.Content.Append("initial-content");

            tagHelper.Process(context, output);

            var expected = requiredLabelExpected ?
                "initial-content<span class=\"form-field__required-label\">required</span>" :
                "initial-content";

            Assert.Equal(expected, output.Content.GetContent());
        }

        private static Task<TagHelperContent> GetChildContent(
            bool useCachedResult, HtmlEncoder encoder) =>
                Task.FromResult<TagHelperContent>(new DefaultTagHelperContent());
    }
}
