using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;
using Xunit;

namespace Buttercup.Web.TagHelpers
{
    public class RequiredLabelTagHelperTests
    {
        [Theory]
        [InlineData(false, "initial-content")]
        [InlineData(true, "initial-content<span class=\"form-field__required-label\">required</span>")]
        public void AddsRequiredLabelWhenModelValueIsRequired(bool isRequired, string expected)
        {
            var mockMetadata = new Mock<ModelMetadata>(
                ModelMetadataIdentity.ForType(typeof(object)));
            mockMetadata.SetupGet(x => x.IsRequired).Returns(isRequired);

            var modelExplorer = new ModelExplorer(mockMetadata.Object, mockMetadata.Object, null);

            var tagHelper = new RequiredLabelTagHelper
            {
                For = new ModelExpression("SampleProperty", modelExplorer),
            };

            var context = new TagHelperContext(
                "label", new TagHelperAttributeList(), new Dictionary<object, object>(), "test");

            var output = new TagHelperOutput(
                "label", new TagHelperAttributeList(), GetChildContent);

            output.Content.Append("initial-content");

            tagHelper.Process(context, output);

            Assert.Equal(expected, output.Content.GetContent());
        }

        private static Task<TagHelperContent> GetChildContent(
            bool useCachedResult, HtmlEncoder encoder) =>
                Task.FromResult<TagHelperContent>(new DefaultTagHelperContent());
    }
}
