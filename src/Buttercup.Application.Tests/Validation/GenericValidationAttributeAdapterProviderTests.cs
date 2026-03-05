using System.ComponentModel.DataAnnotations;
using Buttercup.TestUtils;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Xunit;

namespace Buttercup.Application.Validation;

public sealed class GenericValidationAttributeAdapterProviderTests
{
    public static TheoryData<ValidationAttribute, Type> ExpectedAdapters => new()
    {
        { new RequiredAttribute(), typeof(RequiredAttributeAdapter) },
        { new TimeZoneAttribute(), typeof(GenericAttributeAdapter) },
    };

    [Theory]
    [MemberData(nameof(ExpectedAdapters))]
    public void GetAttributeAdapter_ReturnsExpectedAdapterType(
        ValidationAttribute attribute, Type expectedAdapterType)
    {
        var provider = new GenericValidationAttributeAdapterProvider();

        var adapter = provider.GetAttributeAdapter(attribute, null);

        Assert.NotNull(adapter);
        Assert.IsType(expectedAdapterType, adapter);
    }

    [Theory]
    [MemberData(nameof(ExpectedAdapters))]
    public void GetAttributeAdapter_PassesLocalizerToAdapter(ValidationAttribute attribute, Type _)
    {
        var provider = new GenericValidationAttributeAdapterProvider();
        var localizer = new DictionaryLocalizer<object>()
            .Add("ErrorMessageKey", "Localized error message");

        attribute.ErrorMessage = "ErrorMessageKey";

        var adapter = provider.GetAttributeAdapter(attribute, localizer);
        Assert.NotNull(adapter);

        var metadataProvider = new EmptyModelMetadataProvider();
        var metadata = metadataProvider.GetMetadataForType(typeof(object));
        var validationContext = new ModelValidationContext(
            new(), metadata, metadataProvider, null, null);

        Assert.Equal("Localized error message", adapter.GetErrorMessage(validationContext));
    }
}
