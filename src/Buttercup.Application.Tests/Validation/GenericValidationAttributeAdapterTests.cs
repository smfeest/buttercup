using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Buttercup.TestUtils;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;
using Xunit;

namespace Buttercup.Application.Validation;

public sealed class GenericValidationAttributeAdapterTests
{
    #region GetErrorMessage

    [Fact]
    public void GetErrorMessage_NoCustomMessage_ReturnsDefaultMessage() =>
        Assert.Equal("[base] Foo is invalid", GetErrorMessage(new(), null));

    [Theory]
    [InlineData("[custom] {0} is invalid", "[custom] Foo is invalid")]
    [InlineData("Error_Foo", "Error_Foo")]
    public void GetErrorMessage_CustomMessageNoLocalizer_ReturnsUnlocalizedCustomMessage(
        string customMessage, string expectedResult)
    {
        var attribute = new AlwaysInvalidAttribute { ErrorMessage = customMessage };

        Assert.Equal(expectedResult, GetErrorMessage(attribute, null));
    }

    [Theory]
    [InlineData("[custom] {0} is invalid", "[custom] Foo is invalid")]
    [InlineData("Error_Custom", "Error_Custom")]
    public void GetErrorMessage_CustomMessageNoMatchingResource_ReturnsUnlocalizedCustomMessage(
        string customMessage, string expectedResult)
    {
        var attribute = new AlwaysInvalidAttribute { ErrorMessage = customMessage };

        Assert.Equal(expectedResult, GetErrorMessage(attribute, new DictionaryLocalizer<object>()));
    }

    [Theory]
    [InlineData("[custom] {0} is invalid")]
    [InlineData("Error_Custom")]
    public void GetErrorMessage_CustomMessageMatchingResource_ReturnsLocalizedCustomMessage(
        string customMessage)
    {
        var localizer = new DictionaryLocalizer<object>()
            .Add(customMessage, "[localized] {0} is invalid");
        var attribute = new AlwaysInvalidAttribute { ErrorMessage = customMessage };

        Assert.Equal("[localized] Foo is invalid", GetErrorMessage(attribute, localizer));
    }

    private static string GetErrorMessage(
        AlwaysInvalidAttribute attribute, IStringLocalizer? localizer)
    {
        var adapter = new GenericAttributeAdapter(attribute, localizer);
        var metadataProvider = new EmptyModelMetadataProvider();
        var metadata = metadataProvider.GetMetadataForProperty(
            typeof(SampleModel), nameof(SampleModel.Foo));
        var validationContext = new ModelValidationContext(
            new(), metadata, metadataProvider, new SampleModel(), string.Empty);
        return adapter.GetErrorMessage(validationContext);
    }

    private sealed class AlwaysInvalidAttribute() : ValidationAttribute("[base] {0} is invalid")
    {
        public override string FormatErrorMessage(string name) =>
            string.Format(CultureInfo.CurrentCulture, this.ErrorMessageString, name);

        protected override ValidationResult? IsValid(
            object? value, ValidationContext validationContext) =>
            new(this.FormatErrorMessage(validationContext.DisplayName));
    }

    private sealed record SampleModel(string? Foo = "Bar");

    #endregion
}
