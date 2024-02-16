using System.ComponentModel.DataAnnotations;
using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.Application.Validation;

public sealed class ValidationErrorLocalizerTests
{
    private readonly DictionaryLocalizer<object> stringLocalizer = new();
    private readonly ValidationErrorLocalizer<object> validationErrorLocalizer;
    private readonly ValidationContext validationContext = new(new()) { DisplayName = "Foo" };

    public ValidationErrorLocalizerTests() =>
        this.validationErrorLocalizer = new(this.stringLocalizer);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void FormatMessage_ErrorMessagePropertyNotSet_DelegatesToAttribute(string? errorMessage)
    {
        var attribute = new OddIntegerValidationAttribute() { ErrorMessage = errorMessage };

        Assert.Equal(
            "[unlocalized] Foo must be odd",
            this.validationErrorLocalizer.FormatMessage(this.validationContext, attribute));

    }

    [Fact]
    public void FormatMessage_RangeAttribute_IncludesDisplayNameMinAndMaxValues()
    {
        this.stringLocalizer.Add("Error_OutOfRange", "{0} must be between {1} and {2}");

        var attribute = new RangeAttribute(5, 10) { ErrorMessage = "Error_OutOfRange" };

        Assert.Equal(
            "Foo must be between 5 and 10",
            this.validationErrorLocalizer.FormatMessage(this.validationContext, attribute));
    }

    [Fact]
    public void FormatMessage_StringLengthAttribute_IncludesDisplayNameMinAndMaxLength()
    {
        this.stringLocalizer.Add(
            "Error_InvalidLength", "{0} must be between {2} and {1} characters long");

        var attribute = new StringLengthAttribute(20)
        {
            MinimumLength = 15,
            ErrorMessage = "Error_InvalidLength",
        };

        Assert.Equal(
            "Foo must be between 15 and 20 characters long",
            this.validationErrorLocalizer.FormatMessage(this.validationContext, attribute));
    }

    [Fact]
    public void FormatMessage_AnyOtherAttribute_IncludesDisplayName()
    {
        this.stringLocalizer.Add("Error_MustBeOdd", "[localized] {0} must be odd");

        var attribute = new OddIntegerValidationAttribute() { ErrorMessage = "Error_MustBeOdd" };

        Assert.Equal(
            "[localized] Foo must be odd",
            this.validationErrorLocalizer.FormatMessage(this.validationContext, attribute));
    }

    private sealed class OddIntegerValidationAttribute : ValidationAttribute
    {
        public override string FormatErrorMessage(string name) =>
            $"[unlocalized] {name} must be odd";
    }
}
